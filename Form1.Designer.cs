namespace vault_thing;

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
        pictureBox1 = new PictureBox();
        viewingBox = new RichTextBox();
        viewedFilePath = new Label();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
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
        consoleOutput.Size = new Size(749, 487);
        consoleOutput.TabIndex = 1;
        consoleOutput.Text = "";
        consoleOutput.TextChanged += consoleOutput_TextChanged;
        // 
        // commandBox
        // 
        commandBox.BackColor = Color.FromArgb(75, 73, 89);
        commandBox.ForeColor = Color.MistyRose;
        commandBox.Location = new Point(12, 505);
        commandBox.Name = "commandBox";
        commandBox.PlaceholderText = "Command..";
        commandBox.Size = new Size(669, 23);
        commandBox.TabIndex = 1;
        commandBox.TextChanged += commandBox_TextChanged;
        // 
        // sendButton
        // 
        sendButton.Location = new Point(687, 505);
        sendButton.Name = "sendButton";
        sendButton.Size = new Size(75, 23);
        sendButton.TabIndex = 0;
        sendButton.Text = "Send >>";
        sendButton.UseVisualStyleBackColor = true;
        sendButton.Click += button1_Click;
        // 
        // pictureBox1
        // 
        pictureBox1.Location = new Point(768, 12);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(545, 487);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 3;
        pictureBox1.TabStop = false;
        pictureBox1.Click += pictureBox1_Click;
        // 
        // viewingBox
        // 
        viewingBox.AcceptsTab = true;
        viewingBox.BackColor = Color.FromArgb(28, 27, 34);
        viewingBox.Font = new Font("Cascadia Code", 9F);
        viewingBox.ForeColor = Color.MistyRose;
        viewingBox.HideSelection = false;
        viewingBox.Location = new Point(768, 13);
        viewingBox.Name = "viewingBox";
        viewingBox.Size = new Size(545, 486);
        viewingBox.TabIndex = 4;
        viewingBox.TabStop = false;
        viewingBox.Text = "";
        viewingBox.TextChanged += viewingBox_TextChanged;
        // 
        // viewedFilePath
        // 
        viewedFilePath.ForeColor = Color.MistyRose;
        viewedFilePath.Location = new Point(768, 508);
        viewedFilePath.Name = "viewedFilePath";
        viewedFilePath.Size = new Size(542, 20);
        viewedFilePath.TabIndex = 5;
        viewedFilePath.Text = "<filePath>";
        viewedFilePath.Click += viewedFilePath_Click;
        // 
        // Main
        // 
        AcceptButton = sendButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(43, 42, 51);
        BackgroundImageLayout = ImageLayout.Zoom;
        ClientSize = new Size(1325, 535);
        Controls.Add(viewedFilePath);
        Controls.Add(viewingBox);
        Controls.Add(pictureBox1);
        Controls.Add(sendButton);
        Controls.Add(commandBox);
        Controls.Add(consoleOutput);
        ForeColor = Color.Transparent;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Main";
        Text = "Globulator";
        Load += Main_Load;
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private RichTextBox consoleOutput;
    private TextBox commandBox;
    private Button sendButton;
    private PictureBox pictureBox1;
    private RichTextBox viewingBox;
    private Label viewedFilePath;
}
