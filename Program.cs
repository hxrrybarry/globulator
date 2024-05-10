using System.Runtime.InteropServices;

namespace vault_thing;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Main());
    }

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();
}