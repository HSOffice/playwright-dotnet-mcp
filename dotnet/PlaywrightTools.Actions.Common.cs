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
    [McpServerTool(Name = "browser_close")]
    [Description("Close the page.")]
    public static async Task<string> BrowserCloseAsync(
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);

        return await ExecuteWithResponseAsync(
            "browser_close",
            args,
            async (response, token) =>
            {
                await CloseBrowserContextAsync(token).ConfigureAwait(false);
                response.AddResult("Closed Playwright browser context.");
                response.AddCode("await page.close();");
                response.SetIncludeTabs();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_resize")]
    [Description("Resize the browser window.")]
    public static async Task<string> BrowserResizeAsync(
        [Description("Width of the browser window.")] int width,
        [Description("Height of the browser window.")] int height,
        CancellationToken cancellationToken = default)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be a positive value.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be a positive value.");
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["width"] = width,
            ["height"] = height
        };

        return await ExecuteWithResponseAsync(
            "browser_resize",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.AddCode($"await page.setViewportSize({{ width: {width}, height: {height} }});");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await tab.Page.SetViewportSizeAsync(width, height).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
