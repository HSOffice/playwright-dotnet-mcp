using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_install")]
    [Description("Install the browser specified in the config.")]
    public static async Task<string> BrowserInstallAsync(
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);

        return await ExecuteWithResponseAsync(
            "browser_install",
            args,
            async (response, token) =>
            {
                token.ThrowIfCancellationRequested();

                var channel = ResolveInstallChannel();
                var installArgs = new List<string> { "install" };
                if (!string.IsNullOrWhiteSpace(channel))
                {
                    installArgs.Add(channel);
                }

                var exitCode = await Program.Main(installArgs.ToArray()).ConfigureAwait(false);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Failed to install Playwright browser for channel '{channel}'. Exit code: {exitCode}.");
                }

                response.AddResult($"Installed Playwright browser for channel '{channel}'.");
                response.SetIncludeTabs();
            },
            cancellationToken).ConfigureAwait(false);
    }
}
