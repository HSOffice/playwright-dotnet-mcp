using System.Collections.Generic;
using PlaywrightMcp.Configuration;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Context;
using PlaywrightMcp.Core.Protocol;
using PlaywrightMcp.Core.Services;
using PlaywrightMcp.Core.Utils;
using PlaywrightMcp.Tools;

namespace PlaywrightMcp.Server;

/// <summary>
/// Configures and builds instances of <see cref="McpServer"/>.
/// </summary>
public sealed class McpServerBuilder
{
    private readonly ToolRegistry _toolRegistry = new();
    private readonly ToolExecutionService _executionService = new();
    private BrowserContextFactory _contextFactory = new();
    private FullConfig _configuration = new();
    private TimeProvider _timeProvider = new();

    public McpServerBuilder WithConfiguration(FullConfig configuration)
    {
        _configuration = configuration;
        return this;
    }

    public McpServerBuilder WithContextFactory(BrowserContextFactory factory)
    {
        _contextFactory = factory;
        return this;
    }

    public McpServerBuilder WithTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        return this;
    }

    public McpServerBuilder WithTools(IEnumerable<IToolDefinition> tools)
    {
        _toolRegistry.RegisterTools(tools);
        return this;
    }

    public McpServerBuilder UseDefaultTools()
    {
        _toolRegistry.RegisterTools(BrowserTools.All);
        return this;
    }

    public McpServer Build(IMcpTransport transport)
    {
        var backend = new BrowserServerBackend(_toolRegistry, _executionService, _contextFactory, _configuration, _timeProvider);
        return new McpServer(backend, transport);
    }
}
