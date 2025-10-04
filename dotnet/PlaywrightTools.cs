using ChatUiTest.Playwright.Helper;
using Microsoft.Playwright;
using ModelContextProtocol.Server;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaywrightMcpServer;

[McpServerToolType]
public sealed partial class PlaywrightTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly object Gate = new();
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static IBrowserContext? _context;
    private static IPage? _page;
    private static bool _isChromium;
    private static bool _tracingActive;

    private static readonly List<ConsoleMessageEntry> ConsoleMessages = new();
    private static readonly List<NetworkRequestEntry> NetworkRequests = new();
    private static readonly Dictionary<IRequest, NetworkRequestEntry> NetworkRequestMap = new();

    private static bool Headless =>
        (Environment.GetEnvironmentVariable("MCP_PLAYWRIGHT_HEADLESS") ?? "false")
        .Equals("true", StringComparison.OrdinalIgnoreCase);

    private static string DownloadsDir =>
        Environment.GetEnvironmentVariable("MCP_PLAYWRIGHT_DOWNLOADS_DIR") ??
        Path.GetFullPath("./downloads");

    private static string VideosDir =>
        Environment.GetEnvironmentVariable("MCP_PLAYWRIGHT_VIDEOS_DIR") ??
        Path.GetFullPath("./videos");

    private static string ShotsDir =>
        Path.GetFullPath("./shots");

    private static string TracesDir =>
        Path.GetFullPath("./traces");

    private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);

    private static async Task EnsureLaunchedAsync(CancellationToken cancellationToken)
    {
        if (_page is { IsClosed: true })
            _page = null;
        if (_page is not null)
            return;

        EnsureDirectories();

        _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);

        if (_browser is null)
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Headless,
                Channel = "msedge"
            }).ConfigureAwait(false);
            _isChromium = true;
        }

        if (_context is not null)
        {
            try { await _context.CloseAsync().ConfigureAwait(false); }
            catch { }
            _context = null;
        }

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            RecordVideoDir = VideosDir,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
            Geolocation = new Geolocation { Latitude = 0, Longitude = 0 }
        }).ConfigureAwait(false);

        await _context.GrantAllPermissionsAsync().ConfigureAwait(false);

        var page = await _context.NewPageAsync().ConfigureAwait(false);
        SetActivePage(page);
    }

    private static async Task<IPage> GetPageAsync(CancellationToken cancellationToken)
    {
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);
        return _page ?? throw new InvalidOperationException("Active page not initialized.");
    }

    private static async Task<IBrowserContext> GetContextAsync(CancellationToken cancellationToken)
    {
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);
        return _context ?? throw new InvalidOperationException("Browser context not initialized.");
    }

    private static async Task<ILocator> GetLocatorAsync(
        string selector,
        int? timeoutMs,
        CancellationToken cancellationToken)
    {
        var page = await GetPageAsync(cancellationToken).ConfigureAwait(false);
        var locator = page.Locator(selector).First;
        await locator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs }).ConfigureAwait(false);
        return locator;
    }

    private static string ResolveOutputPath(string outputPath, string defaultDirectory)
    {
        var basePath = Path.GetFullPath(defaultDirectory);
        return Path.GetFullPath(Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(basePath, outputPath));
    }

    private static void SetActivePage(IPage page)
    {
        if (_page == page)
            return;

        if (_page is not null)
            DetachPageEventHandlers(_page);

        _page = page;

        lock (ConsoleMessages)
        {
            ConsoleMessages.Clear();
        }

        lock (NetworkRequests)
        {
            NetworkRequests.Clear();
            NetworkRequestMap.Clear();
        }

        AttachPageEventHandlers(page);
    }

    private static void AttachPageEventHandlers(IPage page)
    {
        page.Console += PageOnConsole;
        page.Request += PageOnRequest;
        page.Response += PageOnResponse;
        page.RequestFailed += PageOnRequestFailed;
    }

    private static void DetachPageEventHandlers(IPage page)
    {
        page.Console -= PageOnConsole;
        page.Request -= PageOnRequest;
        page.Response -= PageOnResponse;
        page.RequestFailed -= PageOnRequestFailed;
    }

    private static void PageOnConsole(object? sender, IConsoleMessage message)
    {
        var entry = new ConsoleMessageEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Type = message.Type,
            Text = message.Text,
            Args = message.Args.Select(arg => arg.ToString()).Where(arg => arg is not null).Cast<string>().ToArray()
        };

        lock (ConsoleMessages)
        {
            ConsoleMessages.Add(entry);
            if (ConsoleMessages.Count > 200)
                ConsoleMessages.RemoveAt(0);
        }
    }

    private static void PageOnRequest(object? sender, IRequest request)
    {
        var entry = new NetworkRequestEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Method = request.Method,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Failure = null,
            Status = null
        };

        lock (NetworkRequests)
        {
            NetworkRequests.Add(entry);
            NetworkRequestMap[request] = entry;
            if (NetworkRequests.Count > 500)
            {
                var first = NetworkRequests.First();
                NetworkRequests.RemoveAt(0);
                var keyToRemove = NetworkRequestMap.FirstOrDefault(kvp => kvp.Value == first).Key;
                if (keyToRemove is not null)
                    NetworkRequestMap.Remove(keyToRemove);
            }
        }
    }

    private static void PageOnResponse(object? sender, IResponse response)
    {
        lock (NetworkRequests)
        {
            if (NetworkRequestMap.TryGetValue(response.Request, out var entry))
            {
                entry.Status = response.Status;
            }
        }
    }

    private static void PageOnRequestFailed(object? sender, IRequest request)
    {
        lock (NetworkRequests)
        {
            if (NetworkRequestMap.TryGetValue(request, out var entry))
            {
                entry.Failure = request.Failure;
            }
        }
    }

    private static ConsoleMessageEntry[] SnapshotConsoleMessages()
    {
        lock (ConsoleMessages)
        {
            return ConsoleMessages.ToArray();
        }
    }

    private static NetworkRequestEntry[] SnapshotNetworkRequests()
    {
        lock (NetworkRequests)
        {
            return NetworkRequests.Select(r => r.Clone()).ToArray();
        }
    }

    private static void EnsureDirectories()
    {
        lock (Gate)
        {
            Directory.CreateDirectory(DownloadsDir);
            Directory.CreateDirectory(VideosDir);
            Directory.CreateDirectory(ShotsDir);
            Directory.CreateDirectory(TracesDir);
        }
    }

    private static int GetPageIndex(IPage page)
    {
        if (_context is null)
            return -1;

        var match = _context.Pages
            .Select((p, idx) => new { Page = p, Index = idx })
            .FirstOrDefault(tuple => tuple.Page == page);
        return match?.Index ?? -1;
    }

    #region helpers

    public sealed record ConsoleMessageEntry
    {
        [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; }
        [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
        [JsonPropertyName("args")][AllowNull] public string[]? Args { get; init; }
    }

    public sealed class NetworkRequestEntry
    {
        [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; }
        [JsonPropertyName("method")] public string Method { get; init; } = string.Empty;
        [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
        [JsonPropertyName("resourceType")][AllowNull] public string? ResourceType { get; init; }
        [JsonPropertyName("status")][AllowNull] public int? Status { get; set; }
        [JsonPropertyName("failure")][AllowNull] public string? Failure { get; set; }

        public NetworkRequestEntry Clone() => new()
        {
            Timestamp = Timestamp,
            Method = Method,
            Url = Url,
            ResourceType = ResourceType,
            Status = Status,
            Failure = Failure
        };
    }

    // ----------------------
    // Helper methods (本文件内提供，解决“当前上下文中不存在名称 ...”)
    // ----------------------

    private static MouseButton? ParseMouseButton(string? button)
    {
        if (string.IsNullOrWhiteSpace(button)) return null;
        switch (button.Trim().ToLowerInvariant())
        {
            case "left": return MouseButton.Left;
            case "middle": return MouseButton.Middle;
            case "right": return MouseButton.Right;
            default: return null; // 未识别则交给 Playwright 默认（Left）
        }
    }

    private static IEnumerable<KeyboardModifier>? ParseModifiers(string[]? modifiers)
    {
        if (modifiers == null || modifiers.Length == 0) return null;

        var list = new List<KeyboardModifier>(modifiers.Length);
        foreach (var m in modifiers)
        {
            switch (m)
            {
                case "Alt":
                    list.Add(KeyboardModifier.Alt);
                    break;
                case "Control":
                    list.Add(KeyboardModifier.Control);
                    break;
                case "Meta":
                    list.Add(KeyboardModifier.Meta);
                    break;
                case "Shift":
                    list.Add(KeyboardModifier.Shift);
                    break;
                case "ControlOrMeta":
                    list.Add(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                        ? KeyboardModifier.Meta
                        : KeyboardModifier.Control);
                    break;
                default:
                    // 忽略未知修饰键，保持宽容
                    break;
            }
        }
        // 去重
        return list.Distinct().ToArray();
    }

    #endregion
}
