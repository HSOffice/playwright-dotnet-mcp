using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcp.Server;

/// <summary>
/// Abstraction for the transport layer used by the MCP server.
/// </summary>
public interface IMcpTransport
{
    Task WriteAsync(object message, CancellationToken cancellationToken);

    Task<object?> ReadAsync(CancellationToken cancellationToken);
}
