using System.Collections.Generic;

namespace PlaywrightMcp.Core.Protocol;

/// <summary>
/// Representation of the MCP protocol contracts shared between the server and host.
/// </summary>
public sealed record ListToolsResponse(IReadOnlyCollection<ToolSummary> Tools);

public sealed record ToolSummary(string Name, string Description, ToolSchema Schema);

public sealed record CallToolRequest(string Name, object? Arguments);

public sealed record CallToolResponse(bool Success, IReadOnlyCollection<object> Blocks);

public sealed record PingRequest(string Message);

public sealed record PingResponse(string Message);
