using System.Collections.Generic;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class TracingTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreatePlaceholderTool("tracing.start", "Starts capturing a Playwright trace."),
    };
}
