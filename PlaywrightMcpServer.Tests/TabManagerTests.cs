using System;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class TabManagerTests
{
    [Fact]
    public void ForPage_ReturnsRegisteredTab()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();

        var tab = manager.Register(pageMock.Object);

        var result = manager.ForPage(pageMock.Object);

        Assert.Same(tab, result);
    }

    [Fact]
    public void ForPage_ReturnsNullForUnknownPage()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();

        var result = manager.ForPage(pageMock.Object);

        Assert.Null(result);
    }

    [Fact]
    public void CollectConsoleMessages_ReturnsMessagesForPage()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");

        var tab = manager.Register(pageMock.Object);

        var message = CreateConsoleMessage("error", "Something went wrong");
        pageMock.Raise(p => p.Console += null!, message.Object);

        var messages = manager.CollectConsoleMessages(pageMock.Object);

        var entry = Assert.Single(messages);
        Assert.Equal("error", entry.Type);
        Assert.Equal("Something went wrong", entry.Text);

        tab.Dispose();
    }

    [Fact]
    public void CollectConsoleMessages_ReturnsEmptyForUnknownPage()
    {
        var manager = new TabManager();
        var pageMock = new Mock<IPage>();

        var messages = manager.CollectConsoleMessages(pageMock.Object);

        Assert.Empty(messages);
    }

    private static Mock<IConsoleMessage> CreateConsoleMessage(string type, string text)
    {
        var message = new Mock<IConsoleMessage>();
        message.SetupGet(m => m.Type).Returns(type);
        message.SetupGet(m => m.Text).Returns(text);
        message.Setup(m => m.Args).Returns(Array.Empty<IJSHandle>());
        return message;
    }
}
