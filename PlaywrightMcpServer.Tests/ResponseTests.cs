using System;
using System.Collections.Generic;
using System.Linq;
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

    [Fact]
    public void Serialize_IncludesResultAndCodeSections()
    {
        var context = new ResponseContext(new TabManager(), new SnapshotManager(), new ResponseConfiguration());
        var response = new Response(context, "tool", new Dictionary<string, object?>());

        response.AddResult("Operation completed");
        response.AddCode("await page.goto('https://example.com')");

        var serialized = response.Serialize();
        var text = Assert.IsType<TextContent>(Assert.Single(serialized.Content)).Text;

        Assert.Contains("### Result", text);
        Assert.Contains("Operation completed", text);
        Assert.Contains("### Ran Playwright code", text);
        Assert.Contains("await page.goto('https://example.com')", text);
    }

    [Fact]
    public void Serialize_RedactsSecretsFromTextContent()
    {
        var configuration = new ResponseConfiguration
        {
            Secrets = new Dictionary<string, string>
            {
                ["api-token"] = "SECRET_VALUE"
            }
        };

        var context = new ResponseContext(new TabManager(), new SnapshotManager(), configuration);
        var response = new Response(context, "tool", new Dictionary<string, object?>());

        response.AddResult("token: SECRET_VALUE");

        var serialized = response.Serialize();
        var text = Assert.IsType<TextContent>(Assert.Single(serialized.Content)).Text;

        Assert.DoesNotContain("SECRET_VALUE", text);
        Assert.Contains("<secret>api-token</secret>", text);
    }

    [Fact]
    public void Serialize_IncludesOpenTabsWhenRequested()
    {
        var tabManager = new TabManager();
        var snapshotManager = new SnapshotManager();
        var context = new ResponseContext(tabManager, snapshotManager, new ResponseConfiguration());

        var pageMock = new Mock<IPage>();
        pageMock.SetupGet(p => p.Url).Returns("https://example.com/");
        pageMock.Setup(p => p.TitleAsync()).ReturnsAsync("Example");
        SetupPageEvents(pageMock);

        tabManager.Register(pageMock.Object);

        var response = new Response(context, "tool", new Dictionary<string, object?>());
        response.SetIncludeTabs();

        var serialized = response.Serialize();
        var text = Assert.IsType<TextContent>(Assert.Single(serialized.Content)).Text;

        Assert.Contains("### Open tabs", text);
        Assert.Contains("https://example.com/", text);
    }

    [Fact]
    public void Serialize_RespectsImageResponseConfiguration()
    {
        var includeConfiguration = new ResponseConfiguration();
        var includeContext = new ResponseContext(new TabManager(), new SnapshotManager(), includeConfiguration);
        var includeResponse = new Response(includeContext, "tool", new Dictionary<string, object?>());
        includeResponse.AddResult("done");
        includeResponse.AddImage("image/png", new byte[] { 1, 2, 3 });

        var includeSerialized = includeResponse.Serialize();
        Assert.Equal(2, includeSerialized.Content.Count);
        var image = Assert.IsType<ImageContent>(includeSerialized.Content.Last());
        Assert.Equal("AQID", image.Data);
        Assert.Equal("image/png", image.MimeType);

        var omitConfiguration = new ResponseConfiguration { ImageResponses = ImageResponseMode.Omit };
        var omitContext = new ResponseContext(new TabManager(), new SnapshotManager(), omitConfiguration);
        var omitResponse = new Response(omitContext, "tool", new Dictionary<string, object?>());
        omitResponse.AddResult("done");
        omitResponse.AddImage("image/png", new byte[] { 1, 2, 3 });

        var omitSerialized = omitResponse.Serialize();
        var content = Assert.Single(omitSerialized.Content);
        Assert.IsType<TextContent>(content);
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
