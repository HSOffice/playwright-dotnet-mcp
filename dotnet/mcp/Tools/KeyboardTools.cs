using System.Collections.Generic;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class KeyboardTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreatePlaceholderTool("keyboard.type", "Types text into the focused element."),
    };
}
