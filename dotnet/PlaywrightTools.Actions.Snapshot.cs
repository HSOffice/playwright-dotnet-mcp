using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_snapshot")]
    [Description("Capture accessibility snapshot of the current page, this is better than screenshot.")]
    public static async Task<string> BrowserSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithResponseAsync(
            "browser_snapshot",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            async (response, token) =>
            {
                // Ensure there is an active tab so the response pipeline can capture a snapshot
                // and attach the appropriate element references (`ref`).
                await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }
}
