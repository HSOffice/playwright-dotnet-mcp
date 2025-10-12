using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_snapshot")]
    [Description("Capture accessibility snapshot of the current page. This is better than screenshots for understanding structure.")]
    public static async Task<string> BrowserSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);

        return await ExecuteWithResponseAsync(
            "browser_snapshot",
            args,
            async (response, token) =>
            {
                await GetActiveTabAsync(token).ConfigureAwait(false);
                response.AddResult("Captured accessibility snapshot of the current page.");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_click")]
    [Description("Perform click on a web page.")]
    public static async Task<string> BrowserClickAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact accessible name for the target element from the page snapshot.")] string elementRef,
        [Description("Whether to perform a double click instead of a single click.")] bool? doubleClick = null,
        [Description("Button to click, defaults to left.")] string? button = null,
        [Description("Modifier keys to press.")] IReadOnlyList<string>? modifiers = null,
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

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef,
            ["doubleClick"] = doubleClick,
            ["button"] = button,
            ["modifiers"] = modifiers?.ToArray()
        };

        return await ExecuteWithResponseAsync(
            "browser_click",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var resolvedLocator = await ResolveLocatorAsync(tab, element, elementRef, token).ConfigureAwait(false);
                if (resolvedLocator.SnapshotRef is not null)
                {
                    args["ref"] = resolvedLocator.SnapshotRef;
                }
                else
                {
                    args["ref"] = elementRef;
                }

                if (resolvedLocator.RoleName is not null)
                {
                    args["role"] = resolvedLocator.RoleName;
                }

                if (resolvedLocator.AccessibleName is not null)
                {
                    args["name"] = resolvedLocator.AccessibleName;
                }

                var locator = resolvedLocator.Locator;

                var (mouseButton, buttonName) = NormalizeMouseButton(button);
                var (modifierValues, modifierNames) = NormalizeModifiers(modifiers);

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        if (doubleClick == true)
                        {
                            var options = new LocatorDblClickOptions();
                            if (mouseButton is { } dblButton)
                            {
                                options.Button = dblButton;
                            }

                            if (modifierValues.Length > 0)
                            {
                                options.Modifiers = modifierValues;
                            }

                            await locator.DblClickAsync(options).ConfigureAwait(false);
                        }
                        else
                        {
                            var options = new LocatorClickOptions();
                            if (mouseButton is { } clickButton)
                            {
                                options.Button = clickButton;
                            }

                            if (modifierValues.Length > 0)
                            {
                                options.Modifiers = modifierValues;
                            }

                            await locator.ClickAsync(options).ConfigureAwait(false);
                        }
                    }
                    catch (PlaywrightException ex) when (resolvedLocator.AccessibleName is not null && resolvedLocator.RoleName is not null)
                    {
                        throw CreateLocatorException(element, resolvedLocator.RoleName, resolvedLocator.AccessibleName, ex);
                    }
                }, token).ConfigureAwait(false);

                var locatorSource = resolvedLocator.LocatorSource;
                var method = doubleClick == true ? "dblclick" : "click";
                var optionsLiteral = FormatClickOptionsLiteral(buttonName, modifierNames);
                response.AddCode(optionsLiteral is null
                    ? $"await {locatorSource}.{method}();"
                    : $"await {locatorSource}.{method}({optionsLiteral});");
                response.AddResult(doubleClick == true
                    ? $"Double clicked {element}."
                    : $"Clicked {element}.");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_drag")]
    [Description("Perform drag and drop between two elements.")]
    public static async Task<string> BrowserDragAsync(
        [Description("Human-readable source element description used to obtain permission to interact with the element.")] string startElement,
        [Description("Exact source element reference from the page snapshot.")] string startRef,
        [Description("Human-readable target element description used to obtain permission to interact with the element.")] string endElement,
        [Description("Exact target element reference from the page snapshot.")] string endRef,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(startElement))
        {
            throw new ArgumentException("Start element description must not be empty.", nameof(startElement));
        }

        if (string.IsNullOrWhiteSpace(endElement))
        {
            throw new ArgumentException("End element description must not be empty.", nameof(endElement));
        }

        if (string.IsNullOrWhiteSpace(startRef))
        {
            throw new ArgumentException("Start element ref must not be empty.", nameof(startRef));
        }

        if (string.IsNullOrWhiteSpace(endRef))
        {
            throw new ArgumentException("End element ref must not be empty.", nameof(endRef));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["startElement"] = startElement,
            ["startRef"] = startRef,
            ["endElement"] = endElement,
            ["endRef"] = endRef
        };

        return await ExecuteWithResponseAsync(
            "browser_drag",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locators = await tab.GetLocatorsByRefAsync(new[]
                {
                    new TabState.RefLocatorRequest(startElement, startRef),
                    new TabState.RefLocatorRequest(endElement, endRef)
                }, token).ConfigureAwait(false);

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await locators[0].DragToAsync(locators[1]).ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                var startSource = $"page.locator({QuoteJsString($"aria-ref={startRef}")})";
                var endSource = $"page.locator({QuoteJsString($"aria-ref={endRef}")})";
                response.AddCode($"await {startSource}.dragTo({endSource});");
                response.AddResult($"Dragged {startElement} to {endElement}.");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_hover")]
    [Description("Hover over element on page.")]
    public static async Task<string> BrowserHoverAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact target element reference from the page snapshot.")] string elementRef,
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

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef
        };

        return await ExecuteWithResponseAsync(
            "browser_hover",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(element, elementRef, token).ConfigureAwait(false);

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await locator.HoverAsync().ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                response.AddCode($"await page.locator({QuoteJsString($"aria-ref={elementRef}")}).hover();");
                response.AddResult($"Hovered over {element}.");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_select_option")]
    [Description("Select an option in a dropdown.")]
    public static async Task<string> BrowserSelectOptionAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact target element reference from the page snapshot.")] string elementRef,
        [Description("Array of values to select in the dropdown. This can be a single value or multiple values.")] IReadOnlyList<string> values,
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

        if (values is null || values.Count == 0)
        {
            throw new ArgumentException("At least one value must be provided.", nameof(values));
        }

        var normalizedValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        if (normalizedValues.Length == 0)
        {
            throw new ArgumentException("At least one non-empty value must be provided.", nameof(values));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef,
            ["values"] = normalizedValues
        };

        return await ExecuteWithResponseAsync(
            "browser_select_option",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(element, elementRef, token).ConfigureAwait(false);
                var options = normalizedValues.Select(value => new SelectOptionValue { Value = value }).ToArray();

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await locator.SelectOptionAsync(options).ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                response.AddCode($"await page.locator({QuoteJsString($"aria-ref={elementRef}")}).selectOption({FormatJsArray(normalizedValues)});");
                response.AddResult($"Selected {string.Join(", ", normalizedValues.Select(v => QuoteForResult(v)))} in {element}.");
                response.SetIncludeSnapshot();
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_generate_locator")]
    [Description("Generate locator for the given element to use in tests.")]
    public static async Task<string> BrowserGenerateLocatorAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact target element reference from the page snapshot.")] string elementRef,
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

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["ref"] = elementRef
        };

        return await ExecuteWithResponseAsync(
            "browser_generate_locator",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var locator = await tab.GetLocatorByRefAsync(element, elementRef, token).ConfigureAwait(false);
                var descriptor = await GenerateLocatorSourceAsync(locator, elementRef, token).ConfigureAwait(false);
                response.AddResult(descriptor);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static (MouseButton? Value, string? Name) NormalizeMouseButton(string? button)
    {
        if (string.IsNullOrWhiteSpace(button))
        {
            return (null, null);
        }

        var normalized = button.Trim().ToLowerInvariant();
        return normalized switch
        {
            "left" => (MouseButton.Left, "left"),
            "right" => (MouseButton.Right, "right"),
            "middle" => (MouseButton.Middle, "middle"),
            _ => throw new ArgumentException($"Unsupported mouse button '{button}'.", nameof(button))
        };
    }

    private static (KeyboardModifier[] Values, string[] Names) NormalizeModifiers(IReadOnlyList<string>? modifiers)
    {
        if (modifiers is null || modifiers.Count == 0)
        {
            return (Array.Empty<KeyboardModifier>(), Array.Empty<string>());
        }

        var values = new List<KeyboardModifier>();
        var names = new List<string>();

        foreach (var modifier in modifiers)
        {
            if (string.IsNullOrWhiteSpace(modifier))
            {
                continue;
            }

            var normalized = modifier.Trim();
            switch (normalized.ToLowerInvariant())
            {
                case "alt":
                    values.Add(KeyboardModifier.Alt);
                    names.Add("Alt");
                    break;
                case "control":
                case "ctrl":
                    values.Add(KeyboardModifier.Control);
                    names.Add("Control");
                    break;
                case "controlormeta":
                    values.Add(KeyboardModifier.ControlOrMeta);
                    names.Add("ControlOrMeta");
                    break;
                case "meta":
                    values.Add(KeyboardModifier.Meta);
                    names.Add("Meta");
                    break;
                case "shift":
                    values.Add(KeyboardModifier.Shift);
                    names.Add("Shift");
                    break;
                default:
                    throw new ArgumentException($"Unsupported modifier '{modifier}'.", nameof(modifiers));
            }
        }

        return (values.ToArray(), names.ToArray());
    }

    private static string? FormatClickOptionsLiteral(string? button, IReadOnlyList<string> modifiers)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(button))
        {
            parts.Add($"button: {QuoteJsString(button)}");
        }

        if (modifiers.Count > 0)
        {
            parts.Add($"modifiers: [{string.Join(", ", modifiers.Select(QuoteJsString))}]");
        }

        if (parts.Count == 0)
        {
            return null;
        }

        return "{ " + string.Join(", ", parts) + " }";
    }

    private static string FormatJsArray(IReadOnlyList<string> values)
        => "[" + string.Join(", ", values.Select(QuoteJsString)) + "]";

    private static string QuoteForResult(string value)
        => $"\"{value.Replace("\"", "\\\"")}\"";

    private static Task<string> GenerateLocatorSourceAsync(ILocator locator, string elementRef, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(locator);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"page.locator({QuoteJsString($"aria-ref={elementRef}")})");
    }

    private static async Task<ResolvedLocator> ResolveLocatorAsync(TabState tab, string elementDescription, string elementRef, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tab);
        cancellationToken.ThrowIfCancellationRequested();

        var descriptor = ParseElementReference(elementRef);

        if (!string.IsNullOrWhiteSpace(descriptor.Role) && !string.IsNullOrWhiteSpace(descriptor.Name))
        {
            var (role, roleName) = NormalizeAriaRole(descriptor.Role);
            var options = new PageGetByRoleOptions { Name = descriptor.Name };
            var locator = tab.Page.GetByRole(role, options);
            return new ResolvedLocator(locator, $"page.getByRole({QuoteJsString(roleName)}, {{ name: {QuoteJsString(descriptor.Name)} }})", null, roleName, descriptor.Name);
        }

        if (string.IsNullOrWhiteSpace(descriptor.Ref))
        {
            throw new ArgumentException("Element ref must include either 'ref' or both 'role' and 'name'.", nameof(elementRef));
        }

        var locatorByRef = await tab.GetLocatorByRefAsync(elementDescription, descriptor.Ref, cancellationToken).ConfigureAwait(false);
        return new ResolvedLocator(locatorByRef, $"page.locator({QuoteJsString($"aria-ref={descriptor.Ref}")})", descriptor.Ref, null, null);
    }

    private static (AriaRole Role, string RoleName) NormalizeAriaRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role must not be empty.", nameof(role));
        }

        var normalized = role.Trim();
        if (!Enum.TryParse(normalized, ignoreCase: true, out AriaRole parsed))
        {
            throw new ArgumentException($"Unsupported ARIA role '{role}'.", nameof(role));
        }

        return (parsed, parsed.ToString().ToLowerInvariant());
    }

    private static ElementReferenceDescriptor ParseElementReference(string elementRef)
    {
        if (string.IsNullOrWhiteSpace(elementRef))
        {
            return new ElementReferenceDescriptor(null, null, null);
        }

        var trimmed = elementRef.Trim();
        if (trimmed.Length > 0 && trimmed[0] == '{')
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                var root = document.RootElement;
                var refValue = GetOptionalString(root, "ref");
                var role = GetOptionalString(root, "role");
                var name = GetOptionalString(root, "name") ?? GetOptionalString(root, "accessibleName");

                if (refValue is not null || (role is not null && name is not null))
                {
                    return new ElementReferenceDescriptor(refValue, role, name);
                }
            }
            catch (JsonException)
            {
                // Ignore malformed JSON and fall back to treating the value as a legacy ref identifier.
            }
        }

        return new ElementReferenceDescriptor(elementRef, null, null);
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString();
            }
        }

        return null;
    }

    private static Exception CreateLocatorException(string elementDescription, string roleName, string accessibleName, PlaywrightException inner)
        => new InvalidOperationException($"Unable to locate element '{elementDescription}' with role '{roleName}' and name '{accessibleName}'. Capture a new snapshot and try again.", inner);

    private readonly record struct ElementReferenceDescriptor(string? Ref, string? Role, string? Name);

    private readonly record struct ResolvedLocator(ILocator Locator, string LocatorSource, string? SnapshotRef, string? RoleName, string? AccessibleName);
}
