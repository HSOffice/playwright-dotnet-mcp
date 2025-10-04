using PlaywrightMcp.Configuration;
using PlaywrightMcp.Core.Context;
using PlaywrightMcp.Core.Utils;

namespace PlaywrightMcp.Core.BrowserServerBackend;

/// <summary>
/// Holds execution-wide services and state for a single tool invocation.
/// </summary>
public sealed class ToolInvocationContext
{
    public ToolInvocationContext(Context.Context browserContext, FullConfig configuration, TimeProvider timeProvider)
    {
        BrowserContext = browserContext;
        Configuration = configuration;
        TimeProvider = timeProvider;
    }

    public Context.Context BrowserContext { get; }

    public FullConfig Configuration { get; }

    public TimeProvider TimeProvider { get; }
}
