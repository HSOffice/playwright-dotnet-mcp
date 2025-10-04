using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Protocol;
using PlaywrightMcp.Core.Runtime;

namespace PlaywrightMcp.Core.Services;

/// <summary>
/// Executes tool calls and provides centralised error handling.
/// </summary>
public sealed class ToolExecutionService
{
    public async Task<Response> ExecuteAsync(
        IToolDefinition tool,
        ToolInvocationContext context,
        JsonElement parameters,
        CancellationToken cancellationToken)
    {
        if (tool is null)
        {
            return Response.Empty;
        }

        return await tool.ExecuteAsync(context, parameters, cancellationToken).ConfigureAwait(false);
    }
}
