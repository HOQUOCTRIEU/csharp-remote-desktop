using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RemoteClient
{
    public partial class Form1 : Form
    {
        private byte[]? aesKey;
        private RSACryptoServiceProvider rsaClient = new RSACryptoServiceProvider(2048);
        private int remoteWidth = Screen.PrimaryScreen.Bounds.Width;
        private int remoteHeight = Screen.PrimaryScreen.Bounds.Height;

        public Form1()
        {
            InitializeComponent();
            txtServerIP.Text = "26.164.149.235";
            txtServerPort.Text = "8080";

            this.KeyPreview = true;          // ← THÊM DÒNG NÀY
        }

        private TcpClient? tcpClient;
        private SslStream? sslStream;

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(txtServerIP.Text, int.Parse(txtServerPort.Text));

                var netStream = tcpClient.GetStream();
                sslStream = new SslStream(netStream, false, ValidateServerCertificate);
                await sslStream.AuthenticateAsClientAsync(txtServerIP.Text);

                var dto = new LoginDto { Username = txtUsername.Text ?? "", Password = txtPassword.Text ?? "" };
                string json = JsonConvert.SerializeObject(dto);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await sslStream.WriteAsync(bytes, 0, bytes.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = JsonConvert.DeserializeObject<dynamic>(responseJson);

                if (response?.Success == true)
                {
                    await ExchangeKeysAsync(sslStream);
                    tabControl.SelectedTab = tabRemote;

                    // ← THÊM 2 DÒNG NÀY
                    picRemoteScreen.Focus();
                    picRemoteScreen.TabStop = true;

                    _ = Task.Run(() => HandleRemoteSessionAsync(sslStream));
                }
                else
                {
                    MessageBox.Show("Đăng nhập thất bại!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // ==================== TRAO ĐỔI KHÓA AES ====================
        private async Task ExchangeKeysAsync(SslStream sslStream)
        {
            string publicKey = rsaClient.ToXmlString(false);
            var keyDto = new KeyExchangeDto { PublicKeyXml = publicKey };
            string json = JsonConvert.SerializeObject(keyDto);
            await sslStream.WriteAsync(Encoding.UTF8.GetBytes(json), 0, json.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            var received = JsonConvert.DeserializeObject<KeyExchangeDto>(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            aesKey = rsaClient.Decrypt(received?.EncryptedAesKey ?? Array.Empty<byte>(), false);
        }

        private async Task<byte[]> ReadExactlyAsync(SslStream sslStream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await sslStream.ReadAsync(buffer, offset, count - offset);
                if (read == 0) throw new Exception("Kết nối bị ngắt");
                offset += read;
            }
            return buffer;
        }
        // ==================== NHẬN ẢNH ====================
        private async Task HandleRemoteSessionAsync(SslStream sslStream)
        {
            try
            {
                while (true)
                {
                    // Đọc kích thước gói tin
                    byte[] lengthBuf = await ReadExactlyAsync(sslStream, 4);
                    int length = BitConverter.ToInt32(lengthBuf, 0);
                    if (length <= 0) break;

                    // Đọc ĐÚNG đủ byte dữ liệu (quan trọng nhất)
                    byte[] encryptedData = await ReadExactlyAsync(sslStream, length);

                    // Giải mã AES
                    byte[] decrypted = DecryptAES(encryptedData, aesKey!);

                    using MemoryStream ms = new MemoryStream(decrypted);
                    var img = Image.FromStream(ms);
                    picRemoteScreen.Invoke((Action)(() => picRemoteScreen.Image = img));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nhận ảnh: " + ex.Message);
            }
        }

        private async Task SendMessageAsync(SslStream sslStream, byte[] data)
        {
            byte[] length = BitConverter.GetBytes(data.Length);
            await sslStream.WriteAsync(length, 0, 4);
            await sslStream.WriteAsync(data, 0, data.Length);
        }

        private async Task<byte[]> ReceiveMessageAsync(SslStream sslStream)
        {
            byte[] lengthBuf = new byte[4];
            await sslStream.ReadAsync(lengthBuf, 0, 4);
            int length = BitConverter.ToInt32(lengthBuf, 0);
            byte[] data = new byte[length];
            await sslStream.ReadAsync(data, 0, length);
            return data;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;

        // ==================== GỬI LỆNH ====================
        private void picRemoteScreen_MouseClick(object sender, MouseEventArgs e)
        {
            if (sslStream == null || aesKey == null) return;
            var cmd = new CommandDto
            {
                Type = "MouseClick",
                X = (int)(e.X * remoteWidth / picRemoteScreen.Width),
                Y = (int)(e.Y * remoteHeight / picRemoteScreen.Height),
                Button = e.Button.ToString()
            };
            SendCommand(cmd);
        }

        private void picRemoteScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (sslStream == null || aesKey == null) return;

            var cmd = new CommandDto
            {
                Type = "KeyPress",
                KeyCode = (int)e.KeyCode
            };
            SendCommand(cmd);
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (sslStream == null || aesKey == null) return;

            using var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var cmd = new CommandDto
                {
                    Type = "SendFile",
                    FileName = Path.GetFileName(dlg.FileName),
                    FileData = File.ReadAllBytes(dlg.FileName)
                };

                SendCommand(cmd);
            }
        }

        private async void btnReceiveFile_Click(object sender, EventArgs e)
        {
            if (sslStream == null || aesKey == null)
            {
                MessageBox.Show("Chưa kết nối với Server!");
                return;
            }

            string fileName = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên file cần nhận từ Server:", "Nhận File");

            if (string.IsNullOrEmpty(fileName)) return;

            try
            {
                var cmd = new CommandDto { Type = "RequestFile", FileName = fileName };
                SendCommand(cmd);

                // Nhận file về và lưu vào Desktop
                await ReceiveFileFromServerAsync(fileName);

                MessageBox.Show($"✅ Đã nhận file {fileName} và lưu vào Desktop!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nhận file: " + ex.Message);
            }
        }

        private async Task ReceiveFileFromServerAsync(string fileName)
        {
            byte[] fileData = await ReceiveMessageAsync(sslStream!);   // Nhận dữ liệu file
            if (fileData.Length == 0) return;

            byte[] decrypted = DecryptAES(fileData, aesKey!);

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string savePath = Path.Combine(desktop, "Received_FromServer_" + fileName);

            File.WriteAllBytes(savePath, decrypted);
        }

        private void SendCommand(CommandDto cmd)
        {
            if (sslStream == null || aesKey == null) return;

            try
            {
                string json = JsonConvert.SerializeObject(cmd);
                byte[] data = Encoding.UTF8.GetBytes(json);

                byte[] encrypted = EncryptAES(data, aesKey);

                byte[] length = BitConverter.GetBytes(encrypted.Length);
                sslStream.Write(length, 0, 4);
                sslStream.Write(encrypted, 0, encrypted.Length);
            }
            catch { }
        }

        private byte[] EncryptAES(byte[] data, byte[] key)
        {
            if (key == null || data == null) return Array.Empty<byte>();

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = new byte[16];
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        private byte[] DecryptAES(byte[] encryptedData, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = new byte[16];                    // IV cố định (khớp Server)
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            using MemoryStream ms = new MemoryStream(encryptedData);
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }

        public class LoginDto { public string? Username { get; set; } public string? Password { get; set; } }
        public class CommandDto { public string? Type { get; set; } public int X { get; set; } public int Y { get; set; } public string? Button { get; set; } public int KeyCode { get; set; } public string? FileName { get; set; } public byte[]? FileData { get; set; } }
        public class KeyExchangeDto { public string? PublicKeyXml { get; set; } public byte[]? EncryptedAesKey { get; set; } }

        private void picRemoteScreen_Click(object sender, EventArgs e)
        {

        }

        private void picRemoteScreen_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (sslStream == null || aesKey == null) return;
            var cmd = new CommandDto
            {
                Type = "MouseClick",
                X = (int)(e.X * remoteWidth / picRemoteScreen.Width),
                Y = (int)(e.Y * remoteHeight / picRemoteScreen.Height),
                Button = e.Button.ToString()
            };
            SendCommand(cmd);
        }

        private void Form1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (sslStream == null || aesKey == null) return;

            var cmd = new CommandDto
            {
                Type = "KeyPress",
                KeyCode = (int)e.KeyCode
            };
            SendCommand(cmd);

            // Ngăn Windows hiển thị chữ "back", "space"... trên Client
            e.SuppressKeyPress = true;
        }
    }
}