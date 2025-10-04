using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "install.browser")]
    [Description("Installs the required browser binaries.")]
    public static Task<string> InstallBrowserAsync(
        [Description("Name of the browser channel to install (e.g., chromium, firefox, webkit, msedge).")] string browser,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var message = $"Automatic installation for '{browser}' is not yet implemented in the .NET server.";
        return Task.FromResult(Serialize(new
        {
            installed = false,
            browser,
            message
        }));
    }
}
