using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_console_messages")]
    [Description("Returns all console messages.")]
    public static async Task<string> BrowserConsoleMessagesAsync(
        [Description("Only return error messages.")] bool? onlyErrors = null,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (onlyErrors.HasValue)
        {
            args["onlyErrors"] = onlyErrors.Value;
        }

        return await ExecuteWithResponseAsync(
            "browser_console_messages",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var messages = tab.GetConsoleMessages(onlyErrors.GetValueOrDefault());

                if (messages.Count == 0)
                {
                    response.AddResult(onlyErrors.GetValueOrDefault()
                        ? "No error console messages."
                        : "No console messages.");
                }
                else
                {
                    response.AddResult(Serialize(messages));
                }
            },
            cancellationToken).ConfigureAwait(false);
    }
}
