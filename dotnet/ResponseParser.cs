using System;
using System.Collections.Generic;
using System.Linq;

namespace PlaywrightMcpServer;

public static class ResponseParser
{
    public static ParsedResponse? Parse(SerializedResponse response)
    {
        if (response.Content.Count == 0 || response.Content[0] is not TextContent text)
        {
            return null;
        }

        var sections = ParseSections(text.Text);
        sections.TryGetValue("Result", out var result);
        sections.TryGetValue("Ran Playwright code", out var code);
        sections.TryGetValue("Open tabs", out var tabs);
        sections.TryGetValue("Page state", out var pageState);
        sections.TryGetValue("New console messages", out var console);
        sections.TryGetValue("Modal state", out var modalState);
        sections.TryGetValue("Downloads", out var downloads);

        var codeBlock = code is null
            ? null
            : code
                .Replace("```js\n", string.Empty)
                .Replace("\n```", string.Empty);

        return new ParsedResponse(
            result,
            codeBlock,
            tabs,
            pageState,
            console,
            modalState,
            downloads,
            response.IsError,
            response.Content.Skip(1).ToArray());
    }

    private static Dictionary<string, string> ParseSections(string text)
    {
        var sections = new Dictionary<string, string>(StringComparer.Ordinal);
        var lines = text.Split('\n');
        string? currentName = null;
        var buffer = new List<string>();

        void Commit()
        {
            if (currentName is not null)
            {
                sections[currentName] = string.Join('\n', buffer).Trim();
            }

            buffer.Clear();
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine;
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                Commit();
                currentName = line[4..].Trim();
            }
            else if (currentName is not null)
            {
                buffer.Add(line);
            }
        }

        Commit();
        return sections;
    }
}

public sealed record ParsedResponse(
    string? Result,
    string? Code,
    string? Tabs,
    string? PageState,
    string? ConsoleMessages,
    string? ModalState,
    string? Downloads,
    bool? IsError,
    IReadOnlyList<IResponseContent> Attachments);
