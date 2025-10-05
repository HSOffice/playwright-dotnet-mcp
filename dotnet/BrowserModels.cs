using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

internal sealed record ConsoleMessageEntry
{
    [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; }
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
    [JsonPropertyName("args")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string[]? Args { get; init; }
}

internal sealed class NetworkRequestEntry
{
    [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; }
    [JsonPropertyName("method")] public string Method { get; init; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    [JsonPropertyName("resourceType")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ResourceType { get; init; }
    [JsonPropertyName("status")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? Status { get; set; }
    [JsonPropertyName("failure")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Failure { get; set; }

    public NetworkRequestEntry Clone() => new()
    {
        Timestamp = Timestamp,
        Method = Method,
        Url = Url,
        ResourceType = ResourceType,
        Status = Status,
        Failure = Failure
    };
}

internal sealed record SnapshotPayload
{
    [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; }
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    [JsonPropertyName("title")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Title { get; init; }
    [JsonPropertyName("aria")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public AccessibilitySnapshot? Aria { get; init; }
    [JsonPropertyName("console")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<ConsoleMessageEntry>? Console { get; init; }
    [JsonPropertyName("network")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<NetworkRequestEntry>? Network { get; init; }
}

internal sealed record TabDescriptor
{
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("url")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Url { get; init; }
    [JsonPropertyName("title")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Title { get; init; }
    [JsonPropertyName("isActive")] public bool IsActive { get; init; }
    [JsonPropertyName("createdAt")] public DateTimeOffset CreatedAt { get; init; }
}

internal sealed record TabRestoreEntry(string? Url);
