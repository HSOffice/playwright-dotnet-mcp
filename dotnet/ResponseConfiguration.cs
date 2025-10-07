using System;
using System.Collections.Generic;

namespace PlaywrightMcpServer;

/// <summary>
/// Configuration for response serialization behaviour.
/// Mirrors the knobs available in the TypeScript implementation.
/// </summary>
public sealed class ResponseConfiguration
{
    /// <summary>
    /// Determines whether image attachments should be emitted.
    /// Defaults to <see cref="ImageResponseMode.Include"/> to match
    /// the TypeScript runtime behaviour.
    /// </summary>
    public ImageResponseMode ImageResponses { get; init; } = ImageResponseMode.Include;

    /// <summary>
    /// Optional map of secrets that should be redacted from textual
    /// responses. When present every occurrence of the value will be
    /// replaced with <c>&lt;secret&gt;{name}&lt;/secret&gt;</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Secrets { get; init; }
        = null;
}

public enum ImageResponseMode
{
    Include,
    Omit
}
