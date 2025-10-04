using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "navigate.go")]
    [Description("Navigates the active tab to a new URL.")]
    public static async Task<string> NavigateGoAsync(
        [Description("The URL to navigate to.")] string url,
        [Description("Navigation timeout in milliseconds. Use null to apply Playwright defaults.")] int? timeoutMs = null,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return Serialize(new
            {
                navigated = false,
                url,
                error = "Invalid URL format."
            });
        }

        var response = await page.GotoAsync(parsed.ToString(), new PageGotoOptions
        {
            Timeout = timeoutMs,
            WaitUntil = WaitUntilState.NetworkIdle
        }).ConfigureAwait(false);

        var title = await page.TitleAsync().ConfigureAwait(false);

        return Serialize(new
        {
            navigated = true,
            url = page.Url,
            status = response?.Status,
            title
        });
    }

    [McpServerTool(Name = "screenshot.capture")]
    [Description("Captures a screenshot of the current page.")]
    public static async Task<string> ScreenshotCaptureAsync(
        [Description("Optional output file name. When omitted a timestamped name will be generated.")] string? fileName = null,
        [Description("Capture the entire scrollable page instead of the viewport.")] bool fullPage = false,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        EnsureDirectories();

        var name = string.IsNullOrWhiteSpace(fileName)
            ? $"screenshot-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.png"
            : fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ".png";

        var path = ResolveOutputPath(name, ShotsDir);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = fullPage
        }).ConfigureAwait(false);

        return Serialize(new
        {
            captured = true,
            path,
            fullPage
        });
    }

    [McpServerTool(Name = "pdf.export")]
    [Description("Exports the current page as a PDF.")]
    public static async Task<string> PdfExportAsync(
        [Description("Optional output file name. When omitted a timestamped name will be generated.")] string? fileName = null,
        [Description("Paper format such as A4, Letter, Legal, etc.")] string format = "A4",
        CancellationToken cancellationToken = default)
    {
        if (!_isChromium)
        {
            return Serialize(new
            {
                exported = false,
                error = "PDF export is only supported when using a Chromium-based browser."
            });
        }

        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        EnsureDirectories();

        var name = string.IsNullOrWhiteSpace(fileName)
            ? $"page-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.pdf"
            : fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ".pdf";

        var path = ResolveOutputPath(name, PdfDir);

        await page.PdfAsync(new PagePdfOptions
        {
            Path = path,
            Format = format
        }).ConfigureAwait(false);

        return Serialize(new
        {
            exported = true,
            path,
            format
        });
    }

    [McpServerTool(Name = "tracing.start")]
    [Description("Starts capturing a Playwright trace.")]
    public static async Task<string> TracingStartAsync(
        [Description("Optional human readable title for the trace session.")] string? title = null,
        [Description("Include JavaScript source files in the trace.")] bool includeSources = true,
        [Description("Capture screenshots during tracing.")] bool includeScreenshots = true,
        [Description("Capture DOM snapshots during tracing.")] bool includeSnapshots = true,
        CancellationToken cancellationToken = default)
    {
        if (_tracingActive)
        {
            return Serialize(new
            {
                tracing = "already_active"
            });
        }

        var context = await GetContextAsync(cancellationToken).ConfigureAwait(false);

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Title = title,
            Sources = includeSources,
            Screenshots = includeScreenshots,
            Snapshots = includeSnapshots
        }).ConfigureAwait(false);

        _tracingActive = true;

        return Serialize(new
        {
            tracing = "started",
            title,
            includeSources,
            includeScreenshots,
            includeSnapshots
        });
    }
}
