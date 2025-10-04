using System.Collections.Generic;

namespace PlaywrightMcp.Core.Context;

/// <summary>
/// Provides helper methods for projecting raw Playwright events into textual descriptions.
/// </summary>
public static class TabEvents
{
    public static IReadOnlyCollection<string> ToSummaries(Tab tab)
    {
        return tab.Events;
    }
}
