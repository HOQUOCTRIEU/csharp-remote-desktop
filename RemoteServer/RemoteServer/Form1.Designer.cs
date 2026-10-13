namespace RemoteServer
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
            label1 = new Label();
            txtStatus = new TextBox();
            btnStartServer = new Button();
            btnStopServer = new Button();
            label2 = new Label();
            txtPort = new TextBox();
            rtbReceivedFiles = new RichTextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 36);
            label1.Name = "label1";
            label1.Size = new Size(97, 20);
            label1.TabIndex = 0;
            label1.Text = "Server Status:";
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(118, 33);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(125, 77);
            txtStatus.TabIndex = 1;
            // 
            // btnStartServer
            // 
            btnStartServer.Location = new Point(15, 192);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(94, 29);
            btnStartServer.TabIndex = 2;
            btnStartServer.Text = "Start Server";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += btnStartServer_Click;
            // 
            // btnStopServer
            // 
            btnStopServer.Location = new Point(149, 192);
            btnStopServer.Name = "btnStopServer";
            btnStopServer.Size = new Size(94, 29);
            btnStopServer.TabIndex = 3;
            btnStopServer.Text = "Stop Server";
            btnStopServer.UseVisualStyleBackColor = true;
            btnStopServer.Click += btnStopServer_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 123);
            label2.Name = "label2";
            label2.Size = new Size(38, 20);
            label2.TabIndex = 4;
            label2.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(118, 116);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(125, 27);
            txtPort.TabIndex = 5;
            txtPort.Text = "8080";
            // 
            // rtbReceivedFiles
            // 
            rtbReceivedFiles.Location = new Point(249, 33);
            rtbReceivedFiles.Name = "rtbReceivedFiles";
            rtbReceivedFiles.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbReceivedFiles.Size = new Size(511, 110);
            rtbReceivedFiles.TabIndex = 7;
            rtbReceivedFiles.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(rtbReceivedFiles);
            Controls.Add(txtPort);
            Controls.Add(label2);
            Controls.Add(btnStopServer);
            Controls.Add(btnStartServer);
            Controls.Add(txtStatus);
            Controls.Add(label1);
            Name = "Form1";
            Text = "ServerForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtStatus;
        private Button btnStartServer;
        private Button btnStopServer;
        private Label label2;
        private TextBox txtPort;
        private RichTextBox rtbReceivedFiles;
    }
}
