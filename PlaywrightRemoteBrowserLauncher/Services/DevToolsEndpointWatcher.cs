using System.Net.Http;
using System.Text.Json;

namespace PlaywrightRemoteBrowserLauncher.Services;

public static class DevToolsEndpointWatcher
{
    public static async Task<string?> WaitForEndpointAsync(int port, TimeSpan timeout, CancellationToken token)
    {
        using var http = new HttpClient();
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var json = await http.GetStringAsync($"http://127.0.0.1:{port}/json/version", token);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("webSocketDebuggerUrl", out var url) &&
                    url.ValueKind == JsonValueKind.String)
                {
                    return url.GetString();
                }
            }
            catch
            {
                // ignored until timeout
            }

            await Task.Delay(500, token);
        }

        return null;
    }
}
