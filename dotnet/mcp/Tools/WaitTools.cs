using System.Collections.Generic;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class WaitTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreatePlaceholderTool("wait.for", "Waits for a condition within the page to be satisfied."),
    };
}
