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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
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
            consoleOutput.HideSelection = false;
            consoleOutput.Location = new Point(12, 12);
            consoleOutput.Name = "consoleOutput";
            consoleOutput.ReadOnly = true;
            consoleOutput.Size = new Size(1262, 591);
            consoleOutput.TabIndex = 0;
            consoleOutput.Text = "";
            consoleOutput.TextChanged += consoleOutput_TextChanged;
            // 
            // commandBox
            // 
            commandBox.BackColor = Color.FromArgb(75, 73, 89);
            commandBox.ForeColor = Color.MistyRose;
            commandBox.Location = new Point(12, 610);
            commandBox.Name = "commandBox";
            commandBox.PlaceholderText = "Command..";
            commandBox.Size = new Size(1181, 23);
            commandBox.TabIndex = 1;
            commandBox.TextChanged += commandBox_TextChanged;
            // 
            // sendButton
            // 
            sendButton.Location = new Point(1199, 609);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(75, 23);
            sendButton.TabIndex = 2;
            sendButton.Text = "Send >>";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += button1_Click;
            // 
            // Main
            // 
            AcceptButton = sendButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(43, 42, 51);
            ClientSize = new Size(1286, 645);
            Controls.Add(sendButton);
            Controls.Add(commandBox);
            Controls.Add(consoleOutput);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            Text = "Globulator";
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
