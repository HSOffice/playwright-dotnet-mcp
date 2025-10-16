using System;
using System.Diagnostics;
using System.Text;

namespace PlaywrightRemoteBrowserLauncher.Services;

public sealed class BrowserProcessLauncher : IDisposable
{
    private Process? _process;

    public Process? Process => _process;

    public bool Start(string exePath, int port, string userDataDir, string? proxyArguments, Action<string> log)
    {
        if (string.IsNullOrWhiteSpace(exePath))
        {
            log("未指定可执行文件路径。");
            return false;
        }

        var (executablePath, extraArguments) = SplitExecutableAndArguments(exePath);

        if (!File.Exists(executablePath))
        {
            log($"未找到可执行文件: {executablePath}");
            return false;
        }

        Directory.CreateDirectory(userDataDir);
        Stop();

        var argsBuilder = new StringBuilder();
        var containsRemotePortArgument = false;
        if (!string.IsNullOrWhiteSpace(extraArguments))
        {
            var normalizedExtra = extraArguments.Trim();
            containsRemotePortArgument = normalizedExtra.Contains("--remote-debugging-port", StringComparison.OrdinalIgnoreCase);

            if (normalizedExtra.Length > 0)
            {
                argsBuilder.Append(normalizedExtra);
                if (!normalizedExtra.EndsWith(' '))
                {
                    argsBuilder.Append(' ');
                }
            }
        }

        if (!containsRemotePortArgument)
        {
            argsBuilder.Append($"--remote-debugging-port={port} ");
        }
        else if (extraArguments is not null && !extraArguments.Contains($"--remote-debugging-port={port}", StringComparison.OrdinalIgnoreCase))
        {
            log($"⚠️ 命令行中已包含 --remote-debugging-port 参数，请确认端口与界面中的 {port} 一致。");
        }

        argsBuilder.Append($"--user-data-dir=\"{userDataDir}\"");
        if (!string.IsNullOrWhiteSpace(proxyArguments))
        {
            argsBuilder.Append($" --proxy-server=\"{proxyArguments}\"");
        }

        var info = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = argsBuilder.ToString(),
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _process = Process.Start(info);
        if (_process is null)
        {
            log("无法启动进程。");
            return false;
        }

        log($"已启动：{executablePath}");
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

    private static (string ExecutablePath, string? ExtraArguments) SplitExecutableAndArguments(string commandLine)
    {
        var span = commandLine.AsSpan().Trim();
        if (span.IsEmpty)
        {
            return (string.Empty, null);
        }

        var inQuotes = false;
        var builder = new StringBuilder();
        var index = 0;

        while (index < span.Length)
        {
            var current = span[index];
            if (current == '\"')
            {
                inQuotes = !inQuotes;
                index++;
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(current))
            {
                break;
            }

            builder.Append(current);
            index++;
        }

        while (index < span.Length && char.IsWhiteSpace(span[index]))
        {
            index++;
        }

        var executable = builder.ToString();
        var extraArguments = index < span.Length ? span[index..].ToString() : null;
        return (executable, string.IsNullOrWhiteSpace(extraArguments) ? null : extraArguments);
    }
}
