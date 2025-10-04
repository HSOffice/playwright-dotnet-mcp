using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Protocol;
using PlaywrightMcp.Core.Runtime;

namespace PlaywrightMcp.Tools;

public static class ToolHelpers
{
    public static IToolDefinition CreatePlaceholderTool(string name, string description)
        => CreateDelegateTool(name, description, (context, parameters, cancellationToken) =>
        {
            var response = new Response();
            response.AddBlock(new MarkdownBlock($"Tool '{name}' is not yet implemented."));
            return Task.FromResult(response);
        });

    public static IToolDefinition CreateDelegateTool(
        string name,
        string description,
        Func<ToolInvocationContext, JsonElement, CancellationToken, Task<Response>> handler,
        ToolSchema? schema = null)
        => new DelegateToolDefinition(
            name,
            description,
            schema ?? ToolSchema.Empty,
            handler ?? throw new ArgumentNullException(nameof(handler)));

    private sealed class DelegateToolDefinition : ToolDefinitionBase
    {
        private readonly Func<ToolInvocationContext, JsonElement, CancellationToken, Task<Response>> _handler;

        public DelegateToolDefinition(
            string name,
            string description,
            ToolSchema schema,
            Func<ToolInvocationContext, JsonElement, CancellationToken, Task<Response>> handler)
            : base(name, description, schema)
        {
            _handler = handler;
        }

        public override Task<Response> ExecuteAsync(ToolInvocationContext context, JsonElement parameters, CancellationToken cancellationToken)
            => _handler(context, parameters, cancellationToken);
    }
}
