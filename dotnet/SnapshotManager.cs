using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

internal sealed class SnapshotManager
{
    public async Task<SnapshotPayload> CaptureAsync(TabState tab, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tab);
        cancellationToken.ThrowIfCancellationRequested();

        var page = tab.Page;
        string? ariaSnapshot = null;
        try
        {
            ariaSnapshot = await TryGetAriaSnapshotAsync(page, cancellationToken).ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
            // Fallback: still produce snapshot metadata when accessibility tree is unavailable.
        }

        var title = await page.TitleAsync().ConfigureAwait(false);
        var url = page.Url ?? string.Empty;
        var (console, network) = tab.TakeActivitySnapshot();

        var snapshotPayload = new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = url,
            Title = title,
            AriaSnapshot = ariaSnapshot,
            Console = console,
            Network = network,
            ModalStates = tab.GetModalStatesSnapshot(),
            Downloads = tab.GetDownloadsSnapshot()
        };

        tab.UpdateMetadata(url, title, snapshotPayload);
        return snapshotPayload;
    }

    private static async Task<string?> TryGetAriaSnapshotAsync(IPage page, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        //var yaml = await page.Locator("html").AriaSnapshotAsync(new()).ConfigureAwait(false);
        var yaml = await page.GetInteractiveAriaSnapshotAsync(
            keepHeadings: false,
            keepUrls: false
        );
        return yaml;
    }
}
