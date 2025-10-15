namespace ExternalBrowserWinForms.Core.Models;

/// <summary>
/// Represents the data required to launch an external browser.
/// </summary>
public sealed class BrowserLaunchRequest
{
    public BrowserLaunchRequest(Uri url, bool useDefaultBrowser, string? browserPath, string? additionalArguments)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        UseDefaultBrowser = useDefaultBrowser;
        BrowserPath = browserPath;
        AdditionalArguments = additionalArguments;
    }

    public Uri Url { get; }

    public bool UseDefaultBrowser { get; }

    public string? BrowserPath { get; }

    public string? AdditionalArguments { get; }
}
