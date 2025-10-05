using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ModelContextProtocol.Server;

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
    private static readonly SnapshotManager SnapshotManager = new();
    private static readonly TabManager TabManager = new();
    private static readonly Dictionary<string, ToolMetadata> ToolRegistry = new(StringComparer.OrdinalIgnoreCase);

    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static IBrowserContext? _context;
    private static string _browserEngine = "chromium";
    private static bool _tracingActive;

    static PlaywrightTools()
    {
        RegisterTool(new ToolMetadata("browser_relaunch", "(Re)launch browser and open a fresh page.", "action", "core-install"));
        RegisterTool(new ToolMetadata("browser_close", "Close and dispose Playwright browser resources.", "action", "core"));
        RegisterTool(new ToolMetadata("browser_navigate", "Navigate to a URL.", "action", "core"));
        RegisterTool(new ToolMetadata("browser_navigate_back", "Go back to the previous page.", "action", "core"));
    }

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

    private static string PdfDir =>
        Path.GetFullPath("./pdf");

    private static string TracesDir =>
        Path.GetFullPath("./traces");

    private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);

    private static async Task EnsureLaunchedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureDirectories();

        _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);

        if (_browser is null)
        {
            _browser = await LaunchBrowserAsync(cancellationToken).ConfigureAwait(false);
        }

        if (_context is null)
        {
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true,
                RecordVideoDir = VideosDir,
                ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
                Geolocation = new Geolocation { Latitude = 0, Longitude = 0 }
            }).ConfigureAwait(false);

            _context.Page += ContextOnPage;
            await _context.GrantPermissionsAsync(new[]
            {
                "clipboard-read",
                "clipboard-write",
                "geolocation",
                "notifications"
            }).ConfigureAwait(false);

            foreach (var page in _context.Pages.Where(p => !p.IsClosed))
            {
                TabManager.Register(page, makeActive: TabManager.ActiveTab is null);
            }
        }

        var active = TabManager.ActiveTab;
        if (active is null || active.Page.IsClosed)
        {
            var page = await _context.NewPageAsync().ConfigureAwait(false);
            active = TabManager.Register(page, makeActive: true);
        }

        TabManager.Activate(active);
    }

    private static async Task<IBrowser> LaunchBrowserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = new BrowserTypeLaunchOptions
        {
            Headless = Headless
        };

        return _browserEngine switch
        {
            "firefox" => await _playwright!.Firefox.LaunchAsync(options).ConfigureAwait(false),
            "webkit" => await _playwright!.Webkit.LaunchAsync(options).ConfigureAwait(false),
            _ => await LaunchChromiumAsync(options).ConfigureAwait(false)
        };
    }

    private static async Task<IBrowser> LaunchChromiumAsync(BrowserTypeLaunchOptions options)
    {
        // If the environment specifies a channel prefer it, otherwise default to Microsoft Edge similar to TS runtime.
        var channel = Environment.GetEnvironmentVariable("MCP_PLAYWRIGHT_CHROMIUM_CHANNEL");
        if (!string.IsNullOrWhiteSpace(channel))
        {
            options.Channel = channel;
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            options.Channel = "msedge";
        }

        return await _playwright!.Chromium.LaunchAsync(options).ConfigureAwait(false);
    }

    private static async Task<IPage> GetPageAsync(CancellationToken cancellationToken)
    {
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);
        return TabManager.ActiveTab?.Page ?? throw new InvalidOperationException("Active page not initialized.");
    }

    private static async Task<TabState> GetActiveTabAsync(CancellationToken cancellationToken)
    {
        await EnsureLaunchedAsync(cancellationToken).ConfigureAwait(false);
        return TabManager.ActiveTab ?? throw new InvalidOperationException("Active tab not available.");
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

    private static void ContextOnPage(object? sender, IPage page)
    {
        TabManager.Register(page, makeActive: false);
    }

    public static SnapshotPayload? TryGetLastSnapshot()
    {
        return TabManager.ActiveTab?.LastSnapshot;
    }

    public static async Task<SnapshotPayload> GetAriaSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var tab = await GetActiveTabAsync(cancellationToken).ConfigureAwait(false);
        return await SnapshotManager.CaptureAsync(tab, cancellationToken).ConfigureAwait(false);
    }

    public static IReadOnlyList<TabDescriptor> DescribeTabs()
    {
        return TabManager.DescribeTabs();
    }

    internal static IReadOnlyDictionary<string, ToolMetadata> RegisteredToolMetadata => ToolRegistry;

    private static void RegisterTool(ToolMetadata metadata)
    {
        ToolRegistry[metadata.Name] = metadata;
    }

    private static void EnsureDirectories()
    {
        lock (Gate)
        {
            Directory.CreateDirectory(DownloadsDir);
            Directory.CreateDirectory(VideosDir);
            Directory.CreateDirectory(ShotsDir);
            Directory.CreateDirectory(PdfDir);
            Directory.CreateDirectory(TracesDir);
        }
    }

    private static MouseButton? ParseMouseButton(string? button)
    {
        if (string.IsNullOrWhiteSpace(button)) return null;
        return button.Trim().ToLowerInvariant() switch
        {
            "left" => MouseButton.Left,
            "middle" => MouseButton.Middle,
            "right" => MouseButton.Right,
            _ => null
        };
    }

    private static IEnumerable<KeyboardModifier>? ParseModifiers(string[]? modifiers)
    {
        if (modifiers is null || modifiers.Length == 0) return null;

        var list = new List<KeyboardModifier>(modifiers.Length);
        foreach (var modifier in modifiers)
        {
            switch (modifier)
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
                    break;
            }
        }

        return list.Distinct().ToArray();
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL must not be empty.", nameof(url));
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (Uri.TryCreate($"https://{url}", UriKind.Absolute, out var https))
        {
            return https.ToString();
        }

        throw new ArgumentException("Invalid URL format.", nameof(url));
    }

    internal sealed record ToolMetadata(string Name, string Title, string Type, string Capability);
}
