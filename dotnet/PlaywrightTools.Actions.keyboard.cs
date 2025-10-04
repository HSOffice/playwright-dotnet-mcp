using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_press_key")]
    [Description("Press a key on the keyboard.")]
    public static async Task<string> BrowserPressKeyAsync(
        [Description("Name of the key to press or a character to generate, such as `ArrowLeft` or `a`.")] string key,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement tool logic for simulating a keyboard key press.
        // Pseudocode:
        // 1. Retrieve the active page instance.
        // 2. Issue the key press event using the specified key identifier.
        // 3. Return a serialized result detailing the key action performed.
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    [McpServerTool(Name = "browser_type")]
    [Description("Type text into editable element.")]
    public static async Task<string> BrowserTypeAsync(
        [Description("Human-readable element description used to obtain permission to interact with the element.")] string element,
        [Description("Exact target element reference from the page snapshot.")] string elementRef,
        [Description("Text to type into the element.")] string text,
        [Description("Whether to submit entered text (press Enter after).")] bool? submit = null,
        [Description("Whether to type one character at a time. Useful for triggering key handlers in the page. By default entire text is filled in at once.")] bool? slowly = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement tool logic for typing text into a specified element.
        // Pseudocode:
        // 1. Locate the element using the provided descriptors.
        // 2. Type the supplied text, honoring the submit and slowly flags as needed.
        // 3. Return a serialized summary of the typing interaction.
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}
