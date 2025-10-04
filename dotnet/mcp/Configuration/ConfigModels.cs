using System.Collections.Generic;

namespace PlaywrightMcp.Configuration;

/// <summary>
/// Represents the root configuration consumed by the MCP server.
/// </summary>
public sealed class FullConfig
{
    public BrowserConfig Browser { get; init; } = new();
    public SecretsConfig Secrets { get; init; } = new();
    public CapabilitySettings Capabilities { get; init; } = new();
}

/// <summary>
/// Browser related options for establishing a Playwright context.
/// </summary>
public sealed class BrowserConfig
{
    public string? Endpoint { get; init; }
    public bool Headless { get; init; } = true;
    public string? DefaultViewport { get; init; }
}

/// <summary>
/// Sensitive values that should be redacted from logs and responses.
/// </summary>
public sealed class SecretsConfig
{
    public IReadOnlyCollection<string> SensitiveValues { get; init; } = new List<string>();
}

/// <summary>
/// Controls which capabilities and tools are exposed by the MCP server.
/// </summary>
public sealed class CapabilitySettings
{
    public bool AllowFileSystem { get; init; }
    public bool EnableTracing { get; init; }
    public IReadOnlyCollection<string>? IncludedTools { get; init; }
    public IReadOnlyCollection<string>? ExcludedTools { get; init; }
}
