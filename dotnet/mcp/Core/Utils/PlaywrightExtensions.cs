namespace PlaywrightMcp.Core.Utils;

/// <summary>
/// Placeholder for extension methods over Playwright objects.
/// </summary>
public static class PlaywrightExtensions
{
    public static string Describe(this object? playwrightObject)
    {
        return playwrightObject?.ToString() ?? "(null)";
    }
}
