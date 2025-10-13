using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_file_upload")]
    [Description("Upload one or multiple files.")]
    public static async Task<string> BrowserFileUploadAsync(
        [Description("The absolute paths to the files to upload. Can be single file or multiple files. If omitted, file chooser is cancelled.")] string[]? paths = null,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["paths"] = paths?.ToArray()
        };

        return await ExecuteWithResponseAsync(
            "browser_file_upload",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                response.SetIncludeSnapshot();

                var modalState = tab.TryGetModalState("fileChooser");
                if (modalState?.FileChooser is not IFileChooser fileChooser)
                {
                    throw new InvalidOperationException("No file chooser visible. Use an action that triggers a file chooser before uploading files.");
                }

                tab.TryClearModalState(modalState);

                var literal = paths is null
                    ? "undefined"
                    : "[" + string.Join(", ", paths.Select(QuoteJsString)) + "]";
                response.AddCode($"await fileChooser.setFiles({literal});");

                await tab.WaitForCompletionAsync(async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    if (paths is { Length: > 0 })
                    {
                        await fileChooser.SetFilesAsync(paths).ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
