using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class ResponseTests
{
    [Fact]
    public async Task FinishAsync_UpdatesTabTitlesWhenTabsRequested()
    {
        var tabManager = new TabManager();
        var snapshotManager = new SnapshotManager();
        var responseContext = new ResponseContext(tabManager, snapshotManager, new ResponseConfiguration());

        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("about:blank");
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Updated Title");
        SetupPageEvents(pageMock);

        var tab = tabManager.Register(pageMock.Object);
        var response = new Response(responseContext, "tool", new Dictionary<string, object?>());
        response.SetIncludeTabs();

        await response.FinishAsync(CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("Updated Title", tab.Title);
        pageMock.Verify(p => p.TitleAsync(), Times.Once);
    }

    private static void SetupPageEvents(Mock<IPage> pageMock)
    {
        pageMock.SetupAdd(p => p.Console += It.IsAny<EventHandler<IConsoleMessage>>());
        pageMock.SetupRemove(p => p.Console -= It.IsAny<EventHandler<IConsoleMessage>>());
        pageMock.SetupAdd(p => p.Request += It.IsAny<EventHandler<IRequest>>());
        pageMock.SetupRemove(p => p.Request -= It.IsAny<EventHandler<IRequest>>());
        pageMock.SetupAdd(p => p.Response += It.IsAny<EventHandler<IResponse>>());
        pageMock.SetupRemove(p => p.Response -= It.IsAny<EventHandler<IResponse>>());
        pageMock.SetupAdd(p => p.RequestFailed += It.IsAny<EventHandler<IRequest>>());
        pageMock.SetupRemove(p => p.RequestFailed -= It.IsAny<EventHandler<IRequest>>());
        pageMock.SetupAdd(p => p.Close += It.IsAny<EventHandler<IPage>>());
        pageMock.SetupRemove(p => p.Close -= It.IsAny<EventHandler<IPage>>());
        pageMock.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>());
        pageMock.SetupRemove(p => p.Dialog -= It.IsAny<EventHandler<IDialog>>());
        pageMock.SetupAdd(p => p.FileChooser += It.IsAny<EventHandler<IFileChooser>>());
        pageMock.SetupRemove(p => p.FileChooser -= It.IsAny<EventHandler<IFileChooser>>());
        pageMock.SetupAdd(p => p.Download += It.IsAny<EventHandler<IDownload>>());
        pageMock.SetupRemove(p => p.Download -= It.IsAny<EventHandler<IDownload>>());
    }
}
