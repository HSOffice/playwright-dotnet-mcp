using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcpServer;

public sealed class ResponseContext
{
    private readonly TabManager _tabManager;
    private readonly SnapshotManager _snapshotManager;

    public ResponseContext(TabManager tabManager, SnapshotManager snapshotManager, ResponseConfiguration configuration)
    {
        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _snapshotManager = snapshotManager ?? throw new ArgumentNullException(nameof(snapshotManager));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public ResponseConfiguration Configuration { get; }

    public IReadOnlyList<TabState> Tabs => _tabManager.Tabs;

    public TabState? CurrentTab => _tabManager.ActiveTab;

    public IReadOnlyList<TabDescriptor> DescribeTabs() => _tabManager.DescribeTabs();

    public Task<SnapshotPayload> CaptureSnapshotAsync(TabState tab, CancellationToken cancellationToken)
        => _snapshotManager.CaptureAsync(tab, cancellationToken);
}
