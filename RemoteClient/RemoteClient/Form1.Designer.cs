namespace RemoteClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabLogin = new TabPage();
            label4 = new Label();
            label3 = new Label();
            txtServerPort = new TextBox();
            txtServerIP = new TextBox();
            btnLogin = new Button();
            txtPassword = new TextBox();
            label2 = new Label();
            txtUsername = new TextBox();
            label1 = new Label();
            tabRemote = new TabPage();
            panel1 = new Panel();
            btnReceiveFile = new Button();
            btnSendFile = new Button();
            picRemoteScreen = new PictureBox();
            tabControl.SuspendLayout();
            tabLogin.SuspendLayout();
            tabRemote.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picRemoteScreen).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabLogin);
            tabControl.Controls.Add(tabRemote);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(800, 450);
            tabControl.TabIndex = 0;
            // 
            // tabLogin
            // 
            tabLogin.Controls.Add(label4);
            tabLogin.Controls.Add(label3);
            tabLogin.Controls.Add(txtServerPort);
            tabLogin.Controls.Add(txtServerIP);
            tabLogin.Controls.Add(btnLogin);
            tabLogin.Controls.Add(txtPassword);
            tabLogin.Controls.Add(label2);
            tabLogin.Controls.Add(txtUsername);
            tabLogin.Controls.Add(label1);
            tabLogin.Location = new Point(4, 29);
            tabLogin.Name = "tabLogin";
            tabLogin.Padding = new Padding(3);
            tabLogin.Size = new Size(792, 417);
            tabLogin.TabIndex = 0;
            tabLogin.Text = "Login";
            tabLogin.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(341, 72);
            label4.Name = "label4";
            label4.Size = new Size(38, 20);
            label4.TabIndex = 8;
            label4.Text = "Port:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(341, 26);
            label3.Name = "label3";
            label3.Size = new Size(24, 20);
            label3.TabIndex = 7;
            label3.Text = "IP:";
            // 
            // txtServerPort
            // 
            txtServerPort.Location = new Point(428, 69);
            txtServerPort.Name = "txtServerPort";
            txtServerPort.Size = new Size(125, 27);
            txtServerPort.TabIndex = 6;
            txtServerPort.Text = "8080";
            // 
            // txtServerIP
            // 
            txtServerIP.Location = new Point(428, 23);
            txtServerIP.Name = "txtServerIP";
            txtServerIP.Size = new Size(125, 27);
            txtServerIP.TabIndex = 5;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(18, 143);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(94, 29);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(113, 69);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(125, 27);
            txtPassword.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 72);
            label2.Name = "label2";
            label2.Size = new Size(73, 20);
            label2.TabIndex = 2;
            label2.Text = "Password:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(113, 23);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(125, 27);
            txtUsername.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 26);
            label1.Name = "label1";
            label1.Size = new Size(85, 20);
            label1.TabIndex = 0;
            label1.Text = "User Name:";
            // 
            // tabRemote
            // 
            tabRemote.Controls.Add(panel1);
            tabRemote.Controls.Add(picRemoteScreen);
            tabRemote.Location = new Point(4, 29);
            tabRemote.Name = "tabRemote";
            tabRemote.Padding = new Padding(3);
            tabRemote.Size = new Size(792, 417);
            tabRemote.TabIndex = 1;
            tabRemote.Text = "Remote Control";
            tabRemote.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnReceiveFile);
            panel1.Controls.Add(btnSendFile);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(3, 368);
            panel1.Name = "panel1";
            panel1.Size = new Size(786, 46);
            panel1.TabIndex = 3;
            // 
            // btnReceiveFile
            // 
            btnReceiveFile.Location = new Point(124, 3);
            btnReceiveFile.Name = "btnReceiveFile";
            btnReceiveFile.Size = new Size(100, 29);
            btnReceiveFile.TabIndex = 2;
            btnReceiveFile.Text = "Receive File";
            btnReceiveFile.UseVisualStyleBackColor = true;
            btnReceiveFile.Click += btnReceiveFile_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(5, 3);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(94, 29);
            btnSendFile.TabIndex = 1;
            btnSendFile.Text = "Send File";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // picRemoteScreen
            // 
            picRemoteScreen.Dock = DockStyle.Fill;
            picRemoteScreen.Location = new Point(3, 3);
            picRemoteScreen.Name = "picRemoteScreen";
            picRemoteScreen.Size = new Size(786, 411);
            picRemoteScreen.SizeMode = PictureBoxSizeMode.Zoom;
            picRemoteScreen.TabIndex = 0;
            picRemoteScreen.TabStop = false;
            picRemoteScreen.Click += picRemoteScreen_Click;
            picRemoteScreen.MouseClick += picRemoteScreen_MouseClick_1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabControl);
            Name = "Form1";
            Text = "ClientForm";
            KeyDown += Form1_KeyDown_1;
            tabControl.ResumeLayout(false);
            tabLogin.ResumeLayout(false);
            tabLogin.PerformLayout();
            tabRemote.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picRemoteScreen).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabLogin;
        private Label label2;
        private TextBox txtUsername;
        private Label label1;
        private TabPage tabRemote;
        private TextBox txtPassword;
        private TextBox txtServerIP;
        private Button btnLogin;
        private Label label4;
        private Label label3;
        private TextBox txtServerPort;
        private Button btnReceiveFile;
        private Button btnSendFile;
        private PictureBox picRemoteScreen;
        private Panel panel1;
    }
}
