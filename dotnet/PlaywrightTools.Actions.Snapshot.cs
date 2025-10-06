using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

/*
 * NOTE: TypeScript version of `browser_snapshot` only toggles `response.setIncludeSnapshot()` and defers
 * snapshot collection to the response pipeline (`Response.finish()`). The .NET MCP server lacks that
 * centralized injection step, so we capture and serialize the accessibility tree immediately. This keeps
 * the architecture simpler, surfaces page state to the LLM right away, and improves observability during
 * integration and debugging.
 */
public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_snapshot")]
    [Description("Capture accessibility snapshot of the current page, this is better than screenshot.")]
    public static async Task<string> BrowserSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
        var page = tab.Page;

        object? accessibilitySnapshot = null;
        try
        {
            accessibilitySnapshot = await page.Accessibility.SnapshotAsync(new AccessibilitySnapshotOptions
            {
                InterestingOnly = false
            }).ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
            // Align with SnapshotManager: swallow accessibility errors so the tool still succeeds.
        }

        var title = await page.TitleAsync().ConfigureAwait(false);

        var payload = new
        {
            includeSnapshot = true,
            url = page.Url,
            title,
            snapshot = accessibilitySnapshot
        };

        return Serialize(payload);
    }
}
