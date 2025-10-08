using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class NavigateIntegrationTests
{
    [Fact]
    public async Task NavigateAsyncRecordsDownloadInResponse()
    {
        var url = "https://example.test/download";
        var tabManager = new TabManager();
        var pageMock = new Mock<IPage>();
        var downloadMock = new Mock<IDownload>();
        var saveCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        pageMock.SetupGet(p => p.Url).Returns(url);
        pageMock.Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions?>()))
            .Returns<string, PageGotoOptions?>((requestedUrl, _) =>
            {
                Assert.Equal(url, requestedUrl);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(50).ConfigureAwait(false);
                    pageMock.Raise(m => m.Download += null!, downloadMock.Object);
                });

                return Task.FromException<IResponse?>(new PlaywrightException("net::ERR_ABORTED"));
            });
        pageMock.Setup(p => p.WaitForLoadStateAsync(It.IsAny<LoadState>(), It.IsAny<PageWaitForLoadStateOptions?>()))
            .Returns(Task.CompletedTask);
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Download ready");

        downloadMock.SetupGet(d => d.SuggestedFilename).Returns("report.pdf");
        downloadMock.Setup(d => d.SaveAsAsync(It.IsAny<string>(), It.IsAny<DownloadSaveAsOptions?>()))
            .Returns<string, DownloadSaveAsOptions?>((_, _) =>
            {
                saveCompletion.TrySetResult(true);
                return Task.CompletedTask;
            });

        var tab = tabManager.Register(pageMock.Object);

        var navigationResponse = await tab.NavigateAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        }, CancellationToken.None).ConfigureAwait(false);

        Assert.Null(navigationResponse);
        await saveCompletion.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

        var downloads = tab.GetDownloadsSnapshot();
        Assert.Single(downloads);
        Assert.Equal("report.pdf", downloads[0].SuggestedFileName);
        Assert.True(downloads[0].Finished);

        var snapshotManager = new SnapshotManager();
        var snapshot = await tab.CaptureSnapshotAsync(snapshotManager, CancellationToken.None).ConfigureAwait(false);
        Assert.Single(snapshot.Downloads);
        Assert.Equal("report.pdf", snapshot.Downloads[0].SuggestedFileName);

        var responseContext = new ResponseContext(tabManager, snapshotManager, new ResponseConfiguration());
        var response = new Response(responseContext, "browser_navigate", new Dictionary<string, object?>());
        response.AddResult($"Navigated to {url}");
        response.SetIncludeSnapshot();
        response.SetIncludeTabs();

        await response.FinishAsync(CancellationToken.None).ConfigureAwait(false);
        var serialized = response.Serialize();
        var textContent = Assert.IsType<TextContent>(Assert.Single(serialized.Content));
        Assert.Contains("Downloaded file report.pdf", textContent.Text, StringComparison.Ordinal);
    }
}
