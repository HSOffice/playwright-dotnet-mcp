using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_fill_form")]
    [Description("Fill multiple form fields.")]
    public static async Task<string> BrowserFillFormAsync(
        [Description("Fields to fill in (name, type, ref, value).")] JsonElement fields,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (fields.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("Fields payload must be an array.", nameof(fields));
        }

        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);

        var parsedFields = ParseFieldDescriptors(fields);
        if (parsedFields.Count == 0)
        {
            throw new ArgumentException("At least one field descriptor is required.", nameof(fields));
        }

        var results = new List<object>(parsedFields.Count);
        var allSucceeded = true;

        foreach (var descriptor in parsedFields)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fieldResult = new Dictionary<string, object?>
            {
                ["name"] = descriptor.Name,
                ["type"] = descriptor.Type,
                ["ref"] = descriptor.Reference,
                ["value"] = descriptor.Value
            };

            try
            {
                var locator = await ResolveFieldLocatorAsync(tab, descriptor, cancellationToken).ConfigureAwait(false);

                switch (descriptor.Kind)
                {
                    case FormFieldType.Textbox:
                    case FormFieldType.Slider:
                        await locator.FillAsync(descriptor.Value ?? string.Empty).ConfigureAwait(false);
                        break;
                    case FormFieldType.Checkbox:
                    case FormFieldType.Radio:
                        await locator.SetCheckedAsync(ParseBooleanValue(descriptor.Value)).ConfigureAwait(false);
                        break;
                    case FormFieldType.Combobox:
                        await locator.SelectOptionAsync(new[]
                        {
                            new SelectOptionValue { Label = descriptor.Value ?? string.Empty }
                        }).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported field type '{descriptor.Type}'.");
                }

                fieldResult["success"] = true;
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                fieldResult["success"] = false;
                fieldResult["error"] = ex.Message;
            }

            results.Add(fieldResult);
        }

        var snapshot = await SnapshotManager.CaptureAsync(tab, cancellationToken).ConfigureAwait(false);

        var response = new
        {
            filled = allSucceeded,
            results,
            snapshot,
            tabs = TabManager.DescribeTabs()
        };

        return Serialize(response);
    }

    private static List<FormFieldDescriptor> ParseFieldDescriptors(JsonElement fields)
    {
        var list = new List<FormFieldDescriptor>();

        foreach (var element in fields.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!element.TryGetProperty("name", out var nameProperty) || nameProperty.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Each field descriptor must include a string 'name'.");
            }

            if (!element.TryGetProperty("type", out var typeProperty) || typeProperty.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Each field descriptor must include a string 'type'.");
            }

            if (!element.TryGetProperty("ref", out var refProperty) || refProperty.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Each field descriptor must include a string 'ref'.");
            }

            element.TryGetProperty("value", out var valueProperty);

            var descriptor = new FormFieldDescriptor(
                nameProperty.GetString() ?? string.Empty,
                typeProperty.GetString() ?? string.Empty,
                refProperty.GetString() ?? string.Empty,
                valueProperty.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False
                    ? valueProperty.ToString()
                    : null);

            list.Add(descriptor);
        }

        return list;
    }

    private static async Task<ILocator> ResolveFieldLocatorAsync(TabState tab, FormFieldDescriptor descriptor, CancellationToken cancellationToken)
    {
        var reference = descriptor.Reference;
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Field reference must not be empty.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var page = tab.Page;
        var normalizedRef = reference.Trim();

        // Mirror TypeScript behaviour: ensure the reference still exists in the latest snapshot when available.
        var lastSnapshot = tab.LastSnapshot?.Aria?.GetRawText();
        if (lastSnapshot is not null
            && !lastSnapshot.Contains($"[ref={normalizedRef}]", StringComparison.Ordinal)
            && !lastSnapshot.Contains($"\"ref\":\"{normalizedRef}\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Ref '{normalizedRef}' not found in the current page snapshot. Capture a new snapshot and retry.");
        }

        var locator = page.Locator($"css=[aria-ref=\"{EscapeSelector(normalizedRef)}\"]");
        if (!await LocatorExistsAsync(locator).ConfigureAwait(false))
        {
            locator = page.Locator($"css=[data-ref=\"{EscapeSelector(normalizedRef)}\"]");
        }

        if (!await LocatorExistsAsync(locator).ConfigureAwait(false))
        {
            locator = page.Locator($"css=[data-playwright-ref=\"{EscapeSelector(normalizedRef)}\"]");
        }

        if (!await LocatorExistsAsync(locator).ConfigureAwait(false))
        {
            locator = page.Locator($"css=[data-mcp-ref=\"{EscapeSelector(normalizedRef)}\"]");
        }

        if (!await LocatorExistsAsync(locator).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"Unable to resolve element with ref '{normalizedRef}'.");
        }

        return locator.First;
    }

    private static async Task<bool> LocatorExistsAsync(ILocator locator)
    {
        try
        {
            await locator.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 500
            }).ConfigureAwait(false);
            return true;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    private static bool ParseBooleanValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeSelector(string value) => value
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("[", "\\[")
        .Replace("]", "\\]")
        .Replace("'", "\\'");

    private sealed record FormFieldDescriptor(string Name, string Type, string Reference, string? Value)
    {
        public FormFieldType Kind => Type.ToLowerInvariant() switch
        {
            "textbox" => FormFieldType.Textbox,
            "slider" => FormFieldType.Slider,
            "checkbox" => FormFieldType.Checkbox,
            "radio" => FormFieldType.Radio,
            "combobox" => FormFieldType.Combobox,
            _ => FormFieldType.Unknown
        };
    }

    private enum FormFieldType
    {
        Unknown,
        Textbox,
        Checkbox,
        Radio,
        Combobox,
        Slider
    }
}
