namespace vault_thing
{
    partial class Main
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
            consoleOutput = new RichTextBox();
            commandBox = new TextBox();
            sendButton = new Button();
            SuspendLayout();
            // 
            // consoleOutput
            // 
            consoleOutput.BackColor = Color.FromArgb(28, 27, 34);
            consoleOutput.Font = new Font("Cascadia Code", 9F);
            consoleOutput.ForeColor = Color.MistyRose;
            consoleOutput.Location = new Point(24, 24);
            consoleOutput.Margin = new Padding(6);
            consoleOutput.Name = "consoleOutput";
            consoleOutput.ReadOnly = true;
            consoleOutput.Size = new Size(2520, 1178);
            consoleOutput.TabIndex = 0;
            consoleOutput.Text = "";
            consoleOutput.TextChanged += consoleOutput_TextChanged;
            // 
            // commandBox
            // 
            commandBox.BackColor = Color.FromArgb(75, 73, 89);
            commandBox.ForeColor = Color.MistyRose;
            commandBox.Location = new Point(24, 1220);
            commandBox.Margin = new Padding(6);
            commandBox.Name = "commandBox";
            commandBox.PlaceholderText = "Command..";
            commandBox.Size = new Size(2358, 39);
            commandBox.TabIndex = 1;
            commandBox.TextChanged += commandBox_TextChanged;
            // 
            // sendButton
            // 
            sendButton.Location = new Point(2398, 1218);
            sendButton.Margin = new Padding(6);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(150, 46);
            sendButton.TabIndex = 2;
            sendButton.Text = "Send >>";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += button1_Click;
            // 
            // Main
            // 
            AcceptButton = sendButton;
            AutoScaleDimensions = new SizeF(192F, 192F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(43, 42, 51);
            ClientSize = new Size(2572, 1290);
            Controls.Add(sendButton);
            Controls.Add(commandBox);
            Controls.Add(consoleOutput);
            Margin = new Padding(6);
            Name = "Main";
            Text = "Vault";
            Load += Main_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox consoleOutput;
        private TextBox commandBox;
        private Button sendButton;
    }
}
