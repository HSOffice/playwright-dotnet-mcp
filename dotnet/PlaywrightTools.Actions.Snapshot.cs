using ModelContextProtocol.Server;
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

        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = await tab.CaptureSnapshotAsync(SnapshotManager, cancellationToken).ConfigureAwait(false);

        var payload = new
        {
            includeSnapshot = true,
            url = snapshot.Url,
            title = snapshot.Title,
            ariaSnapshot = snapshot.Aria,
            modalStates = snapshot.ModalStates,
            consoleMessages = snapshot.Console,
            networkRequests = snapshot.Network,
            downloads = snapshot.Downloads,
            timestamp = snapshot.Timestamp
        };

        return Serialize(payload);
    }
}
