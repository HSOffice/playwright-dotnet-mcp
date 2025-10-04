using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcp.Server;

/// <summary>
/// Periodically notifies the host that the server is still alive.
/// </summary>
public sealed class HeartbeatService
{
    private readonly IMcpTransport _transport;

    public HeartbeatService(IMcpTransport transport)
    {
        _transport = transport;
    }

    public Task SendAsync(CancellationToken cancellationToken)
    {
        var heartbeat = new { type = "heartbeat" };
        return _transport.WriteAsync(heartbeat, cancellationToken);
    }
}
