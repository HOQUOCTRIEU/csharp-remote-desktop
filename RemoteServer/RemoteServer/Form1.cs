using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RemoteServer.Models;

namespace RemoteServer
{
    public partial class Form1 : Form  // Đổi tên thành ServerForm nếu muốn
    {
        [DllImport("user32.dll")]
        
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public Form1()
        {
            InitializeComponent();
            txtPort.Text = "8080";  // Default port
            aesKey = new byte[32];  // Sẽ được tạo random sau khi trao đổi
            txtStatus.AppendText("Server ready. Press Start Server to begin.\r\n");
        }

        private TcpListener? listener;  // Nullable để tránh warning
        private X509Certificate2 serverCert = new X509Certificate2(@"C:\RemoteCert\server_cert.pfx", "123456@"); // Thay password thực tế

        private byte[] aesKey;  // Khóa AES, sẽ trao đổi với client

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, int.Parse(txtPort.Text));
                listener.Start();
                txtStatus.AppendText("Server started on port " + txtPort.Text + ".\r\n");
                _ = AcceptClientsAsync();  // Chạy async task
            }
            catch (Exception ex)
            {
                txtStatus.AppendText("Error starting server: " + ex.Message + "\r\n");
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                try
                {
                    if (listener == null) break;
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);  // Handle client async
                }
                catch (ObjectDisposedException) { break; }  // Khi stop server
                catch (Exception ex)
                {
                    txtStatus.AppendText("Error accepting client: " + ex.Message + "\r\n");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            string? clientIP = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown";
            using NetworkStream netStream = tcpClient.GetStream();
            using SslStream sslStream = new SslStream(netStream, false);
            try
            {
                await sslStream.AuthenticateAsServerAsync(serverCert);

                // Đọc LoginDto từ client
                byte[] buffer = new byte[4096];
                int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                LoginDto? dto = JsonConvert.DeserializeObject<LoginDto>(json);

                if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                {
                    throw new Exception("Invalid login data");
                }

                // Kiểm tra đăng nhập và lưu lịch sử
                using (var context = new AppDbContext())
                {
                    string hashedPassword = HashPassword(dto.Password);
                    var user = context.Users.FirstOrDefault(u => u.Username == dto.Username && u.PasswordHash == hashedPassword);
                    if (user != null)
                    {
                        context.Histories.Add(new ConnectionHistory
                        {
                            ClientIP = clientIP,
                            ConnectionTime = DateTime.Now,
                            UserId = user.Id
                        });
                        await context.SaveChangesAsync();

                        // Gửi success về client
                        string successJson = JsonConvert.SerializeObject(new { Success = true });
                        byte[] response = Encoding.UTF8.GetBytes(successJson);
                        await sslStream.WriteAsync(response, 0, response.Length);

                        // Trao đổi khóa AES bằng RSA
                        await ExchangeKeysAsync(sslStream);

                        // Tiếp tục session
                        await HandleRemoteSessionAsync(sslStream, tcpClient);
                    }
                    else
                    {
                        // Gửi thất bại
                        string failJson = JsonConvert.SerializeObject(new { Success = false, Message = "Invalid credentials" });
                        byte[] failResponse = Encoding.UTF8.GetBytes(failJson);
                        await sslStream.WriteAsync(failResponse, 0, failResponse.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"Error handling client {clientIP}: {ex.Message}\r\n");
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private async Task ExchangeKeysAsync(SslStream sslStream)
        {
            // Nhận public RSA từ Client
            byte[] buffer = new byte[4096];
            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            string keyJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            KeyExchangeDto? keyDto = JsonConvert.DeserializeObject<KeyExchangeDto>(keyJson);

            if (keyDto == null || string.IsNullOrEmpty(keyDto.PublicKeyXml))
            {
                throw new Exception("Invalid key exchange data");
            }

            // Tạo AES key random
            aesKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(aesKey);
            }

            // Mã hóa AES key bằng public RSA của Client
            using RSACryptoServiceProvider rsaServer = new RSACryptoServiceProvider();
            rsaServer.FromXmlString(keyDto.PublicKeyXml);
            byte[] encryptedAesKey = rsaServer.Encrypt(aesKey, false);  // Sử dụng OAEP nếu cần: true

            // Gửi về Client
            KeyExchangeDto responseDto = new KeyExchangeDto { EncryptedAesKey = encryptedAesKey };
            string responseJson = JsonConvert.SerializeObject(responseDto);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);

            txtStatus.AppendText("Key exchange completed.\r\n");
        }
        private async Task HandleRemoteSessionAsync(SslStream sslStream, TcpClient tcpClient)
        {
            try
            {
                while (true)
                {
                    // Gửi ảnh màn hình
                    byte[] imgBytes = GetScreenshotBytes();
                    byte[] encrypted = EncryptAES(imgBytes, aesKey!);
                    await SendMessageAsync(sslStream, encrypted);

                    // Nhận lệnh từ Client
                    byte[] cmdData = await ReceiveMessageAsync(sslStream);
                    if (cmdData.Length == 0) break;

                    // XỬ LÝ LỆNH AN TOÀN (không để crash session)
                    try
                    {
                        byte[] decrypted = DecryptAES(cmdData, aesKey!);
                        string json = Encoding.UTF8.GetString(decrypted);
                        CommandDto? cmd = JsonConvert.DeserializeObject<CommandDto>(json);

                        if (cmd != null)
                        {
                            switch (cmd.Type)
                            {
                                case "MouseClick":
                                    SimulateMouseClick(cmd.X, cmd.Y, cmd.Button ?? "Left");
                                    break;

                                case "KeyPress":
                                    SimulateKeyPress(cmd.KeyCode);
                                    break;

                                case "SendFile":
                                    {
                                        txtStatus.AppendText("Receiving file...\r\n");

                                        if (cmd.FileName != null && cmd.FileData != null)
                                        {
                                            ReceiveFileFromClient(cmd.FileName, cmd.FileData);
                                        }
                                        else
                                        {
                                            txtStatus.AppendText("File data invalid.\r\n");
                                        }
                                        txtStatus.AppendText("File name: " + cmd.FileName + "\r\n");
                                        txtStatus.AppendText("File size: " + cmd.FileData.Length + "\r\n");

                                        break;
                                    }
                            }
                        }
                    }
                    catch (Exception cmdEx)
                    {
                        txtStatus.AppendText("Lỗi xử lý lệnh chuột/phím: " + cmdEx.Message + "\r\n");
                        // KHÔNG đóng kết nối
                    }

                    await Task.Delay(30);
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText("Session lỗi: " + ex.Message + "\r\n");
            }
        }



        private async Task SendFileToClientAsync(SslStream sslStream, string fileName)
        {
            if (!File.Exists(fileName))
            {
                txtStatus.AppendText($"File không tồn tại: {fileName}\r\n");
                return;
            }

            byte[] fileData = File.ReadAllBytes(fileName);
            byte[] encrypted = EncryptAES(fileData, aesKey);
            CommandDto cmd = new CommandDto
            {
                Type = "FileData",
                FileName = fileName,
                FileData = encrypted
            };

            string json = JsonConvert.SerializeObject(cmd);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await SendMessageAsync(sslStream, data);

            txtStatus.AppendText($"Đã gửi file: {fileName}\r\n");
        }

        private void ReceiveFileFromClient(string fileName, byte[] fileData)
        {
            try
            {
                // Lưu vào Desktop của máy Server (dễ tìm nhất)
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string savePath = Path.Combine(desktopPath, "Received_" + fileName);

                File.WriteAllBytes(savePath, fileData);

                txtStatus.AppendText($"✅ File đã lưu tại Desktop:\n{savePath}\n");

                // Hiển thị đường dẫn trong RichTextBox (click mở được)
                rtbReceivedFiles.Invoke((Action)(() =>
                {
                    rtbReceivedFiles.SelectionColor = Color.Blue;
                    rtbReceivedFiles.AppendText(savePath + "\n");
                    rtbReceivedFiles.SelectionColor = Color.Black;
                    rtbReceivedFiles.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                txtStatus.AppendText("Lỗi lưu file: " + ex.Message + "\r\n");
            }
        }

        private async Task SendFileToClient(SslStream sslStream, string fileName)
        {
            if (File.Exists(fileName))
            {
                byte[] fileData = File.ReadAllBytes(fileName);
                byte[] encryptedFile = EncryptAES(fileData, aesKey);
                CommandDto fileCommand = new CommandDto { Type = "FileData", FileName = fileName, FileData = encryptedFile };
                string json = JsonConvert.SerializeObject(fileCommand);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await sslStream.WriteAsync(bytes, 0, bytes.Length);
                txtStatus.AppendText($"Sent file: {fileName}\r\n");
            }
            else
            {
                // Gửi lỗi
            }
        }

        private Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return bmp;
        }

        private byte[] GetScreenshotBytes()
        {
            Bitmap screenshot = CaptureScreen();
            using MemoryStream ms = new MemoryStream();
            screenshot.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }

        private byte[] EncryptAES(byte[] data, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = new byte[16];           // IV cố định (đơn giản & ổn định)
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
            aes.IV = new byte[16];
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            using MemoryStream ms = new MemoryStream(encryptedData);
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }

        // P/Invoke để simulate input
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private void SimulateMouseClick(int x, int y, string button)
        {
            Cursor.Position = new Point(x, y);

            if (button == "Left")
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (button == "Right")
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
        }

        private void SimulateKeyPress(int keyCode)
        {
            Keys key = (Keys)keyCode;

            switch (key)
            {
                // Phím đặc biệt quan trọng
                case Keys.Back: SendKeys.SendWait("{BACKSPACE}"); break;
                case Keys.Delete: SendKeys.SendWait("{DELETE}"); break;
                case Keys.Enter: SendKeys.SendWait("{ENTER}"); break;
                case Keys.Space: SendKeys.SendWait(" "); break;
                case Keys.Tab: SendKeys.SendWait("{TAB}"); break;
                case Keys.Escape: SendKeys.SendWait("{ESC}"); break;

                // Mũi tên
                case Keys.Left: SendKeys.SendWait("{LEFT}"); break;
                case Keys.Right: SendKeys.SendWait("{RIGHT}"); break;
                case Keys.Up: SendKeys.SendWait("{UP}"); break;
                case Keys.Down: SendKeys.SendWait("{DOWN}"); break;

                // Home, End, Page
                case Keys.Home: SendKeys.SendWait("{HOME}"); break;
                case Keys.End: SendKeys.SendWait("{END}"); break;
                case Keys.PageUp: SendKeys.SendWait("{PGUP}"); break;
                case Keys.PageDown: SendKeys.SendWait("{PGDN}"); break;

                // Function keys
                case Keys.F1: SendKeys.SendWait("{F1}"); break;
                case Keys.F2: SendKeys.SendWait("{F2}"); break;
                case Keys.F3: SendKeys.SendWait("{F3}"); break;
                case Keys.F4: SendKeys.SendWait("{F4}"); break;
                case Keys.F5: SendKeys.SendWait("{F5}"); break;
                case Keys.F6: SendKeys.SendWait("{F6}"); break;
                case Keys.F7: SendKeys.SendWait("{F7}"); break;
                case Keys.F8: SendKeys.SendWait("{F8}"); break;
                case Keys.F9: SendKeys.SendWait("{F9}"); break;
                case Keys.F10: SendKeys.SendWait("{F10}"); break;
                case Keys.F11: SendKeys.SendWait("{F11}"); break;
                case Keys.F12: SendKeys.SendWait("{F12}"); break;

                // Ký tự đặc biệt (đã sửa OemComma, OemPlus)
                case Keys.Oemcomma: SendKeys.SendWait(","); break;
                case Keys.OemPeriod: SendKeys.SendWait("."); break;
                case Keys.OemMinus: SendKeys.SendWait("-"); break;
                case Keys.Oemplus: SendKeys.SendWait("+"); break;
                case Keys.OemSemicolon: SendKeys.SendWait(";"); break;
                case Keys.OemQuotes: SendKeys.SendWait("'"); break;
                case Keys.OemOpenBrackets: SendKeys.SendWait("["); break;
                case Keys.OemCloseBrackets: SendKeys.SendWait("]"); break;
                case Keys.OemBackslash: SendKeys.SendWait("\\"); break;

                // Chữ cái và số (mặc định)
                default:
                    if ((key >= Keys.A && key <= Keys.Z) ||
                        (key >= Keys.D0 && key <= Keys.D9))
                    {
                        SendKeys.SendWait(key.ToString());
                    }
                    else
                    {
                        // Các phím khác chuyển thành ký tự
                        SendKeys.SendWait(((char)key).ToString());
                    }
                    break;
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
            await ReadExactlyAsync(sslStream, lengthBuf, 4);

            int length = BitConverter.ToInt32(lengthBuf, 0);

            byte[] data = new byte[length];
            await ReadExactlyAsync(sslStream, data, length);

            return data;
        }

        private async Task ReadExactlyAsync(SslStream stream, byte[] buffer, int length)
        {
            int offset = 0;

            while (offset < length)
            {
                int read = await stream.ReadAsync(buffer, offset, length - offset);

                if (read == 0)
                    throw new Exception("Connection closed");

                offset += read;
            }
        }

        private async Task<byte[]> ReadExactlyAsync(SslStream sslStream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int read = await sslStream.ReadAsync(buffer, offset, count - offset);
                if (read == 0) throw new Exception("Disconnected");

                offset += read;
            }

            return buffer;
        }
        private void btnStopServer_Click(object sender, EventArgs e)
        {
            try
            {
                listener?.Stop();
                txtStatus.AppendText("Server stopped.\r\n");
            }
            catch (Exception ex)
            {
                txtStatus.AppendText("Error stopping server: " + ex.Message + "\r\n");
            }
        }
    }
}

// Thêm class DTO nếu chưa có (trong Models)
public class LoginDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class CommandDto
{
    public string? Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string? Button { get; set; }        // ← thêm ?
    public int KeyCode { get; set; }
    public string? FileName { get; set; }      // ← thêm ?
    public byte[]? FileData { get; set; }      // ← thêm ?
}

public class KeyExchangeDto
{
    public string? PublicKeyXml { get; set; }
    public byte[]? EncryptedAesKey { get; set; }
}