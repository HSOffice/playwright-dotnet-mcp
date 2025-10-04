using System.Collections.Generic;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class ScreenshotTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreatePlaceholderTool("screenshot.capture", "Captures a screenshot of the current page."),
    };
}
