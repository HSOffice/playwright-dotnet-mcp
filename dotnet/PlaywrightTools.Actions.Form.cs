using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
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
        [Description("Request payload containing the fields to fill in.")] BrowserFillFormRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(request);

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["fields"] = request.Fields?.Count ?? 0
        };

        return await ExecuteWithResponseAsync(
            "browser_fill_form",
            args,
            async (response, token) =>
            {
                if (request.Fields is null || request.Fields.Count == 0)
                {
                    response.AddResult("No form fields were provided.");
                    response.SetIncludeTabs();
                    return;
                }

                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var page = tab.Page;
                var updates = new List<string>();

                foreach (var field in request.Fields)
                {
                    token.ThrowIfCancellationRequested();

                    if (field is null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(field.Reference))
                    {
                        throw new ArgumentException($"Field \"{field.Name}\" is missing the required \"ref\" value.");
                    }

                    var normalizedType = NormalizeFieldType(field.Type);
                    if (normalizedType == BrowserFillFormFieldType.Unknown)
                    {
                        throw new ArgumentException($"Unsupported field type '{field.Type}'.");
                    }

                    var locator = page.Locator($"aria-ref={field.Reference}");
                    var locatorSource = $"await page.locator({QuoteJsString($"aria-ref={field.Reference}")})";

                    try
                    {
                        switch (normalizedType)
                        {
                            case BrowserFillFormFieldType.Textbox:
                            case BrowserFillFormFieldType.Slider:
                                {
                                    var secret = LookupSecret(field.Value);
                                    await locator.FillAsync(secret.Value).ConfigureAwait(false);
                                    response.AddCode($"{locatorSource}.fill({secret.Code});");
                                    updates.Add($"- Filled {DescribeField(field)}.");
                                    break;
                                }

                            case BrowserFillFormFieldType.Checkbox:
                            case BrowserFillFormFieldType.Radio:
                                {
                                    var isChecked = ParseBoolean(field.Value, field.Name);
                                    await locator.SetCheckedAsync(isChecked).ConfigureAwait(false);
                                    var literal = isChecked ? "true" : "false";
                                    response.AddCode($"{locatorSource}.setChecked({literal});");
                                    updates.Add($"- Set {DescribeField(field)} to {(isChecked ? "checked" : "unchecked")}.");
                                    break;
                                }

                            case BrowserFillFormFieldType.Combobox:
                                {
                                    var optionLabel = field.Value ?? string.Empty;
                                    await locator.SelectOptionAsync(new[] { new SelectOptionValue { Label = optionLabel } }).ConfigureAwait(false);
                                    response.AddCode($"{locatorSource}.selectOption({QuoteJsString(optionLabel)});");
                                    updates.Add($"- Selected \"{optionLabel}\" for {DescribeField(field)}.");
                                    break;
                                }
                        }
                    }
                    catch (PlaywrightException ex)
                    {
                        throw CreateLocatorException(field, ex);
                    }
                }

                if (updates.Count > 0)
                {
                    response.AddResult("Updated form fields:\n" + string.Join("\n", updates));
                }
                else
                {
                    response.AddResult("No form fields were updated.");
                }

                response.SetIncludeSnapshot();
                response.SetIncludeTabs();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [Description("Payload describing all fields to populate in the form.")]
    public sealed class BrowserFillFormRequest
    {
        [Description("Fields to fill in.")]
        [JsonPropertyName("fields")]
        public IReadOnlyList<BrowserFillFormField> Fields { get; init; } = Array.Empty<BrowserFillFormField>();
    }

    [Description("Descriptor for a single form field interaction.")]
    public sealed class BrowserFillFormField
    {
        [Description("Human-readable field name.")]
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [Description("Type of the field (textbox, checkbox, radio, combobox, slider).")]
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [Description("Exact target field reference from the page snapshot.")]
        [JsonPropertyName("ref")]
        public string Reference { get; init; } = string.Empty;

        [Description("Value to fill in the field. If the field is a checkbox, the value should be `true` or `false`. If the field is a combobox, the value should be the text of the option.")]
        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }

    private enum BrowserFillFormFieldType
    {
        Unknown,
        Textbox,
        Checkbox,
        Radio,
        Combobox,
        Slider
    }

    private static BrowserFillFormFieldType NormalizeFieldType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return BrowserFillFormFieldType.Unknown;
        }

        return type.Trim().ToLowerInvariant() switch
        {
            "textbox" => BrowserFillFormFieldType.Textbox,
            "checkbox" => BrowserFillFormFieldType.Checkbox,
            "radio" => BrowserFillFormFieldType.Radio,
            "combobox" => BrowserFillFormFieldType.Combobox,
            "slider" => BrowserFillFormFieldType.Slider,
            _ => BrowserFillFormFieldType.Unknown
        };
    }

    private static (string Value, string Code) LookupSecret(string? candidate)
    {
        var normalized = candidate ?? string.Empty;

        if (ResponseConfiguration.Secrets is { } secrets && secrets.TryGetValue(normalized, out var secretValue) && !string.IsNullOrEmpty(secretValue))
        {
            return (secretValue, $"process.env[{QuoteJsString(normalized)}]");
        }

        return (normalized, QuoteJsString(normalized));
    }

    private static bool ParseBoolean(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Field \"{fieldName}\" requires a boolean value of 'true' or 'false'.");
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Value '{value}' for field \"{fieldName}\" is not a valid boolean literal. Use 'true' or 'false'.");
    }

    private static string DescribeField(BrowserFillFormField field)
    {
        var name = string.IsNullOrWhiteSpace(field.Name) ? "field" : field.Name;
        var type = string.IsNullOrWhiteSpace(field.Type) ? "unknown" : field.Type;
        return $"{name} ({type})";
    }

    private static string QuoteJsString(string? value)
    {
        value ??= string.Empty;

        return "'" + value
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("'", "\\'")
            + "'";
    }

    private static Exception CreateLocatorException(BrowserFillFormField field, PlaywrightException inner)
    {
        var name = string.IsNullOrWhiteSpace(field.Name) ? field.Reference : field.Name;
        return new InvalidOperationException(
            $"Unable to locate element '{name}' with ref '{field.Reference}'. Capture a new snapshot and try again.",
            inner);
    }
}
