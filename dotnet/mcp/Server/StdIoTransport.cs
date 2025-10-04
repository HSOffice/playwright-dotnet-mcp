using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcp.Server;

/// <summary>
/// Basic transport implementation using standard input/output.
/// </summary>
public sealed class StdIoTransport : IMcpTransport
{
    public Task WriteAsync(object message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Console.Out.WriteLine(payload);
        return Task.CompletedTask;
    }

    public Task<object?> ReadAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<object?>(Console.In.ReadLine());
    }
}
