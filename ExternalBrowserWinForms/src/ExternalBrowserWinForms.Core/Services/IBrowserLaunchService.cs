using ExternalBrowserWinForms.Core.Models;

namespace ExternalBrowserWinForms.Core.Services;

/// <summary>
/// Provides the primary business logic for launching browsers.
/// </summary>
public interface IBrowserLaunchService
{
    Task<BrowserLaunchResult> LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken = default);
}
