using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Configuration;
using PlaywrightMcp.Core.Context;
using PlaywrightMcp.Core.Protocol;
using PlaywrightMcp.Core.Runtime;
using PlaywrightMcp.Core.Services;
using PlaywrightMcp.Core.Utils;

namespace PlaywrightMcp.Core.BrowserServerBackend;

/// <summary>
/// Coordinates tool execution and shared browser context lifecycle.
/// </summary>
public sealed class BrowserServerBackend
{
    private readonly ToolRegistry _toolRegistry;
    private readonly ToolExecutionService _executionService;
    private readonly BrowserContextFactory _contextFactory;
    private readonly FullConfig _configuration;
    private readonly TimeProvider _timeProvider;
    private Context.Context? _context;

    public BrowserServerBackend(
        ToolRegistry toolRegistry,
        ToolExecutionService executionService,
        BrowserContextFactory contextFactory,
        FullConfig configuration,
        TimeProvider timeProvider)
    {
        _toolRegistry = toolRegistry;
        _executionService = executionService;
        _contextFactory = contextFactory;
        _configuration = configuration;
        _timeProvider = timeProvider;
    }

    public IReadOnlyCollection<IToolDefinition> ListTools() => _toolRegistry.GetTools();

    public async Task<Response> ExecuteAsync(string name, JsonElement parameters, CancellationToken cancellationToken)
    {
        if (!_toolRegistry.TryGetTool(name, out var tool))
        {
            return Response.Empty;
        }

        var invocationContext = new ToolInvocationContext(await EnsureContextAsync().ConfigureAwait(false), _configuration, _timeProvider);
        return await _executionService.ExecuteAsync(tool, invocationContext, parameters, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Context.Context> EnsureContextAsync()
    {
        if (_context is null)
        {
            _context = await _contextFactory.CreateAsync().ConfigureAwait(false);
        }

        return _context;
    }
}
