using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Server;

/// <summary>
/// Entry point exposing the MCP server functionality.
/// </summary>
public sealed class McpServer
{
    private readonly BrowserServerBackend _backend;
    private readonly IMcpTransport _transport;

    public McpServer(BrowserServerBackend backend, IMcpTransport transport)
    {
        _backend = backend;
        _transport = transport;
    }

    public Task<ListToolsResponse> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var summaries = _backend.ListTools()
            .Select(t => new ToolSummary(t.Name, t.Description, t.InputSchema))
            .ToList();
        return Task.FromResult(new ListToolsResponse(summaries));
    }

    public async Task<CallToolResponse> CallToolAsync(string name, JsonElement parameters, CancellationToken cancellationToken = default)
    {
        var response = await _backend.ExecuteAsync(name, parameters, cancellationToken).ConfigureAwait(false);
        return ResponseSerializer.Serialize(response);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await _transport.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (message is null)
            {
                break;
            }

            // The dispatcher is intentionally left as a stub for the initial scaffolding.
        }
    }
}
