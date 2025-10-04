using System.Collections.Generic;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class MouseTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreatePlaceholderTool("mouse.click", "Performs a mouse click on a target element."),
    };
}
