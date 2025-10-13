using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_verify_element_visible")]
    [Description("Verify element is visible on the page.")]
    public static async Task<string> BrowserVerifyElementVisibleAsync(
        [Description("ROLE of the element.")] string role,
        [Description("ACCESSIBLE_NAME of the element.")] string accessibleName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role must not be empty.", nameof(role));
        }

        if (string.IsNullOrWhiteSpace(accessibleName))
        {
            throw new ArgumentException("Accessible name must not be empty.", nameof(accessibleName));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["role"] = role,
            ["accessibleName"] = accessibleName
        };

        return await ExecuteWithResponseAsync(
            "browser_verify_element_visible",
            args,
            async (response, token) =>
            {
                if (!Enum.TryParse<AriaRole>(role, true, out var ariaRole))
                {
                    throw new ArgumentException($"Unsupported role '{role}'.", nameof(role));
                }

                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = tab.Page.GetByRole(ariaRole, new() { Name = accessibleName });
                var count = await locator.CountAsync().ConfigureAwait(false);
                if (count == 0)
                {
                    response.AddError($"Element with role \"{role}\" and accessible name \"{accessibleName}\" not found");
                    return;
                }

                response.AddCode($"await expect(page.getByRole({QuoteJsString(role)}, {{ name: {QuoteJsString(accessibleName)} }})).toBeVisible();");
                response.AddResult("Done");
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_verify_text_visible")]
    [Description("Verify text is visible on the page.")]
    public static async Task<string> BrowserVerifyTextVisibleAsync(
        [Description("TEXT to verify. Can be found in the snapshot like this: `- role \"Accessible Name\": {TEXT}` or like this: `- text: {TEXT}`.")] string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text must not be empty.", nameof(text));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["text"] = text
        };

        return await ExecuteWithResponseAsync(
            "browser_verify_text_visible",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = tab.Page.GetByText(text);
                var count = await locator.CountAsync().ConfigureAwait(false);
                if (count == 0)
                {
                    response.AddError("Text not found");
                    return;
                }

                var visible = false;
                for (var i = 0; i < count; i++)
                {
                    var candidate = locator.Nth(i);
                    if (await candidate.IsVisibleAsync().ConfigureAwait(false))
                    {
                        visible = true;
                        break;
                    }
                }

                if (!visible)
                {
                    response.AddError("Text not found");
                    return;
                }

                response.AddCode($"await expect(page.getByText({QuoteJsString(text)})).toBeVisible();");
                response.AddResult("Done");
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_verify_list_visible")]
    [Description("Verify list is visible on the page.")]
    public static async Task<string> BrowserVerifyListVisibleAsync(
        [Description("Human-readable list description.")] string element,
        [Description("Exact target element reference that points to the list.")] string elementRef,
        [Description("Items to verify.")] string[] items,
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

        items ??= Array.Empty<string>();

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef,
            ["items"] = items.ToArray()
        };

        return await ExecuteWithResponseAsync(
            "browser_verify_list_visible",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(new TabState.RefLocatorRequest(element, elementRef), token).ConfigureAwait(false);

                var collected = new List<string>();
                foreach (var item in items)
                {
                    var itemLocator = locator.GetByText(item);
                    var count = await itemLocator.CountAsync().ConfigureAwait(false);
                    if (count == 0)
                    {
                        response.AddError($"Item \"{item}\" not found");
                        return;
                    }

                    var textContent = await itemLocator.First.TextContentAsync().ConfigureAwait(false) ?? string.Empty;
                    collected.Add(textContent);
                }

                var formattedItems = collected
                    .Select(text => $"  - listitem: {QuoteForResult(text)}");
                var ariaSnapshot = "`\n- list:\n" + string.Join("\n", formattedItems) + "\n`";

                response.AddCode($"await expect(page.locator('body')).toMatchAriaSnapshot({ariaSnapshot});");
                response.AddResult("Done");
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_verify_value")]
    [Description("Verify element value.")]
    public static async Task<string> BrowserVerifyValueAsync(
        [Description("Type of the element (`textbox`/`checkbox`/`radio`/`combobox`/`slider`).")] string type,
        [Description("Human-readable element description.")] string element,
        [Description("Exact target element reference that points to the element.")] string elementRef,
        [Description("Value to verify. For checkbox, use \"true\" or \"false\".")] string value,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type must not be empty.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(element))
        {
            throw new ArgumentException("Element description must not be empty.", nameof(element));
        }

        if (string.IsNullOrWhiteSpace(elementRef))
        {
            throw new ArgumentException("Element ref must not be empty.", nameof(elementRef));
        }

        var normalizedType = NormalizeFieldType(type);
        if (normalizedType == BrowserFillFormFieldType.Unknown)
        {
            throw new ArgumentException($"Unsupported element type '{type}'.", nameof(type));
        }

        value ??= string.Empty;

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["type"] = type,
            ["element"] = element,
            ["ref"] = elementRef,
            ["value"] = value
        };

        return await ExecuteWithResponseAsync(
            "browser_verify_value",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(new TabState.RefLocatorRequest(element, elementRef), token).ConfigureAwait(false);
                var locatorSource = await GenerateLocatorSourceAsync(locator, elementRef, token).ConfigureAwait(false);

                switch (normalizedType)
                {
                    case BrowserFillFormFieldType.Textbox:
                    case BrowserFillFormFieldType.Slider:
                    case BrowserFillFormFieldType.Combobox:
                        {
                            var currentValue = await locator.InputValueAsync().ConfigureAwait(false);
                            if (!string.Equals(currentValue, value, StringComparison.Ordinal))
                            {
                                response.AddError($"Expected value \"{value}\", but got \"{currentValue}\"");
                                return;
                            }

                            response.AddCode($"await expect({locatorSource}).toHaveValue({QuoteJsString(value)});");
                            break;
                        }

                    case BrowserFillFormFieldType.Checkbox:
                    case BrowserFillFormFieldType.Radio:
                        {
                            var expected = ParseBoolean(value, element);
                            var currentValue = await locator.IsCheckedAsync().ConfigureAwait(false);
                            if (currentValue != expected)
                            {
                                response.AddError($"Expected value \"{value}\", but got \"{currentValue.ToString().ToLowerInvariant()}\"");
                                return;
                            }

                            var matcher = currentValue ? "toBeChecked" : "not.toBeChecked";
                            response.AddCode($"await expect({locatorSource}).{matcher}();");
                            break;
                        }
                }

                response.AddResult("Done");
            },
            cancellationToken).ConfigureAwait(false);
    }
}
