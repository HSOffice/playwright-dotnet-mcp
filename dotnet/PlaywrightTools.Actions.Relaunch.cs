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
    /// <summary>
    /// 重新启动浏览器会话：先彻底关闭现有实例，再按配置重新拉起浏览器并打开新的空白页面。
    /// </summary>
    /// <param name="cancellationToken">用于在启动过程中取消操作的取消令牌。</param>
    /// <returns>包含重新启动状态与核心运行信息的 JSON 字符串。</returns>
    [McpServerTool(Name = "browser_relaunch")]
    [Description("(Re)launch browser and open a fresh page.")]
    public static async Task<string> RelaunchAsync(CancellationToken cancellationToken = default)
    {
        await CloseAsync(cancellationToken).ConfigureAwait(false);
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);
        return Serialize(new { relaunched = true, headless = Headless, engine = _isChromium ? "chromium" : "unknown" });
    }

    /// <summary>
    /// 关闭并释放当前 Playwright 会话涉及的页面、上下文和浏览器等所有资源，确保不会留下后台进程。
    /// </summary>
    /// <param name="cancellationToken">用于在需要时取消关闭流程的取消令牌。</param>
    /// <returns>表示关闭结果的 JSON 字符串。</returns>
    [McpServerTool(Name = "browser_close")]
    [Description("Close and dispose Playwright browser resources.")]
    public static async Task<string> CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_page is not null)
        {
            DetachPageEventHandlers(_page);
            try { await _page.CloseAsync().ConfigureAwait(false); }
            catch { }
            _page = null;
        }

        if (_context is not null)
        {
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

        _isChromium = false;
        _tracingActive = false;
        return Serialize(new { closed = true });
    }
}
