using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_take_screenshot")]
    [Description("Take a screenshot of the current page.")]
    public static async Task<string> BrowserTakeScreenshotAsync(
        [Description("Image format for the screenshot. Default is png.")] string? type = null,
        [Description("File name to save the screenshot to. Defaults to an auto-generated name if not specified.")] string? filename = null,
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string? element = null,
        [Description("Exact target element reference from the page snapshot.")] string? elementRef = null,
        [Description("When true, takes a screenshot of the full scrollable page, instead of the currently visible viewport. Cannot be used with element screenshots.")] bool? fullPage = null,
        CancellationToken cancellationToken = default)
    {
        var format = string.IsNullOrWhiteSpace(type) ? "png" : type.Trim().ToLowerInvariant();
        if (format is not ("png" or "jpeg"))
        {
            throw new ArgumentException("Screenshot type must be either 'png' or 'jpeg'.", nameof(type));
        }

        var hasElement = !string.IsNullOrWhiteSpace(element) || !string.IsNullOrWhiteSpace(elementRef);
        if (hasElement && (string.IsNullOrWhiteSpace(element) || string.IsNullOrWhiteSpace(elementRef)))
        {
            throw new ArgumentException("Both element and ref must be provided for element screenshots.");
        }

        if (fullPage == true && hasElement)
        {
            throw new ArgumentException("fullPage cannot be used with element screenshots.", nameof(fullPage));
        }

        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["type"] = format,
            ["filename"] = filename,
            ["element"] = element,
            ["ref"] = elementRef,
            ["fullPage"] = fullPage
        };

        return await ExecuteWithResponseAsync(
            "browser_take_screenshot",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var fileName = string.IsNullOrWhiteSpace(filename)
                    ? GenerateTimestampedFileName(format)
                    : filename!;
                var outputPath = ResolveShotsOutputPath(fileName);
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var isElementScreenshot = hasElement;
                var screenshotTarget = isElementScreenshot
                    ? element!
                    : fullPage == true ? "full page" : "viewport";

                response.AddCode($"// Screenshot {screenshotTarget} and save it as {outputPath}");

                byte[] buffer;

                if (isElementScreenshot)
                {
                    var locator = await tab.GetLocatorByRefAsync(new TabState.RefLocatorRequest(element!, elementRef!), token).ConfigureAwait(false);
                    var locatorSource = await GenerateLocatorSourceAsync(locator, elementRef!, token).ConfigureAwait(false);

                    var locatorOptions = new LocatorScreenshotOptions
                    {
                        Path = outputPath,
                        Type = format == "png" ? ScreenshotType.Png : ScreenshotType.Jpeg,
                        Quality = format == "jpeg" ? 90 : null,
                        Scale = ScreenshotScale.Css
                    };

                    response.AddCode($"await {locatorSource}.screenshot({{ path: {QuoteJsString(outputPath)}, type: {QuoteJsString(format)}, quality: {(format == "jpeg" ? "90" : "undefined")} }});");
                    buffer = await locator.ScreenshotAsync(locatorOptions).ConfigureAwait(false);
                }
                else
                {
                    var pageOptions = new PageScreenshotOptions
                    {
                        Path = outputPath,
                        Type = format == "png" ? ScreenshotType.Png : ScreenshotType.Jpeg,
                        Quality = format == "jpeg" ? 90 : null,
                        Scale = ScreenshotScale.Css,
                        FullPage = fullPage
                    };

                    var optionsLiteral = format == "jpeg"
                        ? $"{{ path: {QuoteJsString(outputPath)}, type: {QuoteJsString(format)}, quality: 90, scale: 'css'{(fullPage is null ? string.Empty : ", fullPage: " + (fullPage == true ? "true" : "false"))} }}"
                        : $"{{ path: {QuoteJsString(outputPath)}, type: {QuoteJsString(format)}, scale: 'css'{(fullPage is null ? string.Empty : ", fullPage: " + (fullPage == true ? "true" : "false"))} }}";

                    response.AddCode($"await page.screenshot({optionsLiteral});");
                    buffer = await tab.Page.ScreenshotAsync(pageOptions).ConfigureAwait(false);
                }

                response.AddResult($"Took the {screenshotTarget} screenshot and saved it as {outputPath}");

                if (fullPage != true)
                {
                    var contentType = format == "png" ? "image/png" : "image/jpeg";
                    response.AddImage(contentType, buffer);
                }
            },
            cancellationToken).ConfigureAwait(false);
    }
}
