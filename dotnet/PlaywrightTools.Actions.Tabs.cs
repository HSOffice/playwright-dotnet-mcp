using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_tabs")]
    [Description("List, create, close, or select a browser tab.")]
    public static async Task<string> BrowserTabsAsync(
        [Description("Operation to perform.")] string action,
        [Description("Tab index, used for close/select. If omitted for close, current tab is closed.")] int? index = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action must not be empty.", nameof(action));
        }

        var normalized = action.Trim().ToLowerInvariant();
        if (normalized is not ("list" or "new" or "close" or "select"))
        {
            throw new ArgumentException($"Unsupported tab action '{action}'.", nameof(action));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["action"] = normalized,
            ["index"] = index
        };

        return await ExecuteWithResponseAsync(
            "browser_tabs",
            args,
            async (response, token) =>
            {
                switch (normalized)
                {
                    case "list":
                        await EnsureLaunchedAsync(token).ConfigureAwait(false);
                        response.SetIncludeTabs();
                        break;

                    case "new":
                        await CreateNewTabAsync(token).ConfigureAwait(false);
                        response.SetIncludeTabs();
                        break;

                    case "close":
                        {
                            var tab = index.HasValue
                                ? GetTabByIndex(index.Value)
                                : await GetActiveTabAsync(token).ConfigureAwait(false);

                            await tab.Page.CloseAsync().ConfigureAwait(false);
                            response.SetIncludeSnapshot();
                            break;
                        }

                    case "select":
                        {
                            if (!index.HasValue)
                            {
                                throw new ArgumentException("Tab index is required", nameof(index));
                            }

                            var tab = GetTabByIndex(index.Value);
                            await tab.Page.BringToFrontAsync().ConfigureAwait(false);
                            TabManager.Activate(tab);
                            response.SetIncludeSnapshot();
                            break;
                        }
                }
            },
            cancellationToken).ConfigureAwait(false);
    }
}
