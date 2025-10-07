using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PlaywrightMcpServer;

internal static class SnapshotMarkdownBuilder
{
    public static IReadOnlyList<string> Build(SnapshotPayload snapshot, bool omitSnapshot)
    {
        var lines = new List<string>();

        if (snapshot.Console is { Count: > 0 })
        {
            lines.Add("### New console messages");
            foreach (var entry in snapshot.Console)
            {
                lines.Add($"- {Trim(entry.Text, 100)}");
            }

            lines.Add(string.Empty);
        }

        if (snapshot.Network is { Count: > 0 })
        {
            lines.Add("### Network requests");
            foreach (var entry in snapshot.Network)
            {
                var status = entry.Status?.ToString() ?? entry.Failure ?? "pending";
                lines.Add($"- {entry.Method} {entry.Url} ({status})");
            }

            lines.Add(string.Empty);
        }

        lines.Add("### Page state");
        lines.Add($"- Page URL: {snapshot.Url}");
        lines.Add($"- Page Title: {snapshot.Title ?? string.Empty}");
        lines.Add("- Page Snapshot:");
        lines.Add("```json");
        lines.Add(omitSnapshot ? "<snapshot>" : Format(snapshot.Aria));
        lines.Add("```");

        return lines;
    }

    private static string Format(JsonElement? element)
    {
        if (element is null)
        {
            return "null";
        }

        return JsonSerializer.Serialize(element.Value, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string Trim(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text.Substring(0, maxLength) + "...";
    }
}
