using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class TabStateTests
{
    [Fact]
    public async Task GetLocatorsByRefAsync_ReturnsAnnotatedLocators()
    {
        var snapshot = "[ref=field-1] something [ref=field-2]";
        var pageMock = CreatePageMock(snapshot, out var locatorMap);

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        var requests = new[]
        {
            new TabState.RefLocatorRequest("First field", "field-1"),
            new TabState.RefLocatorRequest("Second field", "field-2"),
        };

        var locators = await tab.GetLocatorsByRefAsync(requests, CancellationToken.None).ConfigureAwait(false);

        Assert.Collection(locators,
            locator => Assert.Same(locatorMap["aria-ref=field-1"].Object, locator),
            locator => Assert.Same(locatorMap["aria-ref=field-2"].Object, locator));

        locatorMap["aria-ref=field-1"].As<IDescribableLocator>().Verify(l => l.Describe("First field"), Times.Once);
        locatorMap["aria-ref=field-2"].As<IDescribableLocator>().Verify(l => l.Describe("Second field"), Times.Once);
    }

    [Fact]
    public async Task GetLocatorsByRefAsync_ThrowsWhenRefMissing()
    {
        var snapshot = "[ref=field-1]";
        var pageMock = CreatePageMock(snapshot, out _);
        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        var requests = new[]
        {
            new TabState.RefLocatorRequest("Missing field", "missing-ref"),
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tab.GetLocatorsByRefAsync(requests, CancellationToken.None)).ConfigureAwait(false);

        Assert.Contains("Ref 'missing-ref'", exception.Message);
        Assert.Contains("Capture a new snapshot", exception.Message);
    }

    [Fact]
    public async Task CaptureSnapshotAsync_UpdatesMetadata()
    {
        var pageMock = new Mock<IPage>();
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Snapshot Title");
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });
        var snapshotManager = new SnapshotManager();

        var snapshot = await tab.CaptureSnapshotAsync(snapshotManager, CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("https://example.com/", tab.Url);
        Assert.Equal("Snapshot Title", tab.Title);
        Assert.Same(snapshot, tab.LastSnapshot);
        Assert.Equal("Snapshot Title", snapshot.Title);
        Assert.Equal("https://example.com/", snapshot.Url);
    }

    [Fact]
    public void GetConsoleMessages_ReturnsAllMessages()
    {
        var (tab, pageMock, consoleHandler) = CreateTabWithConsoleCapture();
        var handler = Assert.NotNull(consoleHandler);

        var infoMessage = CreateConsoleMessage("log", "info message");
        var errorMessage = CreateConsoleMessage("error", "error message");

        handler(pageMock.Object, infoMessage.Object);
        handler(pageMock.Object, errorMessage.Object);

        var messages = tab.GetConsoleMessages(onlyErrors: false);

        Assert.Equal(2, messages.Count);
        Assert.Collection(messages,
            message =>
            {
                Assert.Equal("log", message.Type);
                Assert.Equal("info message", message.Text);
            },
            message =>
            {
                Assert.Equal("error", message.Type);
                Assert.Equal("error message", message.Text);
            });

        tab.Dispose();
    }

    [Fact]
    public void GetConsoleMessages_ReturnsOnlyErrors()
    {
        var (tab, pageMock, consoleHandler) = CreateTabWithConsoleCapture();
        var handler = Assert.NotNull(consoleHandler);

        var infoMessage = CreateConsoleMessage("log", "info message");
        var errorMessage = CreateConsoleMessage("error", "error message");

        handler(pageMock.Object, infoMessage.Object);
        handler(pageMock.Object, errorMessage.Object);

        var messages = tab.GetConsoleMessages(onlyErrors: true);

        var message = Assert.Single(messages);
        Assert.Equal("error", message.Type);
        Assert.Equal("error message", message.Text);

        tab.Dispose();
    }

    [Fact]
    public void GetNetworkRequests_ReturnsClones()
    {
        var (tab, pageMock, requestHandler, responseHandler, _, _) = CreateTabWithNetworkCapture();
        var onRequest = Assert.NotNull(requestHandler);
        var onResponse = Assert.NotNull(responseHandler);

        var request = CreateRequestMock("GET", "https://example.com/api", "xhr");
        var response = CreateResponseMock(request, status: 200);

        onRequest(pageMock.Object, request.Object);
        onResponse(pageMock.Object, response.Object);

        var first = tab.GetNetworkRequests();
        var entry = Assert.Single(first);
        Assert.Equal(200, entry.Status);
        Assert.Null(entry.Failure);

        entry.Status = 404;
        entry.Failure = "Not Found";

        var second = tab.GetNetworkRequests();
        var secondEntry = Assert.Single(second);
        Assert.Equal(200, secondEntry.Status);
        Assert.Null(secondEntry.Failure);

        tab.Dispose();
    }

    [Fact]
    public async Task NavigateAsync_ClearsNetworkRequestsBeforeNavigation()
    {
        var (tab, pageMock, requestHandler, responseHandler, _, setUrl) = CreateTabWithNetworkCapture();
        var onRequest = Assert.NotNull(requestHandler);
        var onResponse = Assert.NotNull(responseHandler);

        var initialRequest = CreateRequestMock("POST", "https://example.com/api", "xhr");
        var initialResponse = CreateResponseMock(initialRequest, status: 201);
        onRequest(pageMock.Object, initialRequest.Object);
        onResponse(pageMock.Object, initialResponse.Object);

        Assert.Single(tab.GetNetworkRequests());

        pageMock.Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions?>()))
            .Returns<string, PageGotoOptions?>((url, _) =>
            {
                setUrl(url);
                Assert.Empty(tab.GetNetworkRequests());

                var navigationRequest = CreateRequestMock("GET", url, "document");
                var navigationResponse = CreateResponseMock(navigationRequest, status: 200);

                onRequest(pageMock.Object, navigationRequest.Object);
                onResponse(pageMock.Object, navigationResponse.Object);

                return Task.FromResult<IResponse?>(navigationResponse.Object);
            });

        var response = await tab.NavigateAsync("https://contoso.test/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        }, CancellationToken.None).ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);

        var requestsAfter = tab.GetNetworkRequests();
        var entry = Assert.Single(requestsAfter);
        Assert.Equal("https://contoso.test/", entry.Url);
        Assert.Equal("GET", entry.Method);
        Assert.Equal(200, entry.Status);

        tab.Dispose();
    }

    [Fact]
    public async Task WaitForLoadStateAsync_InvokesPlaywrightLoadState()
    {
        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.Setup(p => p.WaitForLoadStateAsync(It.IsAny<LoadState>(), It.IsAny<PageWaitForLoadStateOptions?>()))
            .Returns(Task.CompletedTask);

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        var options = new PageWaitForLoadStateOptions
        {
            Timeout = 1234
        };

        await tab.WaitForLoadStateAsync(options: options, cancellationToken: CancellationToken.None).ConfigureAwait(false);

        pageMock.Verify(p => p.WaitForLoadStateAsync(LoadState.Load, options), Times.Once);
    }

    [Fact]
    public async Task WaitForLoadStateAsync_IgnoresTimeoutExceptions()
    {
        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.Setup(p => p.WaitForLoadStateAsync(It.IsAny<LoadState>(), It.IsAny<PageWaitForLoadStateOptions?>()))
            .Returns(Task.FromException(new TimeoutException()));

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        await tab.WaitForLoadStateAsync(cancellationToken: CancellationToken.None).ConfigureAwait(false);

        pageMock.Verify(p => p.WaitForLoadStateAsync(LoadState.Load, It.IsAny<PageWaitForLoadStateOptions?>()), Times.Once);
    }

    [Fact]
    public async Task WaitForLoadStateAsync_IgnoresPlaywrightTimeoutExceptions()
    {
        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.Setup(p => p.WaitForLoadStateAsync(It.IsAny<LoadState>(), It.IsAny<PageWaitForLoadStateOptions?>()))
            .Returns(Task.FromException(new PlaywrightException("Timeout 5000ms exceeded")));

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        await tab.WaitForLoadStateAsync(cancellationToken: CancellationToken.None).ConfigureAwait(false);

        pageMock.Verify(p => p.WaitForLoadStateAsync(LoadState.Load, It.IsAny<PageWaitForLoadStateOptions?>()), Times.Once);
    }

    [Fact]
    public async Task WaitForLoadStateAsync_ThrowsWhenStateUnsupported()
    {
        var pageMock = new Mock<IPage>();
        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            tab.WaitForLoadStateAsync(LoadState.DOMContentLoaded, cancellationToken: CancellationToken.None)).ConfigureAwait(false);

        Assert.Contains("Only LoadState.Load is supported.", exception.Message);
    }

    [Fact]
    public async Task WaitForTimeoutAsync_UsesEvaluateWhenNotBlocked()
    {
        var pageMock = new Mock<IPage>();
        pageMock.Setup(p => p.EvaluateAsync<object>(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((object?)null);

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });

        await tab.WaitForTimeoutAsync(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);

        pageMock.Verify(p => p.EvaluateAsync<object>(
            "(ms) => new Promise(resolve => setTimeout(resolve, ms))",
            It.Is<object?>(arg => arg is double value && Math.Abs(value - 250d) < 0.001)), Times.Once);
    }

    [Fact]
    public async Task WaitForTimeoutAsync_UsesDelayWhenJavaScriptBlocked()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");
        pageMock.Setup(p => p.EvaluateAsync<object>(It.IsAny<string>(), It.IsAny<object?>()))
            .Throws(new InvalidOperationException("Should not evaluate"));

        var tab = manager.Register(pageMock.Object);

        var dialogMock = new Mock<IDialog>();
        dialogMock.SetupGet(d => d.Type).Returns("alert");
        dialogMock.SetupGet(d => d.Message).Returns("Blocking dialog");
        pageMock.Raise(p => p.Dialog += null!, dialogMock.Object);

        var watch = Stopwatch.StartNew();
        await tab.WaitForTimeoutAsync(TimeSpan.FromMilliseconds(20)).ConfigureAwait(false);
        watch.Stop();

        Assert.True(watch.Elapsed >= TimeSpan.FromMilliseconds(20));
        pageMock.Verify(p => p.EvaluateAsync<object>(It.IsAny<string>(), It.IsAny<object?>()), Times.Never);

        tab.Dispose();
    }

    [Fact]
    public void GetModalStatesMarkdown_ReturnsFormattedLines()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");

        var tab = manager.Register(pageMock.Object);

        var dialogMock = new Mock<IDialog>();
        dialogMock.SetupGet(d => d.Type).Returns("prompt");
        dialogMock.SetupGet(d => d.Message).Returns("Provide value");
        pageMock.Raise(p => p.Dialog += null!, dialogMock.Object);

        var markdown = tab.GetModalStatesMarkdown();

        Assert.Collection(markdown,
            line => Assert.Equal("### Modal state", line),
            line => Assert.Equal("- [\"prompt\" dialog with message \"Provide value\"]: can be handled by the \"browser_handle_dialog\" tool", line));

        tab.Dispose();
    }

    private static Mock<IPage> CreatePageMock(string snapshot, out Dictionary<string, Mock<ILocator>> locatorMap)
    {
        var locatorDictionary = new Dictionary<string, Mock<ILocator>>(StringComparer.Ordinal)
        {
            ["aria-ref=field-1"] = CreateLocatorMock(),
            ["aria-ref=field-2"] = CreateLocatorMock(),
        };

        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.As<IPageSnapshotForAi>().Setup(p => p._SnapshotForAIAsync()).ReturnsAsync(snapshot);
        pageMock.Setup(p => p.Locator(It.IsAny<string>()))
            .Returns<string>(selector =>
            {
                if (!locatorDictionary.TryGetValue(selector, out var locator))
                {
                    locator = CreateLocatorMock();
                    locatorDictionary[selector] = locator;
                }

                return locator.Object;
            });

        locatorMap = locatorDictionary;
        return pageMock;
    }

    private static Mock<ILocator> CreateLocatorMock()
    {
        var locatorMock = new Mock<ILocator>(MockBehavior.Strict);
        locatorMock.As<IDescribableLocator>().Setup(l => l.Describe(It.IsAny<string>())).Returns(locatorMock.Object);
        return locatorMock;
    }

    private static (TabState Tab, Mock<IPage> PageMock, EventHandler<IConsoleMessage>? ConsoleHandler) CreateTabWithConsoleCapture()
    {
        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Example");

        EventHandler<IConsoleMessage>? consoleHandler = null;
        pageMock.SetupAdd(p => p.Console += It.IsAny<EventHandler<IConsoleMessage>>())
            .Callback<EventHandler<IConsoleMessage>>(handler => consoleHandler += handler);
        pageMock.SetupRemove(p => p.Console -= It.IsAny<EventHandler<IConsoleMessage>>())
            .Callback<EventHandler<IConsoleMessage>>(handler => consoleHandler -= handler);

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

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });
        tab.AttachHandlers();

        return (tab, pageMock, consoleHandler);
    }

    private static Mock<IConsoleMessage> CreateConsoleMessage(string type, string text)
    {
        var message = new Mock<IConsoleMessage>(MockBehavior.Strict);
        message.SetupGet(m => m.Type).Returns(type);
        message.SetupGet(m => m.Text).Returns(text);
        message.Setup(m => m.Args).Returns(Array.Empty<IJSHandle>());
        return message;
    }

    private static (
        TabState Tab,
        Mock<IPage> PageMock,
        EventHandler<IRequest>? RequestHandler,
        EventHandler<IResponse>? ResponseHandler,
        EventHandler<IRequest>? RequestFailedHandler,
        Action<string> UpdateUrl) CreateTabWithNetworkCapture()
    {
        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        var currentUrl = "https://example.com/";

        pageMock.SetupGet(p => p.Url).Returns(() => currentUrl);
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Example");

        EventHandler<IConsoleMessage>? consoleHandler = null;
        pageMock.SetupAdd(p => p.Console += It.IsAny<EventHandler<IConsoleMessage>>())
            .Callback<EventHandler<IConsoleMessage>>(handler => consoleHandler += handler);
        pageMock.SetupRemove(p => p.Console -= It.IsAny<EventHandler<IConsoleMessage>>())
            .Callback<EventHandler<IConsoleMessage>>(handler => consoleHandler -= handler);

        EventHandler<IRequest>? requestHandler = null;
        pageMock.SetupAdd(p => p.Request += It.IsAny<EventHandler<IRequest>>())
            .Callback<EventHandler<IRequest>>(handler => requestHandler += handler);
        pageMock.SetupRemove(p => p.Request -= It.IsAny<EventHandler<IRequest>>())
            .Callback<EventHandler<IRequest>>(handler => requestHandler -= handler);

        EventHandler<IResponse>? responseHandler = null;
        pageMock.SetupAdd(p => p.Response += It.IsAny<EventHandler<IResponse>>())
            .Callback<EventHandler<IResponse>>(handler => responseHandler += handler);
        pageMock.SetupRemove(p => p.Response -= It.IsAny<EventHandler<IResponse>>())
            .Callback<EventHandler<IResponse>>(handler => responseHandler -= handler);

        EventHandler<IRequest>? requestFailedHandler = null;
        pageMock.SetupAdd(p => p.RequestFailed += It.IsAny<EventHandler<IRequest>>())
            .Callback<EventHandler<IRequest>>(handler => requestFailedHandler += handler);
        pageMock.SetupRemove(p => p.RequestFailed -= It.IsAny<EventHandler<IRequest>>())
            .Callback<EventHandler<IRequest>>(handler => requestFailedHandler -= handler);

        pageMock.SetupAdd(p => p.Close += It.IsAny<EventHandler<IPage>>());
        pageMock.SetupRemove(p => p.Close -= It.IsAny<EventHandler<IPage>>());
        pageMock.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>());
        pageMock.SetupRemove(p => p.Dialog -= It.IsAny<EventHandler<IDialog>>());
        pageMock.SetupAdd(p => p.FileChooser += It.IsAny<EventHandler<IFileChooser>>());
        pageMock.SetupRemove(p => p.FileChooser -= It.IsAny<EventHandler<IFileChooser>>());
        pageMock.SetupAdd(p => p.Download += It.IsAny<EventHandler<IDownload>>());
        pageMock.SetupRemove(p => p.Download -= It.IsAny<EventHandler<IDownload>>());
        pageMock.Setup(p => p.WaitForLoadStateAsync(It.IsAny<LoadState>(), It.IsAny<PageWaitForLoadStateOptions?>()))
            .Returns(Task.CompletedTask);

        var tab = new TabState(pageMock.Object, "tab-1", DateTimeOffset.UtcNow, _ => { });
        tab.AttachHandlers();

        return (tab, pageMock, requestHandler, responseHandler, requestFailedHandler, newUrl => currentUrl = newUrl);
    }

    private static Mock<IRequest> CreateRequestMock(string method, string url, string? resourceType)
    {
        var request = new Mock<IRequest>(MockBehavior.Strict);
        request.SetupGet(r => r.Method).Returns(method);
        request.SetupGet(r => r.Url).Returns(url);
        request.SetupGet(r => r.ResourceType).Returns(resourceType);
        request.SetupGet(r => r.Failure).Returns((string?)null);
        return request;
    }

    private static Mock<IResponse> CreateResponseMock(Mock<IRequest> request, int status)
    {
        var response = new Mock<IResponse>(MockBehavior.Strict);
        response.SetupGet(r => r.Request).Returns(request.Object);
        response.SetupGet(r => r.Status).Returns(status);
        return response;
    }

    private interface IPageSnapshotForAi
    {
        Task<string> _SnapshotForAIAsync();
    }

    private interface IDescribableLocator
    {
        ILocator Describe(string description);
    }
}
