using ExternalBrowserWinForms.App.Forms;
using ExternalBrowserWinForms.Core.Services;
using ExternalBrowserWinForms.Core.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace ExternalBrowserWinForms.App.Composition;

/// <summary>
/// Configures dependency injection for the WinForms application.
/// </summary>
internal static class ServiceProviderFactory
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILaunchRequestValidator, LaunchRequestValidator>();
        services.AddSingleton<IBrowserProcessRunner, BrowserProcessRunner>();
        services.AddSingleton<IBrowserLaunchService, BrowserLaunchService>();

        services.AddSingleton<MainForm>();

        return services.BuildServiceProvider();
    }
}
