using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_wait_for")]
    [Description("Wait for text to appear or disappear or a specified time to pass.")]
    public static async Task<string> BrowserWaitForAsync(
        [Description("The time to wait in seconds.")] double? time = null,
        [Description("The text to wait for.")] string? text = null,
        [Description("The text to wait for to disappear.")] string? textGone = null,
        CancellationToken cancellationToken = default)
    {
        if (time is null && string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(textGone))
        {
            throw new ArgumentException("Either time, text, or textGone must be provided.");
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["time"] = time,
            ["text"] = text,
            ["textGone"] = textGone
        };

        return await ExecuteWithResponseAsync(
            "browser_wait_for",
            args,
            async (response, token) =>
            {
                if (time is { } seconds)
                {
                    response.AddCode($"await new Promise(f => setTimeout(f, {seconds.ToString(CultureInfo.InvariantCulture)} * 1000));");
                    var waitDuration = TimeSpan.FromSeconds(Math.Max(0, seconds));
                    if (waitDuration > TimeSpan.FromSeconds(30))
                    {
                        waitDuration = TimeSpan.FromSeconds(30);
                    }

                    if (waitDuration > TimeSpan.Zero)
                    {
                        await Task.Delay(waitDuration, token).ConfigureAwait(false);
                    }
                }

                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                ILocator? visibleLocator = null;
                ILocator? hiddenLocator = null;

                if (!string.IsNullOrWhiteSpace(textGone))
                {
                    hiddenLocator = tab.Page.GetByText(textGone!).First;
                    response.AddCode($"await page.getByText({QuoteJsString(textGone!)}).first().waitFor({{ state: 'hidden' }});");
                    token.ThrowIfCancellationRequested();
                    await hiddenLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Hidden
                    }).ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    visibleLocator = tab.Page.GetByText(text!).First;
                    response.AddCode($"await page.getByText({QuoteJsString(text!)}).first().waitFor({{ state: 'visible' }});");
                    token.ThrowIfCancellationRequested();
                    await visibleLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible
                    }).ConfigureAwait(false);
                }

                response.AddResult($"Waited for {text ?? textGone ?? time?.ToString(CultureInfo.InvariantCulture)}");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }
}
