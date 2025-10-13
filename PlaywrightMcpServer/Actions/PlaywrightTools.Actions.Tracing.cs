using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_start_tracing")]
    [Description("Start trace recording.")]
    public static async Task<string> BrowserStartTracingAsync(
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);

        return await ExecuteWithResponseAsync(
            "browser_start_tracing",
            args,
            async (response, token) =>
            {
                await EnsureLaunchedAsync(token).ConfigureAwait(false);
                if (_context is null)
                {
                    throw new InvalidOperationException("Browser context not initialized.");
                }

                if (_tracingActive)
                {
                    response.AddError("Tracing is already active.");
                    return;
                }

                Directory.CreateDirectory(TracesDir);
                var name = $"trace-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var archivePath = Path.Combine(TracesDir, $"{name}.zip");
                var legend = $"- Trace archive: {archivePath}";

                await _context.Tracing.StartAsync(new TracingStartOptions
                {
                    Name = name,
                    Screenshots = true,
                    Snapshots = true
                }).ConfigureAwait(false);

                _tracingActive = true;
                _traceLegend = legend;
                _traceName = name;

                response.AddResult($"Tracing started, saving to {TracesDir}.\n{legend}");
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_stop_tracing")]
    [Description("Stop trace recording.")]
    public static async Task<string> BrowserStopTracingAsync(
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);

        return await ExecuteWithResponseAsync(
            "browser_stop_tracing",
            args,
            async (response, token) =>
            {
                if (!_tracingActive)
                {
                    response.AddError("Tracing is not active.");
                    return;
                }

                if (_context is null)
                {
                    throw new InvalidOperationException("Browser context not initialized.");
                }

                var name = _traceName ?? $"trace-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var archivePath = Path.Combine(TracesDir, $"{name}.zip");
                var directory = Path.GetDirectoryName(archivePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await _context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = archivePath
                }).ConfigureAwait(false);

                var legend = _traceLegend ?? $"- Trace archive: {archivePath}";
                response.AddResult($"Tracing stopped.\n{legend}");

                _tracingActive = false;
                _traceLegend = null;
                _traceName = null;
            },
            cancellationToken).ConfigureAwait(false);
    }
}
