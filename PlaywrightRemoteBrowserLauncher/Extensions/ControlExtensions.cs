using System.Windows.Forms;

namespace PlaywrightRemoteBrowserLauncher.Extensions;

public static class ControlExtensions
{
    public static void InvokeSafe(this Control control, Action action)
    {
        if (control.IsDisposed)
        {
            return;
        }

        if (control.InvokeRequired)
        {
            control.BeginInvoke(action);
        }
        else
        {
            action();
        }
    }
}
