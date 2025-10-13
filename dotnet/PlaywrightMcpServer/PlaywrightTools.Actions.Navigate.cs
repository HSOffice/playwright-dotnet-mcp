using System;
using System.Collections.Generic;
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
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["url"] = normalizedUrl
        };

        return await ExecuteWithResponseAsync(
            "browser_navigate",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var navigationResponse = await tab.NavigateAsync(normalizedUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                }, token).ConfigureAwait(false);

                var resultLines = new List<string>
                {
                    $"Navigated to {tab.Page.Url}"
                };

                if (navigationResponse is not null)
                {
                    resultLines.Add($"Status: {navigationResponse.Status}");
                }

                response.AddResult(string.Join("\n", resultLines));
                response.AddCode($"await page.goto('{normalizedUrl.Replace("'", "\\'")}');");
                response.SetIncludeSnapshot();
                response.SetIncludeTabs();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_navigate_back")]
    [Description("Go back to the previous page.")]
    public static async Task<string> BrowserNavigateBackAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithResponseAsync(
            "browser_navigate_back",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                IResponse? navigationResponse = null;

                await tab.WaitForCompletionAsync(async ct =>
                {
                    navigationResponse = await tab.Page.GoBackAsync(new PageGoBackOptions
                    {
                        WaitUntil = WaitUntilState.Load
                    }).ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                if (navigationResponse is null)
                {
                    response.AddResult("No previous page in history to navigate back to.");
                }
                else
                {
                    var resultLines = new List<string>
                    {
                        $"Navigated back to {tab.Page.Url}",
                        $"Status: {navigationResponse.Status}"
                    };
                    response.AddResult(string.Join("\n", resultLines));
                }

                response.AddCode("await page.goBack();");
                response.SetIncludeSnapshot();
                response.SetIncludeTabs();
            },
            cancellationToken).ConfigureAwait(false);
    }
}
