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
    [McpServerTool(Name = "browser_handle_dialog")]
    [Description("Handle a dialog.")]
    public static async Task<string> BrowserHandleDialogAsync(
        [Description("Whether to accept the dialog.")] bool accept,
        [Description("The text of the prompt in case of a prompt dialog.")] string? promptText = null,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["accept"] = accept,
            ["promptText"] = promptText
        };

        return await ExecuteWithResponseAsync(
            "browser_handle_dialog",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();

                var modalState = tab.TryGetModalState("dialog");
                if (modalState?.Dialog is not IDialog dialog)
                {
                    throw new InvalidOperationException("No dialog visible");
                }

                tab.TryClearModalState(modalState);

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    if (accept)
                    {
                        await dialog.AcceptAsync(promptText).ConfigureAwait(false);
                    }
                    else
                    {
                        await dialog.DismissAsync().ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
