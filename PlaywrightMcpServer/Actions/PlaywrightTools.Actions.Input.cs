using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "form.fill")]
    [Description("Fills a form field with the specified value.")]
    public static async Task<string> FormFillAsync(
        [Description("Playwright selector identifying the target element.")] string selector,
        [Description("Value to fill into the field.")] string value,
        [Description("Timeout in milliseconds for locating the element.")] int? timeoutMs = null,
        CancellationToken cancellationToken = default)
    {
        var locator = await GetLocatorAsync(selector, timeoutMs, cancellationToken).ConfigureAwait(false);
        await locator.FillAsync(value ?? string.Empty, new LocatorFillOptions
        {
            Timeout = timeoutMs
        }).ConfigureAwait(false);

        return Serialize(new
        {
            filled = true,
            selector,
            value
        });
    }

    [McpServerTool(Name = "keyboard.type")]
    [Description("Types text into the focused element.")]
    public static async Task<string> KeyboardTypeAsync(
        [Description("Text to type into the active element.")] string text,
        [Description("Delay between individual key presses in milliseconds.")] int? delayMs = null,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        await page.Keyboard.TypeAsync(text ?? string.Empty, new KeyboardTypeOptions
        {
            Delay = delayMs
        }).ConfigureAwait(false);

        return Serialize(new
        {
            typed = text ?? string.Empty,
            delayMs
        });
    }

    [McpServerTool(Name = "mouse.click")]
    [Description("Performs a mouse click on a target element.")]
    public static async Task<string> MouseClickAsync(
        [Description("Playwright selector identifying the element to click.")] string selector,
        [Description("Mouse button to use (left, middle, right).")] string? button = null,
        [Description("Number of consecutive clicks to perform.")] int? clickCount = null,
        [Description("Delay between mouse down and mouse up in milliseconds.")] int? delayMs = null,
        [Description("Timeout in milliseconds for locating the element.")] int? timeoutMs = null,
        [Description("Optional keyboard modifiers (Alt, Control, Meta, Shift, ControlOrMeta).")] string[]? modifiers = null,
        CancellationToken cancellationToken = default)
    {
        var locator = await GetLocatorAsync(selector, timeoutMs, cancellationToken).ConfigureAwait(false);
        var parsedButton = ParseMouseButton(button);
        var parsedModifiers = ParseModifiers(modifiers)?.ToArray();

        await locator.ClickAsync(new LocatorClickOptions
        {
            Button = parsedButton,
            ClickCount = clickCount,
            Delay = delayMs,
            Timeout = timeoutMs,
            Modifiers = parsedModifiers
        }).ConfigureAwait(false);

        return Serialize(new
        {
            clicked = true,
            selector,
            button = parsedButton?.ToString() ?? "Left",
            clickCount = clickCount ?? 1,
            delayMs
        });
    }

    [McpServerTool(Name = "files.save")]
    [Description("Saves a file produced by the browser session.")]
    public static async Task<string> FilesSaveAsync(
        [Description("Base64 encoded file contents.")] string base64Content,
        [Description("Desired file name relative to the downloads directory.")] string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureDirectories();

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64Content ?? string.Empty);
        }
        catch (FormatException ex)
        {
            return Serialize(new
            {
                saved = false,
                error = ex.Message
            });
        }

        var name = string.IsNullOrWhiteSpace(fileName)
            ? $"download-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.bin"
            : fileName;

        var path = ResolveOutputPath(name, DownloadsDir);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);

        return Serialize(new
        {
            saved = true,
            path,
            size = bytes.Length
        });
    }
}
