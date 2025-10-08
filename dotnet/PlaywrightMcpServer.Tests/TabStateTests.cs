using System;
using System.Collections.Generic;
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

    private static Mock<IPage> CreatePageMock(string snapshot, out Dictionary<string, Mock<ILocator>> locatorMap)
    {
        locatorMap = new Dictionary<string, Mock<ILocator>>(StringComparer.Ordinal)
        {
            ["aria-ref=field-1"] = CreateLocatorMock(),
            ["aria-ref=field-2"] = CreateLocatorMock(),
        };

        var pageMock = new Mock<IPage>(MockBehavior.Strict);
        pageMock.As<IPageSnapshotForAi>().Setup(p => p._SnapshotForAIAsync()).ReturnsAsync(snapshot);
        pageMock.Setup(p => p.Locator(It.IsAny<string>()))
            .Returns<string>(selector =>
            {
                if (!locatorMap.TryGetValue(selector, out var locator))
                {
                    locator = CreateLocatorMock();
                    locatorMap[selector] = locator;
                }

                return locator.Object;
            });

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

    private interface IPageSnapshotForAi
    {
        Task<string> _SnapshotForAIAsync();
    }

    private interface IDescribableLocator
    {
        ILocator Describe(string description);
    }
}
