using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "dialogs.accept")]
    [Description("Accepts the next dialog presented by the page.")]
    public static async Task<string> DialogsAcceptAsync(
        [Description("Optional text to provide when accepting prompt dialogs.")] string? promptText = null,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        var completion = new TaskCompletionSource<(string Type, string Message, string? Default)>(TaskCreationOptions.RunContinuationsAsynchronously);

        async void Handler(object? sender, IDialog dialog)
        {
            page.Dialog -= Handler;
            try
            {
                if (dialog.Type == DialogType.Prompt && promptText is not null)
                {
                    await dialog.AcceptAsync(promptText).ConfigureAwait(false);
                }
                else
                {
                    await dialog.AcceptAsync().ConfigureAwait(false);
                }

                completion.TrySetResult((dialog.Type, dialog.Message, dialog.DefaultValue));
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        }

        page.Dialog += Handler;
        using var registration = cancellationToken.Register(() =>
        {
            page.Dialog -= Handler;
            completion.TrySetCanceled(cancellationToken);
        });

        var result = await completion.Task.ConfigureAwait(false);

        return Serialize(new
        {
            accepted = true,
            type = result.Type,
            message = result.Message,
            defaultValue = result.Default,
            promptText
        });
    }

    [McpServerTool(Name = "evaluate.script")]
    [Description("Evaluates a script within the active page.")]
    public static async Task<string> EvaluateScriptAsync(
        [Description("JavaScript expression or function to evaluate.")] string script,
        [Description("Optional JSON encoded argument passed to the script as the first parameter.")] string? jsonArgument = null,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        object? argument = null;

        if (!string.IsNullOrWhiteSpace(jsonArgument))
        {
            try
            {
                argument = JsonSerializer.Deserialize<object?>(jsonArgument);
            }
            catch (JsonException ex)
            {
                return Serialize(new
                {
                    evaluated = false,
                    error = ex.Message
                });
            }
        }

        var result = await page.EvaluateAsync<object?>(script, argument).ConfigureAwait(false);

        return Serialize(new
        {
            evaluated = true,
            result
        });
    }
}
