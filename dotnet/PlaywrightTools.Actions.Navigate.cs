using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_navigate")]
    [Description("Navigate to a URL.")]
    public static async Task<string> BrowserNavigateAsync(
        [Description("The URL to navigate to.")] string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL must not be empty.", nameof(url));
        }

        var normalizedUrl = NormalizeUrl(url);
        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);

        var response = await tab.Page.GotoAsync(normalizedUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        }).ConfigureAwait(false);

        var snapshot = await SnapshotManager.CaptureAsync(tab, cancellationToken).ConfigureAwait(false);

        var result = new
        {
            navigated = true,
            url = tab.Page.Url,
            status = response?.Status,
            snapshot,
            tabs = TabManager.DescribeTabs()
        };

        return Serialize(result);
    }

    [McpServerTool(Name = "browser_navigate_back")]
    [Description("Go back to the previous page.")]
    public static async Task<string> BrowserNavigateBackAsync(
        CancellationToken cancellationToken = default)
    {
        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
        var response = await tab.Page.GoBackAsync(new PageGoBackOptions
        {
            WaitUntil = WaitUntilState.Load
        }).ConfigureAwait(false);

        var snapshot = await SnapshotManager.CaptureAsync(tab, cancellationToken).ConfigureAwait(false);

        var result = new
        {
            navigated = response is not null,
            url = tab.Page.Url,
            status = response?.Status,
            snapshot,
            tabs = TabManager.DescribeTabs()
        };

        return Serialize(result);
    }
}
