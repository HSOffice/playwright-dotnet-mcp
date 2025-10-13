using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_mouse_move_xy")]
    [Description("Move mouse to a given position.")]
    public static async Task<string> BrowserMouseMoveXyAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("X coordinate.")] double x,
        [Description("Y coordinate.")] double y,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["x"] = x,
            ["y"] = y
        };

        return await ExecuteWithResponseAsync(
            "browser_mouse_move_xy",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.AddCode($"// Move mouse to ({x}, {y})");
                response.AddCode($"await page.mouse.move({x}, {y});");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await tab.Page.Mouse.MoveAsync((float)x, (float)y).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_mouse_click_xy")]
    [Description("Click left mouse button at a given position.")]
    public static async Task<string> BrowserMouseClickXyAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("X coordinate.")] double x,
        [Description("Y coordinate.")] double y,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["x"] = x,
            ["y"] = y
        };

        return await ExecuteWithResponseAsync(
            "browser_mouse_click_xy",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();
                response.AddCode($"// Click mouse at coordinates ({x}, {y})");
                response.AddCode($"await page.mouse.move({x}, {y});");
                response.AddCode("await page.mouse.down();");
                response.AddCode("await page.mouse.up();");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    var mouse = tab.Page.Mouse;
                    await mouse.MoveAsync((float)x, (float)y).ConfigureAwait(false);
                    await mouse.DownAsync().ConfigureAwait(false);
                    await mouse.UpAsync().ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "browser_mouse_drag_xy")]
    [Description("Drag left mouse button to a given position.")]
    public static async Task<string> BrowserMouseDragXyAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Start X coordinate.")] double startX,
        [Description("Start Y coordinate.")] double startY,
        [Description("End X coordinate.")] double endX,
        [Description("End Y coordinate.")] double endY,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["element"] = element,
            ["startX"] = startX,
            ["startY"] = startY,
            ["endX"] = endX,
            ["endY"] = endY
        };

        return await ExecuteWithResponseAsync(
            "browser_mouse_drag_xy",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();
                response.AddCode($"// Drag mouse from ({startX}, {startY}) to ({endX}, {endY})");
                response.AddCode($"await page.mouse.move({startX}, {startY});");
                response.AddCode("await page.mouse.down();");
                response.AddCode($"await page.mouse.move({endX}, {endY});");
                response.AddCode("await page.mouse.up();");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    var mouse = tab.Page.Mouse;
                    await mouse.MoveAsync((float)startX, (float)startY).ConfigureAwait(false);
                    await mouse.DownAsync().ConfigureAwait(false);
                    await mouse.MoveAsync((float)endX, (float)endY).ConfigureAwait(false);
                    await mouse.UpAsync().ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
