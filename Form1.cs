using System.DirectoryServices;
using WMPLib;

namespace vault_thing;

public partial class Main : Form
{
    public Main()
    {
        InitializeComponent();
    }
    CommandConsole console;

    private void PlayTimeCircuitsSFX()
    {
        WindowsMediaPlayer player = new()
        {
            URL = "time_circuits.mp3"
        };
        player.controls.play();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        console = new();
        consoleOutput.AppendText($"{DateTime.Now:HH:mm:ss}; ", Color.Magenta); 
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText(">> Program booted..\n", Color.Green);

        PlayTimeCircuitsSFX();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        consoleOutput.AppendText($"{DateTime.Now:HH:mm:ss}; ", Color.Magenta);
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText($"<< {commandBox.Text}", Color.Cyan);
        (string, Color) response = console.ProcessCommand(commandBox.Text);
        commandBox.Clear();

        string responseText = response.Item1;
        Color responseColour = response.Item2;

        consoleOutput.AppendText($"\n{DateTime.Now:HH:mm:ss}; ", Color.Magenta);
        consoleOutput.AppendText($"{console.CurrentDirectory}; ", Color.Yellow);
        consoleOutput.AppendText($"{console.CurrentGlobName}; ", Color.Orange);
        consoleOutput.AppendText($">> {responseText}\n", responseColour);
    }

    private void commandBox_TextChanged(object sender, EventArgs e)
    {

    }

    private void consoleOutput_TextChanged(object sender, EventArgs e)
    {

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
