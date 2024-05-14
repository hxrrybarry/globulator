using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Drawing;
using WMPLib;
using System.Drawing.Imaging;

namespace vault_thing;

public partial class Main : Form
{
    private delegate void ImageDelegate(string path);
    private delegate void TextDelegate(string text, string path);
    private static Main? form = null;

    private int fileIndex = 0;

    private Size formOriginalSize;
    private Rectangle recSendButton;
    private Rectangle recCommandBox;
    private Rectangle recConsoleBox;
    private Rectangle recViewingBox;
    private Rectangle recPictureBox;
    private Rectangle recPathLabel;

    public Main()
    {
        InitializeComponent();

        this.Resize += Main_Resiz;
        formOriginalSize = this.Size;
        recSendButton = new Rectangle(sendButton.Location, sendButton.Size);
        recCommandBox = new Rectangle(commandBox.Location, commandBox.Size);
        recConsoleBox = new Rectangle(consoleOutput.Location, consoleOutput.Size);
        recViewingBox = new Rectangle(viewingBox.Location, viewingBox.Size);
        recPictureBox = new Rectangle(pictureBox1.Location, pictureBox1.Size);
        recPathLabel = new Rectangle(viewedFilePath.Location, viewedFilePath.Size);

        commandBox.Multiline = true;
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    private void Main_Resiz(object sender, EventArgs e)
    {
        ResizeControl(sendButton, recSendButton);
        ResizeControl(commandBox, recCommandBox);
        ResizeControl(consoleOutput, recConsoleBox);
        ResizeControl(viewingBox, recViewingBox);
        ResizeControl(pictureBox1, recPictureBox);
        ResizeControl(viewedFilePath, recPathLabel);
    }

    private void ResizeControl(Control c, Rectangle r)
    {
        float xRatio = (float)(this.Width) / (float)(formOriginalSize.Width);
        float yRatio = (float)(this.Height) / (float)(formOriginalSize.Height);
        int newX = (int)(r.X * xRatio);
        int newY = (int)(r.Y * yRatio);

        int newWidth = (int)(r.Width * xRatio);
        int newHeight = (int)(r.Height * yRatio);

        c.Location = new Point(newX, newY);
        c.Size = new Size(newWidth, newHeight);
    }

    CommandConsole console;

    readonly WindowsMediaPlayer notifySFX = new()
    {
        URL = "sfx/notify.mp3"
    };
    readonly WindowsMediaPlayer successSFX = new()
    {
        URL = "sfx/success.mp3"
    };
    readonly WindowsMediaPlayer errorSFX = new()
    {
        URL = "sfx/error.mp3"
    };
    
    private void Main_Load(object sender, EventArgs e)
    {
        successSFX.controls.stop();
        errorSFX.controls.stop();

        form = this;

        console = new();
        consoleOutput.AppendText($"{DateTime.Now:HH:mm:ss}; ", Color.Magenta);
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText(">> Program booted..\n", Color.Green);

        viewedFilePath.Text = console.CurrentDirectory;

        if (console.isMuted)
            notifySFX.controls.stop();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        consoleOutput.AppendText($"{DateTime.Now:HH:mm:ss}; ", Color.Magenta);
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText($"<< {commandBox.Text}", Color.Cyan);
        (string, Color) response = console.ProcessCommand(commandBox.Text, (viewingBox.Text, viewedFilePath.Text));
        commandBox.Clear();

        string responseText = response.Item1;
        Color responseColour = response.Item2;

        if (!console.isMuted)
        {
            if (responseColour == Color.Red)
                errorSFX.controls.play();
            else if (responseColour == Color.Green)
                successSFX.controls.play();
            else
                notifySFX.controls.play();
        }

        consoleOutput.AppendText($"\n{DateTime.Now:HH:mm:ss}; ", Color.Magenta);
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText($">> {responseText}\n", responseColour);

        this.AcceptButton = null;
        console.AllFilesInCurrentPath = Directory.GetFiles(console.CurrentDirectory);
    }

    private void commandBox_TextChanged(object sender, EventArgs e)
    {
        this.AcceptButton = sendButton;
    }

    private void consoleOutput_TextChanged(object sender, EventArgs e)
    {
        
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {

    }

    public static void SetPictureBoxImage(string path)
    {
        form?.SetImage(path);
    }

    private void SetImage(string path)
    {
        // if this returns true, it means it was called from an external thread.
        if (InvokeRequired)
        {
            // create a delegate of this method and let the form run it
            Invoke(new ImageDelegate(SetImage), [path]);
            return; // important
        }



        // set image and disable text box
        pictureBox1.ImageLocation = path;
        pictureBox1.Visible = true;
        viewingBox.Visible = false;   
        viewedFilePath.Text = path;
    }

    private void viewingBox_TextChanged(object sender, EventArgs e)
    {
        this.AcceptButton = null;
    }

    public static void SetViewingBoxText(string text, string path)
    {
        form?.SetText(text, path);
    }

    private void SetText(string text, string path)
    {
        // if this returns true, it means it was called from an external thread
        if (InvokeRequired)
        {
            // create a delegate of this method and let the form run it
            Invoke(new TextDelegate(SetText), [text, path]);
            return; // important
        }

        // set text and disable picture box
        viewingBox.Text = text;
        viewingBox.Visible = true;
        pictureBox1.Visible = false;    
        viewedFilePath.Text = path;
    }

    private void viewedFilePath_Click(object sender, EventArgs e)
    {

    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // need to check if it's these calls at all, otherwise it will set the default accept button to null on any key
        // this will prevent a user from pressing return to enter a command
        if (keyData == Keys.F11 || keyData == Keys.F12)
        {
            if (keyData == Keys.F11 && fileIndex != 0)
                fileIndex--;

            else if (keyData == Keys.F12 && fileIndex < console.AllFilesInCurrentPath.Length - 1)
                fileIndex++;

            try {

                string path = console.AllFilesInCurrentPath[fileIndex];
                if (path.EndsWith("png") || path.EndsWith("jpg") || path.EndsWith("jpeg") || path.EndsWith("bmp") || path.EndsWith("gif"))
                    SetPictureBoxImage(path);
                else
                    SetViewingBoxText(File.ReadAllText(path), path);
            } catch (IndexOutOfRangeException) {
                SetViewingBoxText("No files in selected directory.", console.CurrentDirectory);
            }
        }
                      
        return base.ProcessCmdKey(ref msg, keyData);
    }
}

public static class RichTextBoxExtensions
{
    public static void AppendText(this RichTextBox box, string text, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;

        box.SelectionColor = color;
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
    }
}
