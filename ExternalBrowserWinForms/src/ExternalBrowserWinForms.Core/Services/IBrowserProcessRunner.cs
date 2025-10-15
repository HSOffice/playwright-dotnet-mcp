using ExternalBrowserWinForms.Core.Models;

namespace ExternalBrowserWinForms.Core.Services;

/// <summary>
/// Abstraction around launching processes for browsers.
/// </summary>
public interface IBrowserProcessRunner
{
    void Launch(BrowserLaunchRequest request);
}
