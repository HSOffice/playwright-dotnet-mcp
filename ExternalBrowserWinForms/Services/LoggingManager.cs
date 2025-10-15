using System.Text.Json;

namespace ExternalBrowserWinForms.Services;

public sealed class LoggingManager
{
    private readonly string _logsRoot;
    private readonly Action<string> _log;

    public LoggingManager(string logsRoot, Action<string> log)
    {
        _logsRoot = logsRoot;
        _log = log;
    }

    public bool IsEnabled { get; private set; }

    public string? ConsoleLogPath { get; private set; }

    public string? NetworkLogPath { get; private set; }

    public void Start()
    {
        Directory.CreateDirectory(_logsRoot);
        var stem = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        ConsoleLogPath = Path.Combine(_logsRoot, $"console-{stem}.ndjson");
        NetworkLogPath = Path.Combine(_logsRoot, $"network-{stem}.csv");

        File.WriteAllText(NetworkLogPath, "time,page,kind,method,url,status\n");
        IsEnabled = true;
        _log($"日志开始：console->{ConsoleLogPath}, network->{NetworkLogPath}");
    }

    public void Stop()
    {
        IsEnabled = false;
        _log("日志已停止。");
    }

    public void WriteConsole(string type, string text)
    {
        if (!IsEnabled || ConsoleLogPath is null)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                ts = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                type,
                text
            });
            File.AppendAllText(ConsoleLogPath, json + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }

    public void WriteNetwork(string pageName, string kind, string method, string url, int? status = null)
    {
        if (!IsEnabled || NetworkLogPath is null)
        {
            return;
        }

        try
        {
            var statusValue = status?.ToString() ?? string.Empty;
            var csv = $"{DateTime.Now:HH:mm:ss},{pageName},{kind},{method},{url},{statusValue}";
            File.AppendAllText(NetworkLogPath, csv + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }
}
