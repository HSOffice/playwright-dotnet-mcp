using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightMcp.Core.BrowserServerBackend;
using PlaywrightMcp.Core.Protocol;
using PlaywrightMcp.Core.Runtime;

namespace PlaywrightMcp.Tools;

/// <summary>
/// Provides browser lifecycle management tools such as relaunch and close.
/// </summary>
public static class RelaunchTools
{
    public static IReadOnlyCollection<IToolDefinition> CreateTools() => new List<IToolDefinition>
    {
        ToolHelpers.CreateDelegateTool(
            name: "browser_relaunch",
            description: "(Re)launch browser and open a fresh page.",
            handler: RelaunchAsync),
        ToolHelpers.CreateDelegateTool(
            name: "browser_close",
            description: "Close and dispose Playwright browser resources.",
            handler: CloseAsync)
    };

    private static Task<Response> RelaunchAsync(
        ToolInvocationContext context,
        JsonElement parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tab = context.BrowserContext.RelaunchBrowser();

        var response = new Response();
        response.AddBlock(new MarkdownBlock("Browser relaunched and ready for automation."));
        response.Metadata["relaunched"] = true;
        response.Metadata["activeTabId"] = tab.Id;
        response.Metadata["tabs"] = context.BrowserContext.Tabs.Select(t => t.Id).ToArray();
        response.Metadata["isBrowserLaunched"] = context.BrowserContext.IsBrowserLaunched;

        return Task.FromResult(response);
    }

    private static Task<Response> CloseAsync(
        ToolInvocationContext context,
        JsonElement parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        context.BrowserContext.CloseBrowser();

        var response = new Response();
        response.AddBlock(new MarkdownBlock("Browser session closed."));
        response.Metadata["closed"] = true;
        response.Metadata["isBrowserLaunched"] = context.BrowserContext.IsBrowserLaunched;
        response.Metadata["tabs"] = context.BrowserContext.Tabs.Select(t => t.Id).ToArray();

        return Task.FromResult(response);
    }
}
