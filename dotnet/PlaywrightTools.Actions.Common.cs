using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "common.echo")]
    [Description("Echoes the provided text back to the caller.")]
    public static Task<string> EchoAsync(
        [Description("Text to echo back to the caller.")] string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        text ??= string.Empty;
        return Task.FromResult(Serialize(new
        {
            echoed = text,
            length = text.Length
        }));
    }

    [McpServerTool(Name = "console.read")]
    [Description("Reads messages emitted to the console.")]
    public static Task<string> ConsoleReadAsync(
        [Description("Optional console message type filter (log, warning, error, etc.).")] string? type = null,
        [Description("Maximum number of recent messages to return. Use null for all available messages.")] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = SnapshotConsoleMessages();
        var filtered = string.IsNullOrWhiteSpace(type)
            ? snapshot
            : snapshot.Where(entry => string.Equals(entry.Type, type, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (limit is > 0 && filtered.Length > limit.Value)
        {
            filtered = filtered[^limit.Value..];
        }

        return Task.FromResult(Serialize(new
        {
            count = filtered.Length,
            messages = filtered
        }));
    }

    [McpServerTool(Name = "network.inspect")]
    [Description("Inspects recent network requests.")]
    public static Task<string> NetworkInspectAsync(
        [Description("Maximum number of recent requests to return. Use null for all tracked requests.")] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapshot = SnapshotNetworkRequests();
        if (limit is > 0 && snapshot.Length > limit.Value)
        {
            snapshot = snapshot[^limit.Value..];
        }

        return Task.FromResult(Serialize(new
        {
            count = snapshot.Length,
            requests = snapshot
        }));
    }

    [McpServerTool(Name = "tabs.list")]
    [Description("Lists the currently open tabs.")]
    public static async Task<string> TabsListAsync(
        [Description("Whether to include the HTML title for each tab (may require additional Playwright calls).")] bool includeTitles = true,
        CancellationToken cancellationToken = default)
    {
        var context = await GetContextAsync(cancellationToken).ConfigureAwait(false);
        var pages = context.Pages;

        var tabTasks = pages.Select(async (page, index) => new
        {
            index,
            url = page.Url,
            isActive = page == _page,
            title = includeTitles ? await page.TitleAsync().ConfigureAwait(false) : null,
            isClosed = page.IsClosed
        });

        var tabs = await Task.WhenAll(tabTasks).ConfigureAwait(false);

        return Serialize(new
        {
            activeIndex = GetPageIndex(_page ?? pages.FirstOrDefault() ?? throw new InvalidOperationException("No active page.")),
            tabs
        });
    }

    [McpServerTool(Name = "verify.expect")]
    [Description("Asserts that a condition holds on the page.")]
    public static async Task<string> VerifyExpectAsync(
        [Description("Selector identifying the element to verify.")] string selector,
        [Description("Text that should be contained within the element.")] string expectedSubstring,
        [Description("Timeout in milliseconds to wait for the element to appear before verification.")] int? timeoutMs = null,
        CancellationToken cancellationToken = default)
    {
        var locator = await GetLocatorAsync(selector, timeoutMs, cancellationToken).ConfigureAwait(false);
        var text = await locator.InnerTextAsync().ConfigureAwait(false) ?? string.Empty;
        var passed = text.IndexOf(expectedSubstring, StringComparison.OrdinalIgnoreCase) >= 0;

        return Serialize(new
        {
            passed,
            selector,
            expected = expectedSubstring,
            actual = text
        });
    }

    [McpServerTool(Name = "wait.for")]
    [Description("Waits for a condition within the page to be satisfied.")]
    public static async Task<string> WaitForAsync(
        [Description("Selector to wait for before continuing.")] string selector,
        [Description("Timeout in milliseconds to wait before giving up.")] int timeoutMs = 30000,
        [Description("Which state the selector should reach before resolving (attached, detached, visible, hidden).")] string state = "visible",
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);

        var waitState = state?.ToLowerInvariant() switch
        {
            "attached" => WaitForSelectorState.Attached,
            "detached" => WaitForSelectorState.Detached,
            "hidden" => WaitForSelectorState.Hidden,
            _ => WaitForSelectorState.Visible
        };

        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs,
            State = waitState
        }).ConfigureAwait(false);

        return Serialize(new
        {
            waitedFor = selector,
            timeoutMs,
            state = waitState.ToString()
        });
    }

    [McpServerTool(Name = "snapshot.generate")]
    [Description("Generates a textual snapshot of the page.")]
    public static async Task<string> SnapshotGenerateAsync(
        [Description("Whether to include the full HTML content.")] bool includeHtml = true,
        [Description("Optional selector to extract text content from.")] string? selector = null,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);

        string? content = null;
        if (!string.IsNullOrWhiteSpace(selector))
        {
            var locator = await GetLocatorAsync(selector, null, cancellationToken).ConfigureAwait(false);
            content = await locator.InnerTextAsync().ConfigureAwait(false);
        }
        else if (includeHtml)
        {
            content = await page.ContentAsync().ConfigureAwait(false);
        }
        else
        {
            content = await page.InnerTextAsync("body").ConfigureAwait(false);
        }

        return Serialize(new
        {
            url = page.Url,
            timestamp = DateTimeOffset.UtcNow,
            content
        });
    }
}
