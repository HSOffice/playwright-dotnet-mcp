using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    /// <summary>
    /// 重新启动浏览器会话，可选择浏览器内核，并恢复先前标签页的导航状态。
    /// </summary>
    [McpServerTool(Name = "browser_relaunch")]
    [Description("(Re)launch browser and open a fresh page.")]
    public static async Task<string> RelaunchAsync(
        [Description("Browser engine to launch (chromium, firefox, webkit).")] string? engine = null,
        CancellationToken cancellationToken = default)
    {
        var restorePlan = TabManager.CreateRestorePlan();

        if (!string.IsNullOrWhiteSpace(engine))
        {
            _browserEngine = NormalizeEngine(engine);
        }

        await CloseAsync(cancellationToken).ConfigureAwait(false);
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);

        var context = await GetContextAsync(cancellationToken).ConfigureAwait(false);
        SnapshotPayload? initialSnapshot = null;

        if (restorePlan.Count > 0)
        {
            var activeTab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
            var first = restorePlan[0];
            if (!string.IsNullOrWhiteSpace(first.Url))
            {
                await activeTab.Page.GotoAsync(first.Url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                }).ConfigureAwait(false);
                initialSnapshot = await SnapshotManager.CaptureAsync(activeTab, cancellationToken).ConfigureAwait(false);
            }

            for (var index = 1; index < restorePlan.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = restorePlan[index];
                var page = await context.NewPageAsync().ConfigureAwait(false);
                var tab = TabManager.Register(page, makeActive: false);
                if (!string.IsNullOrWhiteSpace(entry.Url))
                {
                    await page.GotoAsync(entry.Url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle
                    }).ConfigureAwait(false);
                    await SnapshotManager.CaptureAsync(tab, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        else
        {
            var active = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
            initialSnapshot = await SnapshotManager.CaptureAsync(active, cancellationToken).ConfigureAwait(false);
        }

        var response = new
        {
            relaunched = true,
            headless = Headless,
            engine = _browserEngine,
            tabs = TabManager.DescribeTabs(),
            snapshot = initialSnapshot
        };

        return Serialize(response);
    }

    /// <summary>
    /// 关闭并释放当前 Playwright 实例及其上下文、页面等资源。
    /// </summary>
    [McpServerTool(Name = "browser_close")]
    [Description("Close and dispose Playwright browser resources.")]
    public static async Task<string> CloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_context is not null)
        {
            _context.Page -= ContextOnPage;
            try { await _context.CloseAsync().ConfigureAwait(false); }
            catch { }
            _context = null;
        }

        if (_browser is not null)
        {
            try { await _browser.CloseAsync().ConfigureAwait(false); }
            catch { }
            _browser = null;
        }

        if (_playwright is not null)
        {
            _playwright.Dispose();
            _playwright = null;
        }

        TabManager.Reset();
        _tracingActive = false;

        return Serialize(new { closed = true });
    }

    private static string NormalizeEngine(string? engine)
    {
        if (string.IsNullOrWhiteSpace(engine))
        {
            return "chromium";
        }

        return engine.Trim().ToLowerInvariant() switch
        {
            "chromium" or "chrome" or "msedge" or "edge" => "chromium",
            "firefox" or "ff" => "firefox",
            "webkit" or "safari" => "webkit",
            _ => "chromium"
        };
    }
}
