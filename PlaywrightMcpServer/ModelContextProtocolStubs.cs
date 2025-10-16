using System;

namespace ModelContextProtocol.Server;

/// <summary>
/// Minimal stub of the MCP server tool type attribute so the project can build
/// without the external <c>ModelContextProtocol.Server</c> NuGet package.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class McpServerToolTypeAttribute : Attribute
{
}

/// <summary>
/// Minimal stub of the MCP server tool attribute used to describe individual tools.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class McpServerToolAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the unique name of the tool.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional capability category for the tool.
    /// </summary>
    public string? Capability { get; set; }

    /// <summary>
    /// Gets or sets the optional type for the tool (for example, action/readOnly/input).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets an optional JSON schema describing the tool's input payload.
    /// </summary>
    public string? InputSchema { get; set; }

    /// <summary>
    /// Gets or sets a localized description of the tool.
    /// </summary>
    public string? Description { get; set; }
}
