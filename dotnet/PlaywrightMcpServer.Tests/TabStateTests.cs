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

    private interface IPageSnapshotForAi
    {
        Task<string> _SnapshotForAIAsync();
    }

    private interface IDescribableLocator
    {
        ILocator Describe(string description);
    }
}
