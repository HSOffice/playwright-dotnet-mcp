using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_evaluate")]
    [Description("Evaluate JavaScript expression on page or element.")]
    public static async Task<string> BrowserEvaluateAsync(
        [Description("JavaScript function to execute, optionally receiving the element when provided.")] string function,
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string? element = null,
        [Description("Exact target element reference from the page snapshot.")] string? elementRef = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(function))
        {
            throw new ArgumentException("Function must not be empty.", nameof(function));
        }

        var hasElement = !string.IsNullOrWhiteSpace(element) || !string.IsNullOrWhiteSpace(elementRef);
        if (hasElement && (string.IsNullOrWhiteSpace(element) || string.IsNullOrWhiteSpace(elementRef)))
        {
            throw new ArgumentException("Both element and ref must be provided when targeting an element.");
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["function"] = function,
            ["element"] = element,
            ["ref"] = elementRef
        };

        return await ExecuteWithResponseAsync(
            "browser_evaluate",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();

                ILocator? locator = null;
                string? locatorSource = null;

                if (!string.IsNullOrWhiteSpace(element) && !string.IsNullOrWhiteSpace(elementRef))
                {
                    locator = await tab.GetLocatorByRefAsync(new TabState.RefLocatorRequest(element!, elementRef!), token).ConfigureAwait(false);
                    locatorSource = await GenerateLocatorSourceAsync(locator, elementRef!, token).ConfigureAwait(false);
                    response.AddCode($"await {locatorSource}.evaluate({QuoteJsString(function)});");
                }
                else
                {
                    response.AddCode($"await page.evaluate({QuoteJsString(function)});");
                }

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();

                    object? result = locator is null
                        ? await tab.Page.EvaluateAsync<object?>(function).ConfigureAwait(false)
                        : await locator.EvaluateAsync<object?>(function).ConfigureAwait(false);

                    response.AddResult(FormatEvaluationResult(result));
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static string FormatEvaluationResult(object? value)
    {
        if (value is null)
        {
            return "undefined";
        }

        if (value is string str)
        {
            return JsonSerializer.Serialize(str);
        }

        if (value is bool boolValue)
        {
            return boolValue ? "true" : "false";
        }

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Undefined => "undefined",
                JsonValueKind.Null => "null",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.String => JsonSerializer.Serialize(element.GetString()),
                _ => JsonSerializer.Serialize(JsonDocument.Parse(element.GetRawText()).RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            };
        }

        if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }
}
