using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Runtime;

namespace PlaywrightMcp.Core.Protocol;

/// <summary>
/// Describes a tool that can be invoked through the MCP protocol.
/// </summary>
public interface IToolDefinition
{
    string Name { get; }
    string Description { get; }
    ToolSchema InputSchema { get; }

    Task<Response> ExecuteAsync(ToolInvocationContext context, JsonElement parameters, CancellationToken cancellationToken);
}

/// <summary>
/// A simple base implementation that derived tools can inherit from.
/// </summary>
public abstract class ToolDefinitionBase : IToolDefinition
{
    protected ToolDefinitionBase(string name, string description, ToolSchema schema)
    {
        Name = name;
        Description = description;
        InputSchema = schema;
    }

    public string Name { get; }

    public string Description { get; }

    public ToolSchema InputSchema { get; }

    public abstract Task<Response> ExecuteAsync(ToolInvocationContext context, JsonElement parameters, CancellationToken cancellationToken);
}
