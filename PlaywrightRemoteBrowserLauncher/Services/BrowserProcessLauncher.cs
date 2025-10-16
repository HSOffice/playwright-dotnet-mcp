using System.Diagnostics;

namespace PlaywrightRemoteBrowserLauncher.Services;

public sealed class BrowserProcessLauncher : IDisposable
{
    private Process? _process;

    public Process? Process => _process;

    public bool Start(string exePath, int port, string userDataDir, string? proxyArguments, Action<string> log)
    {
        if (!File.Exists(exePath))
        {
            log($"未找到可执行文件: {exePath}");
            return false;
        }

        Directory.CreateDirectory(userDataDir);
        Stop();

        var args = $"--remote-debugging-port={port} --user-data-dir=\"{userDataDir}\"";
        if (!string.IsNullOrWhiteSpace(proxyArguments))
        {
            args += $" --proxy-server=\"{proxyArguments}\"";
        }

        var info = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _process = Process.Start(info);
        if (_process is null)
        {
            log("无法启动进程。");
            return false;
        }

        log($"已启动：{exePath}");
        return true;
    }

    public void Stop()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(true);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _process = null;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
