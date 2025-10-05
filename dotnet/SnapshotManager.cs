using System;
using System.Text.Json;
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
        JsonElement? aria = null;
        try
        {
            var accessibilitySnapshot = await page.Accessibility.SnapshotAsync(new AccessibilitySnapshotOptions
            {
                InterestingOnly = false
            }).ConfigureAwait(false);

            if (accessibilitySnapshot is not null)
            {
                aria = JsonSerializer.SerializeToElement(accessibilitySnapshot, accessibilitySnapshot.GetType());
            }
        }
        catch (PlaywrightException)
        {
            // Fallback: still produce snapshot metadata when accessibility tree is unavailable.
        }

        var title = await page.TitleAsync().ConfigureAwait(false);
        var url = page.Url ?? string.Empty;
        var (console, network) = tab.TakeActivitySnapshot();

        var snapshot = new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = url,
            Title = title,
            Aria = aria,
            Console = console,
            Network = network
        };

        tab.UpdateMetadata(url, title, snapshot);
        return snapshot;
    }
}
