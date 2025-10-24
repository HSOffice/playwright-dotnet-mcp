using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlaywrightRemoteBrowserLauncher.Services;

public sealed class BrowserProcessLauncher : IDisposable
{
    private Process? _process;
    private bool _ownsProcess;
    private string? _processExecutablePath;

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

        if (_process is { HasExited: false } &&
            _processExecutablePath is not null &&
            string.Equals(_processExecutablePath, executablePath, StringComparison.OrdinalIgnoreCase))
        {
            log($"进程已在运行 (PID {_process.Id})：{executablePath}");
            return true;
        }

        Stop();

        var existingProcess = FindExistingProcess(executablePath);
        if (existingProcess is not null)
        {
            _process = existingProcess;
            _ownsProcess = false;
            _processExecutablePath = executablePath;
            log($"已获取现有进程 (PID {existingProcess.Id})：{executablePath}");
            return true;
        }

        if (!IsPortAvailable(port, out var portError))
        {
            log(portError ?? $"❌ 端口 {port} 当前不可用，请更换端口后重试。");
            return false;
        }

        Directory.CreateDirectory(userDataDir);

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

        info.Environment["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = $"--remote-debugging-port={port}";

        _process = Process.Start(info);
        if (_process is null)
        {
            log("无法启动进程。");
            return false;
        }

        _ownsProcess = true;
        _processExecutablePath = executablePath;
        log($"已启动新进程 (PID {_process.Id})：{executablePath}");
        return true;
    }

    public void Stop()
    {
        var process = _process;
        try
        {
            if (process is { HasExited: false } && _ownsProcess)
            {
                process.Kill(true);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _process = null;
            _processExecutablePath = null;
            _ownsProcess = false;

            try
            {
                process?.Dispose();
            }
            catch
            {
                // ignored
            }
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

    private static Process? FindExistingProcess(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        try
        {
            var processName = Path.GetFileNameWithoutExtension(executablePath);
            if (string.IsNullOrWhiteSpace(processName))
            {
                return null;
            }

            var candidates = Process.GetProcessesByName(processName);
            Process? matched = null;
            foreach (var process in candidates)
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(path) && process is { HasExited: false } &&
                        string.Equals(path, executablePath, StringComparison.OrdinalIgnoreCase))
                    {
                        matched = process;
                        break;
                    }
                }
                catch (Win32Exception)
                {
                    // ignore processes that cannot be inspected
                }
                catch (InvalidOperationException)
                {
                    // process exited during inspection
                }
            }

            foreach (var process in candidates)
            {
                if (!ReferenceEquals(process, matched))
                {
                    process.Dispose();
                }
            }

            return matched;
        }
        catch (Exception)
        {
            // ignore errors when searching for existing processes
        }

        return null;
    }

    private static bool IsPortAvailable(int port, out string? errorMessage)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Server.ExclusiveAddressUse = true;
            listener.Start();
            listener.Stop();
            errorMessage = null;
            return true;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            errorMessage = $"❌ 端口 {port} 已被其他进程占用，请更换端口后重试。";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"❌ 无法检测端口 {port} 是否可用：{ex.Message}";
            return false;
        }
    }
}
