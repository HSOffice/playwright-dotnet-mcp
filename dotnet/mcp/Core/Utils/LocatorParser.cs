namespace PlaywrightMcp.Core.Utils;

/// <summary>
/// Provides helper methods for working with Playwright locators.
/// </summary>
public static class LocatorParser
{
    public static string Normalize(string locator) => locator?.Trim() ?? string.Empty;
}
