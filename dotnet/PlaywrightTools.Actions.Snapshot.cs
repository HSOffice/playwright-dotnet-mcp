using Microsoft.Playwright;
using ModelContextProtocol.Server;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcpServer;

/*
 * NOTE:
 * - TS 版本通过 response.setIncludeSnapshot() 延迟由响应管线注入快照。
 * - .NET 端没有该集中注入点，因此这里直接生成并序列化快照，保证 LLM 立即拿到页面可访问性结构。
 * - 采用 v1.49+ 的 ARIA Snapshot（YAML 字符串）以取代已弃用的 Accessibility.SnapshotAsync/InterestingOnly。
 */
public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_snapshot")]
    [Description("Capture ARIA snapshot (accessibility tree) of the current page; better than plain screenshots for understanding structure.")]
    public static async Task<string> BrowserSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 复用已有的页面获取逻辑（项目中已实现）
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);

        string? ariaYaml = null;
        try
        {
            // ✅ 推荐：使用 ARIA Snapshot（Playwright .NET v1.49+）
            // 抓取整个页面（body）的可访问性树，以 YAML 表示，适合 LLM 消化与匹配。
            ariaYaml = await page.Locator("body").AriaSnapshotAsync().ConfigureAwait(false);

        }
        catch (PlaywrightException)
        {
            // 与 TS 管线一致：不要因为可访问性快照失败而让工具报错，继续返回基本信息。
            ariaYaml ??= null;
        }

        var payload = new
        {
            includeSnapshot = true,
            url = page.Url,
            title = await page.TitleAsync().ConfigureAwait(false),
            // ARIA 快照（YAML）。配合 ToMatchAriaSnapshot 等断言非常方便。
            ariaSnapshot = ariaYaml
        };

        return Serialize(payload);
    }
}
