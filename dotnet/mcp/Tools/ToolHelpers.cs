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
        => new PlaceholderTool(name, description);

    private sealed class PlaceholderTool : ToolDefinitionBase
    {
        public PlaceholderTool(string name, string description)
            : base(name, description, ToolSchema.Empty)
        {
        }

        public override Task<Response> ExecuteAsync(ToolInvocationContext context, JsonElement parameters, CancellationToken cancellationToken)
        {
            var response = new Response();
            response.AddBlock(new MarkdownBlock($"Tool '{Name}' is not yet implemented."));
            return Task.FromResult(response);
        }
    }
}
