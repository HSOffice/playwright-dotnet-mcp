using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class TabStateWaitForCompletionTests
{
    [Fact]
    public async Task WaitForCompletionAsync_WaitsForOutstandingRequests()
    {
        var tabManager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.test");

        var delayCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        pageMock.Setup(p => p.WaitForTimeoutAsync(It.IsAny<float>()))
            .Returns<float>(_ => delayCompletion.Task);

        var tab = tabManager.Register(pageMock.Object);

        var requestMock = new Mock<IRequest>();
        var responseMock = new Mock<IResponse>();
        responseMock.SetupGet(r => r.Request).Returns(requestMock.Object);

        var responseCompletion = new TaskCompletionSource<IResponse?>(TaskCreationOptions.RunContinuationsAsynchronously);
        requestMock.Setup(r => r.ResponseAsync()).Returns(responseCompletion.Task);

        var waitTask = tab.WaitForCompletionAsync(async ct =>
        {
            pageMock.Raise(m => m.Request += null!, requestMock.Object);
            await Task.CompletedTask;
        }, CancellationToken.None);

        await Task.Delay(50).ConfigureAwait(false);
        Assert.False(waitTask.IsCompleted);

        pageMock.Raise(m => m.Response += null!, responseMock.Object);
        responseCompletion.TrySetResult(responseMock.Object);

        await Task.Delay(50).ConfigureAwait(false);
        Assert.False(waitTask.IsCompleted);

        delayCompletion.TrySetResult(true);

        await waitTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        pageMock.Verify(p => p.WaitForTimeoutAsync(1000), Times.Once);
    }

    [Fact]
    public async Task WaitForCompletionAsync_UnblocksWhenModalAppears()
    {
        var tabManager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.test");
        pageMock.Setup(p => p.WaitForTimeoutAsync(It.IsAny<float>())).Returns(Task.CompletedTask);

        var tab = tabManager.Register(pageMock.Object);
        var blocker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var waitTask = tab.WaitForCompletionAsync(_ => blocker.Task, CancellationToken.None);

        await Task.Delay(50).ConfigureAwait(false);
        Assert.False(waitTask.IsCompleted);

        var dialogMock = new Mock<IDialog>();
        dialogMock.SetupGet(d => d.Type).Returns("alert");
        dialogMock.SetupGet(d => d.Message).Returns("Blocking dialog");

        pageMock.Raise(m => m.Dialog += null!, dialogMock.Object);

        await waitTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

        blocker.TrySetResult();
    }

    [Fact]
    public async Task WaitForCompletionAsync_WaitsForPostActionDelay()
    {
        var tabManager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.test");

        var delayCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        pageMock.Setup(p => p.WaitForTimeoutAsync(It.IsAny<float>()))
            .Returns<float>(_ => delayCompletion.Task);

        var tab = tabManager.Register(pageMock.Object);

        var waitTask = tab.WaitForCompletionAsync(_ => Task.CompletedTask, CancellationToken.None);

        await Task.Delay(50).ConfigureAwait(false);
        Assert.False(waitTask.IsCompleted);

        delayCompletion.TrySetResult(true);

        await waitTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        pageMock.Verify(p => p.WaitForTimeoutAsync(1000), Times.Once);
    }
}
