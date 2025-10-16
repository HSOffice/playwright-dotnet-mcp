using System;
using System.Windows.Forms;

namespace PlaywrightRemoteBrowserLauncher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
