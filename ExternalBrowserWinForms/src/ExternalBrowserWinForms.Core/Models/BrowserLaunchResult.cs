namespace ExternalBrowserWinForms.Core.Models;

/// <summary>
/// Represents the outcome of a browser launch operation.
/// </summary>
public sealed class BrowserLaunchResult
{
    private BrowserLaunchResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }

    public string Message { get; }

    public static BrowserLaunchResult Successful(string message) => new(true, message);

    public static BrowserLaunchResult Failed(string message) => new(false, message);
}
