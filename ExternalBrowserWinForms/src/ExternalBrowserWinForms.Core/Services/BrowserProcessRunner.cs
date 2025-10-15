using System.Diagnostics;
using ExternalBrowserWinForms.Core.Models;

namespace ExternalBrowserWinForms.Core.Services;

/// <summary>
/// Launches external browser processes.
/// </summary>
public sealed class BrowserProcessRunner : IBrowserProcessRunner
{
    public void Launch(BrowserLaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UseDefaultBrowser)
        {
            var shellStart = new ProcessStartInfo
            {
                FileName = request.Url.ToString(),
                UseShellExecute = true
            };

            _ = Process.Start(shellStart) ?? throw new InvalidOperationException("无法启动默认浏览器。");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.BrowserPath))
        {
            throw new InvalidOperationException("缺少浏览器路径。");
        }

        var arguments = string.IsNullOrWhiteSpace(request.AdditionalArguments)
            ? request.Url.ToString()
            : string.Join(" ", request.AdditionalArguments, request.Url.ToString());

        var startInfo = new ProcessStartInfo
        {
            FileName = request.BrowserPath,
            Arguments = arguments,
            UseShellExecute = false
        };

        _ = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动指定的浏览器。");
    }
}
