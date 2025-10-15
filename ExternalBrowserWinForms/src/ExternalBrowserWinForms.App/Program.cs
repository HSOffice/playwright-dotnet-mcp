using System;
using System.Windows.Forms;
using ExternalBrowserWinForms.App.Composition;
using ExternalBrowserWinForms.App.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace ExternalBrowserWinForms.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var services = ServiceProviderFactory.Create();
        using var scope = services.CreateScope();

        var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
