using System.Collections.Generic;

namespace PlaywrightMcpServer;

internal static class ModalStateMarkdownBuilder
{
    public static IReadOnlyList<string> Build(IReadOnlyList<ModalStateEntry> modalStates)
    {
        var lines = new List<string> { "### Modal state" };

        if (modalStates.Count == 0)
        {
            lines.Add("- There is no modal state present");
            return lines;
        }

        foreach (var state in modalStates)
        {
            lines.Add($"- [{state.Description}]: can be handled by the \"{state.ClearedBy}\" tool");
        }

        return lines;
    }
}
