using System.Threading.Tasks;

namespace PlaywrightMcp.Core.Context;

/// <summary>
/// Provides a minimal abstraction for creating browser contexts.
/// </summary>
public sealed class BrowserContextFactory
{
    public Task<Context> CreateAsync()
    {
        return Task.FromResult(new Context());
    }
}
