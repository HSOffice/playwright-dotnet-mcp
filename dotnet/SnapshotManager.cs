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
            var accessibilitySnapshot = await GetAccessibilitySnapshotAsync(page, cancellationToken).ConfigureAwait(false);

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

        var snapshotPayload = new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = url,
            Title = title,
            Aria = aria,
            Console = console,
            Network = network,
            ModalStates = tab.GetModalStatesSnapshot(),
            Downloads = tab.GetDownloadsSnapshot()
        };

        tab.UpdateMetadata(url, title, snapshotPayload);
        return snapshotPayload;
    }

    private static async Task<object?> GetAccessibilitySnapshotAsync(IPage page, CancellationToken cancellationToken)
    {
        var options = new AccessibilitySnapshotOptions
        {
            InterestingOnly = false
        };

        var snapshotTask = InvokeAccessibilitySnapshotAsync(page, options, cancellationToken);
        if (snapshotTask is not null)
        {
            await snapshotTask.ConfigureAwait(false);
            return GetTaskResult(snapshotTask);
        }

        var accessibility = page.GetType().GetProperty("Accessibility")?.GetValue(page);
        if (accessibility is not null)
        {
            var legacyTask = InvokeLegacySnapshotAsync(accessibility, options);
            if (legacyTask is not null)
            {
                await legacyTask.ConfigureAwait(false);
                return GetTaskResult(legacyTask);
            }
        }

        return null;
    }

    private static Task? InvokeAccessibilitySnapshotAsync(IPage page, AccessibilitySnapshotOptions options, CancellationToken cancellationToken)
    {
        var pageType = page.GetType();

        var method = pageType.GetMethod(
            "AccessibilitySnapshotAsync",
            new[] { typeof(AccessibilitySnapshotOptions), typeof(CancellationToken) })
            ?? pageType.GetMethod("AccessibilitySnapshotAsync", new[] { typeof(AccessibilitySnapshotOptions) });

        if (method is null)
        {
            return null;
        }

        var parameters = method.GetParameters().Length == 2
            ? new object?[] { options, cancellationToken }
            : new object?[] { options };

        return method.Invoke(page, parameters) as Task;
    }

    private static Task? InvokeLegacySnapshotAsync(object accessibility, AccessibilitySnapshotOptions options)
    {
        var method = accessibility.GetType().GetMethod("SnapshotAsync", new[] { typeof(AccessibilitySnapshotOptions) });
        if (method is null)
        {
            return null;
        }

        return method.Invoke(accessibility, new object?[] { options }) as Task;
    }

    private static object? GetTaskResult(Task task)
    {
        var type = task.GetType();
        var resultProperty = type.GetProperty("Result");
        return resultProperty?.GetValue(task);
    }
}
