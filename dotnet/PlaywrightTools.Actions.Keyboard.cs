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
    [McpServerTool(Name = "browser_press_key")]
    [Description("Press a key on the keyboard.")]
    public static async Task<string> BrowserPressKeyAsync(
        [Description("Name of the key to press or a character to generate, such as `ArrowLeft` or `a`.")] string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must not be empty.", nameof(key));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["key"] = key
        };

        return await ExecuteWithResponseAsync(
            "browser_press_key",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();
                response.AddCode($"// Press {key}");
                response.AddCode($"await page.keyboard.press({QuoteJsString(key)});");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await tab.Page.Keyboard.PressAsync(key).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_type")]
    [Description("Type text into editable element.")]
    public static async Task<string> BrowserTypeAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact target element reference from the page snapshot.")] string elementRef,
        [Description("Text to type into the element.")] string text,
        [Description("Whether to submit entered text (press Enter after).")] bool? submit = null,
        [Description("Whether to type one character at a time. Useful for triggering key handlers in the page. By default entire text is filled in at once.")] bool? slowly = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(element))
        {
            throw new ArgumentException("Element description must not be empty.", nameof(element));
        }

        if (string.IsNullOrWhiteSpace(elementRef))
        {
            throw new ArgumentException("Element ref must not be empty.", nameof(elementRef));
        }

        text ??= string.Empty;

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef,
            ["text"] = text,
            ["submit"] = submit,
            ["slowly"] = slowly
        };

        return await ExecuteWithResponseAsync(
            "browser_type",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(new TabState.RefLocatorRequest(element, elementRef), token).ConfigureAwait(false);
                var locatorSource = await GenerateLocatorSourceAsync(locator, elementRef, token).ConfigureAwait(false);
                var secret = LookupSecret(text);

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();

                    if (slowly == true)
                    {
                        response.SetIncludeSnapshot();
                        response.AddCode($"await {locatorSource}.pressSequentially({secret.Code});");
                        await locator.PressSequentiallyAsync(secret.Value).ConfigureAwait(false);
                    }
                    else
                    {
                        response.AddCode($"await {locatorSource}.fill({secret.Code});");
                        await locator.FillAsync(secret.Value).ConfigureAwait(false);
                    }

                    if (submit == true)
                    {
                        response.SetIncludeSnapshot();
                        response.AddCode($"await {locatorSource}.press('Enter');");
                        await locator.PressAsync("Enter").ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
