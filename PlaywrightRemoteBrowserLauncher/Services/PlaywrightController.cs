using PlaywrightRemoteBrowserLauncher.Models;
using Microsoft.Playwright;

namespace PlaywrightRemoteBrowserLauncher.Services;

public sealed class PlaywrightController : IAsyncDisposable
{
    private readonly LoggingManager _loggingManager;
    private readonly Action<string> _log;
    private readonly Dictionary<IPage, string> _pageNames = new();

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _primaryPage;
    private int _pageCounter;
    private bool _shuttingDown;

    public PlaywrightController(LoggingManager loggingManager, Action<string> log)
    {
        _loggingManager = loggingManager;
        _log = log;
    }

    public event Action<PageItem>? PageAttached;

    public event Action<IPage>? PageClosed;

    public IBrowserContext? Context => _context;

    public IPage? CurrentPage => _primaryPage;

    public async Task ConnectAsync(int port)
    {
        _playwright ??= await Playwright.CreateAsync();
        _log("连接 Playwright（CDP）…");
        _browser = await _playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{port}");
        _log("✅ 已连接到浏览器");
    }

    public async Task EnsureContextAsync(ContextConfiguration config)
    {
        if (_browser is null)
        {
            throw new InvalidOperationException("尚未连接浏览器。");
        }

        if (_context is not null)
        {
            return;
        }

        var options = new BrowserNewContextOptions
        {
            AcceptDownloads = config.AcceptDownloads,
            IgnoreHTTPSErrors = config.IgnoreHttpsErrors,
        };

        if (config.RecordHar)
        {
            options.RecordHarPath = config.RecordHarPath;
            options.RecordHarOmitContent = false;
            if (!string.IsNullOrWhiteSpace(config.RecordHarPath))
            {
                var directory = Path.GetDirectoryName(config.RecordHarPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            _log($"HAR 录制中：{config.RecordHarPath}");
        }

        _context = await _browser.NewContextAsync(options);

        if (!string.IsNullOrWhiteSpace(config.InitScript))
        {
            await _context.AddInitScriptAsync(config.InitScript);
            _log("已注册 AddInitScript。");
        }

        if (config.ExposeDotnet)
        {
            await _context.ExposeFunctionAsync(config.ExposedFunctionName,
                (Func<string, string>)(s => $"pong:{s}:{DateTime.Now:HHmmss}"));
            _log($"已暴露 .NET 方法：{config.ExposedFunctionName}(str) => returns 'pong:...' ");
        }

        _context.Page += (_, page) => AttachPage(page, downloadDirectory: null);
    }

    public async Task<IPage> CreatePageAsync(string downloadDirectory)
    {
        if (_context is null)
        {
            throw new InvalidOperationException("尚未创建 Context。");
        }

        if (!Directory.Exists(downloadDirectory))
        {
            Directory.CreateDirectory(downloadDirectory);
        }

        var page = await _context.NewPageAsync();
        AttachPage(page, downloadDirectory);
        _primaryPage = page;
        _log("✅ 已创建 Context + Page");
        return page;
    }

    public void SelectPage(IPage? page)
    {
        _primaryPage = page;
    }

    public async Task<bool> NavigateAsync(IPage page, string url, int retries, int delayMs, string? postNavigationScript)
    {
        for (var attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                _log($"🌐 访问 {url} … (尝试 {attempt + 1}/{retries + 1})");
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                var title = await page.TitleAsync();
                _log("页面标题: " + title);

                if (!string.IsNullOrWhiteSpace(postNavigationScript))
                {
                    try
                    {
                        var result = await page.EvaluateAsync<string>(postNavigationScript);
                        _log($"PostNav Evaluate 结果: {result}");
                    }
                    catch (Exception ex)
                    {
                        _log("PostNav Evaluate 失败：" + ex.Message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log($"导航失败：{ex.Message}");
                if (attempt < retries)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        return false;
    }

    public async Task<bool> CaptureScreenshotAsync(string path)
    {
        var page = _primaryPage;
        if (page is null)
        {
            _log("无法截图：当前没有页面。");
            return false;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            _log("截图失败：路径为空。");
            return false;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        _log($"📸 已保存截图：{path}");
        return true;
    }

    public async Task SaveSnapshotAsync(string directory, int port)
    {
        if (_primaryPage is null)
        {
            throw new InvalidOperationException("保存快照失败：当前没有页面。");
        }

        Directory.CreateDirectory(directory);
        var htmlPath = Path.Combine(directory, "page.html");
        var pngPath = Path.Combine(directory, "page.png");
        var jsonPath = Path.Combine(directory, "version.json");

        var html = await _primaryPage.ContentAsync();
        await File.WriteAllTextAsync(htmlPath, html);
        await _primaryPage.ScreenshotAsync(new PageScreenshotOptions { Path = pngPath, FullPage = true });

        using var http = new HttpClient();
        var version = await http.GetStringAsync($"http://127.0.0.1:{port}/json/version");
        await File.WriteAllTextAsync(jsonPath, version);

        _log($"✅ 快照已保存：{directory}");
    }

    public async Task ExportJsonAsync(string endpoint, string destinationPath)
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync(endpoint);
        await File.WriteAllTextAsync(destinationPath, json);
        _log($"✅ 已导出 {endpoint}：{destinationPath}");
    }

    public async Task CleanupAsync()
    {
        if (_shuttingDown)
        {
            return;
        }

        _shuttingDown = true;
        try
        {
            _log("开始清理…");
            foreach (var ctx in _browser?.Contexts ?? Array.Empty<IBrowserContext>())
            {
                foreach (var page in ctx.Pages)
                {
                    await page.CloseAsync();
                }
                await ctx.CloseAsync();
            }

            if (_browser is not null)
            {
                await _browser.CloseAsync();
            }

            _pageNames.Clear();
            _primaryPage = null;
            _context = null;
            _browser = null;
            _log("清理完成。");
        }
        catch (Exception ex)
        {
            _log("清理异常：" + ex);
        }
        finally
        {
            _shuttingDown = false;
        }
    }

    private void AttachPage(IPage page, string? downloadDirectory)
    {
        if (_pageNames.ContainsKey(page))
        {
            return;
        }

        var name = $"Page-{++_pageCounter}";
        _pageNames[page] = name;

        if (!string.IsNullOrWhiteSpace(downloadDirectory))
        {
            page.Download += async (_, download) =>
            {
                try
                {
                    var destination = Path.Combine(downloadDirectory, download.SuggestedFilename);
                    await download.SaveAsAsync(destination);
                    _log($"⬇️ 下载完成：{destination}");
                }
                catch (Exception ex)
                {
                    _log("保存下载失败：" + ex);
                }
            };
        }

        page.Console += (_, e) =>
        {
            var line = $"[Console][{name}][{e.Type}] {e.Text}";
            _log(line);
            _loggingManager.WriteConsole(e.Type.ToString(), e.Text);
        };

        page.Request += (_, request) =>
        {
            _loggingManager.WriteNetwork(name, "REQ", request.Method, request.Url);
        };

        page.Response += (_, response) =>
        {
            _loggingManager.WriteNetwork(name, "RES", response.Request.Method, response.Url, response.Status);
        };

        page.Close += (_, _) =>
        {
            if (_shuttingDown)
            {
                return;
            }

            _log($"页面关闭：{name}");
            _pageNames.Remove(page);
            PageClosed?.Invoke(page);
        };

        PageAttached?.Invoke(new PageItem(page, name));
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync();
        if (_playwright is not null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
    }
}
