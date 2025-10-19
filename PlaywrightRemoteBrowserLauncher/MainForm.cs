using System.ComponentModel;
using System.Diagnostics;
using PlaywrightRemoteBrowserLauncher.Extensions;
using PlaywrightRemoteBrowserLauncher.Models;
using PlaywrightRemoteBrowserLauncher.Services;
using Microsoft.Playwright;

namespace PlaywrightRemoteBrowserLauncher;

public partial class MainForm : Form
{
    private readonly Lazy<RuntimeState> _runtime;
    private readonly BrowserProcessLauncher _processLauncher = new();

    private CancellationTokenSource? _waitCancellation;

    public MainForm()
    {
        InitializeComponent();

        _runtime = new Lazy<RuntimeState>(() => RuntimeState.Create(AppendLog));

        if (IsDesignMode())
        {
            return;
        }

        var runtime = Runtime;
        runtime.Playwright.PageAttached += OnPageAttached;
        runtime.Playwright.PageClosed += OnPageClosed;

        txtExePath.Text = @"..\..\..\..\WebView2BrowserHost\bin\Debug\net8.0-windows\WebView2BrowserHost.exe";
        numPort.Value = 9222;
        txtStartUrl.Text = "https://example.com";
    }

    private RuntimeState Runtime => _runtime.Value;

    private LoggingManager LoggingManager => Runtime.LoggingManager;

    private PlaywrightController Playwright => Runtime.Playwright;

    private string LogsRoot => Runtime.LogsRoot;

    private string DownloadsRoot => Runtime.DownloadsRoot;

    private string UserDataRoot => Runtime.UserDataRoot;

    private string ScreenshotsRoot => Runtime.ScreenshotsRoot;

    private string RunLogPath => Runtime.RunLogPath;

    private PageItem? SelectedItem => lstPages.SelectedItem as PageItem;

    private IPage? SelectedPage => SelectedItem?.Page ?? Playwright.CurrentPage;

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        txtLog.InvokeSafe(() =>
        {
            txtLog.AppendText(line + Environment.NewLine);
        });

        if (!_runtime.IsValueCreated)
        {
            return;
        }

        try
        {
            File.AppendAllText(RunLogPath, line + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }

    private async void btnLaunch_Click(object sender, EventArgs e)
    {
        Directory.CreateDirectory(UserDataRoot);
        var commandLine = txtExePath.Text.Trim();
        var portFromCommandLine = ExtractRemoteDebuggingPort(commandLine);
        if (portFromCommandLine is int parsedPort)
        {
            if (parsedPort < (int)numPort.Minimum || parsedPort > (int)numPort.Maximum)
            {
                AppendLog($"⚠️ 命令行中指定的 --remote-debugging-port={parsedPort} 超出可用范围 ({numPort.Minimum}-{numPort.Maximum})，继续使用界面端口 {(int)numPort.Value}。");
            }
            else if ((int)numPort.Value != parsedPort)
            {
                AppendLog($"检测到命令行端口 {parsedPort}，已同步到界面。");
                numPort.Value = parsedPort;
            }
        }
        var started = _processLauncher.Start(
            commandLine,
            (int)numPort.Value,
            UserDataRoot,
            txtProxy.Text.Trim(),
            AppendLog);

        if (started)
        {
            btnWaitDevTools.Enabled = true;
            btnCloseAll.Enabled = true;
        }
    }

    private static int? ExtractRemoteDebuggingPort(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return null;
        }

        const string marker = "--remote-debugging-port=";
        var index = commandLine.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        index += marker.Length;
        if (index >= commandLine.Length)
        {
            return null;
        }

        var span = commandLine.AsSpan(index);
        var end = 0;
        while (end < span.Length && !char.IsWhiteSpace(span[end]))
        {
            end++;
        }

        if (end == 0)
        {
            return null;
        }

        var valueSpan = span[..end].Trim();
        var value = valueSpan.ToString().Trim('\"', '\'');
        return int.TryParse(value, out var port) ? port : null;
    }

    private async void btnWaitDevTools_Click(object sender, EventArgs e)
    {
        btnWaitDevTools.Enabled = false;
        _waitCancellation?.Cancel();
        _waitCancellation = new CancellationTokenSource();

        AppendLog($"等待 DevTools 接口（端口 {(int)numPort.Value}）…");
        try
        {
            var endpoint = await DevToolsEndpointWatcher.WaitForEndpointAsync((int)numPort.Value, TimeSpan.FromSeconds(15), _waitCancellation.Token);
            if (endpoint is null)
            {
                AppendLog("未检测到 DevTools 调试接口，请确认该浏览器支持 --remote-debugging-port。");
            }
            else
            {
                AppendLog($"✅ WebSocket 端点: {endpoint}");
                btnConnect.Enabled = true;
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("等待已取消。");
        }
    }

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;
        try
        {
            await Playwright.ConnectAsync((int)numPort.Value);
            btnNewPage.Enabled = true;
        }
        catch (Exception ex)
        {
            AppendLog("连接失败：" + ex);
        }
    }

    private async void btnNewPage_Click(object sender, EventArgs e)
    {
        try
        {
            var page = await Playwright.AcquireExistingPageAsync();
            if (page is null)
            {
                AppendLog("未能找到现有页面，请确认目标浏览器已打开页面。");
                return;
            }

            btnGoto.Enabled = true;
        }
        catch (Exception ex)
        {
            AppendLog("获取页面失败：" + ex);
        }
    }

    private async void btnGoto_Click(object sender, EventArgs e)
    {
        var page = SelectedPage;
        if (page is null)
        {
            AppendLog("尚未获取 Page。");
            return;
        }

        var url = string.IsNullOrWhiteSpace(txtStartUrl.Text) ? "https://example.com" : txtStartUrl.Text.Trim();
        var success = await Playwright.NavigateAsync(page, url, (int)numRetryCount.Value, (int)numRetryDelayMs.Value, chkPostNavScript.Checked ? txtPostNavScript.Text : null);
        if (success && chkAutoScreenshot.Checked)
        {
            await Playwright.CaptureScreenshotAsync(GenerateScreenshotPath());
        }
    }

    private async void btnRunAll_Click(object sender, EventArgs e)
    {
        var state = ProtectButtonsDuringRunAll();
        try
        {
            Directory.CreateDirectory(UserDataRoot);
            if (!_processLauncher.Start(txtExePath.Text.Trim(), (int)numPort.Value, UserDataRoot, txtProxy.Text.Trim(), AppendLog))
            {
                return;
            }

            btnWaitDevTools_Click(sender, e);
            await Playwright.ConnectAsync((int)numPort.Value);
            var config = new ContextConfiguration
            {
                IgnoreHttpsErrors = chkIgnoreTls.Checked,
                InitScript = chkInitScript.Checked ? txtInitScript.Text : null,
                ExposeDotnet = chkExposeDotnet.Checked,
                ExposedFunctionName = string.IsNullOrWhiteSpace(txtExposeName.Text) ? "dotnetPing" : txtExposeName.Text.Trim(),
                RecordHar = false
            };
            await Playwright.EnsureContextAsync(config);
            Directory.CreateDirectory(DownloadsRoot);
            var page = await Playwright.CreatePageAsync(DownloadsRoot);
            var url = string.IsNullOrWhiteSpace(txtStartUrl.Text) ? "https://example.com" : txtStartUrl.Text.Trim();
            if (await Playwright.NavigateAsync(page, url, (int)numRetryCount.Value, (int)numRetryDelayMs.Value, chkPostNavScript.Checked ? txtPostNavScript.Text : null) && chkAutoScreenshot.Checked)
            {
                await Playwright.CaptureScreenshotAsync(GenerateScreenshotPath());
            }

            btnNewPage.Enabled = true;
            btnGoto.Enabled = true;
        }
        catch (Exception ex)
        {
            AppendLog("一键运行失败：" + ex);
        }
        finally
        {
            RestoreButtonsAfterRunAll(state);
        }
    }

    private async void btnResetRunAll_Click(object sender, EventArgs e)
    {
        btnRunAll.Enabled = false;
        btnResetRunAll.Enabled = false;
        AppendLog("🔄 重置中…");
        await CleanupAsync();
        AppendLog("🔄 重置完成，开始一键运行…");
        btnRunAll_Click(sender, e);
    }

    private (Control control, bool enabled)[] ProtectButtonsDuringRunAll()
    {
        var tracked = new (Control control, bool enabled)[]
        {
            (btnLaunch, btnLaunch.Enabled),
            (btnWaitDevTools, btnWaitDevTools.Enabled),
            (btnConnect, btnConnect.Enabled),
            (btnNewPage, btnNewPage.Enabled),
            (btnGoto, btnGoto.Enabled),
            (btnCloseAll, btnCloseAll.Enabled),
            (btnRunAll, btnRunAll.Enabled),
            (btnResetRunAll, btnResetRunAll.Enabled),
            (btnStartLogging, btnStartLogging.Enabled),
            (btnStopLogging, btnStopLogging.Enabled),
            (btnSaveSnapshot, btnSaveSnapshot.Enabled)
        };

        btnLaunch.Enabled = false;
        btnWaitDevTools.Enabled = false;
        btnConnect.Enabled = false;
        btnNewPage.Enabled = false;
        btnGoto.Enabled = false;
        btnCloseAll.Enabled = true;
        btnRunAll.Enabled = false;
        btnResetRunAll.Enabled = false;
        btnStartLogging.Enabled = true;
        btnStopLogging.Enabled = false;
        btnSaveSnapshot.Enabled = true;

        return tracked;
    }

    private void RestoreButtonsAfterRunAll((Control control, bool enabled)[] state)
    {
        foreach (var (control, enabled) in state)
        {
            control.Enabled = enabled;
        }
    }

    private void btnStartLogging_Click(object sender, EventArgs e)
    {
        try
        {
            LoggingManager.Start();
            btnStartLogging.Enabled = false;
            btnStopLogging.Enabled = true;
        }
        catch (Exception ex)
        {
            AppendLog("开启日志失败：" + ex);
        }
    }

    private void btnStopLogging_Click(object sender, EventArgs e)
    {
        LoggingManager.Stop();
        btnStartLogging.Enabled = true;
        btnStopLogging.Enabled = false;
    }

    private void btnSaveSnapshot_Click(object sender, EventArgs e)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var directory = Path.Combine(LogsRoot, $"snapshot-{stamp}");
        try
        {
            _ = Playwright.SaveSnapshotAsync(directory, (int)numPort.Value);
        }
        catch (Exception ex)
        {
            AppendLog("保存快照失败：" + ex.Message);
        }
    }

    private void btnExportJsonList_Click(object sender, EventArgs e)
    {
        var path = Path.Combine(LogsRoot, $"json-list-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        _ = Playwright.ExportJsonAsync($"http://127.0.0.1:{(int)numPort.Value}/json/list", path);
    }

    private void btnExportJsonProtocol_Click(object sender, EventArgs e)
    {
        var path = Path.Combine(LogsRoot, $"json-protocol-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        _ = Playwright.ExportJsonAsync($"http://127.0.0.1:{(int)numPort.Value}/json/protocol", path);
    }

    private async void btnRefreshPages_Click(object sender, EventArgs e)
    {
        lstPages.Items.Clear();
        if (Playwright.Context is null)
        {
            return;
        }

        foreach (var page in Playwright.Context.Pages)
        {
            await Task.Yield();
            lstPages.Items.Add(new PageItem(page, await page.TitleAsync()));
        }
    }

    private async void btnOpenNewTab_Click(object sender, EventArgs e)
    {
        if (Playwright.Context is null)
        {
            AppendLog("尚未连接 / 创建 Context。");
            return;
        }

        var page = await Playwright.Context.NewPageAsync();
        if (!string.IsNullOrWhiteSpace(txtNewTabUrl.Text))
        {
            await page.GotoAsync(txtNewTabUrl.Text.Trim());
        }

        Playwright.SelectPage(page);
        AppendLog("新开标签页");
    }

    private void lstPages_SelectedIndexChanged(object sender, EventArgs e)
    {
        Playwright.SelectPage(SelectedPage);
        AppendLog($"切换当前页面为：{SelectedItem?.Name ?? "(unknown)"}");
    }

    private void btnBrowseExe_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择外部浏览器类程序（Chromium 内核）",
            Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtExePath.Text = dialog.FileName;
            AppendLog($"已选择 EXE：{dialog.FileName}");
        }
    }

    private async void btnCloseAll_Click(object sender, EventArgs e)
    {
        await CleanupAsync();
        _processLauncher.Stop();
    }

    private async Task CleanupAsync()
    {
        _waitCancellation?.Cancel();
        if (!_runtime.IsValueCreated)
        {
            return;
        }

        await Playwright.CleanupAsync();
        lstPages.Items.Clear();
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        await CleanupAsync();
        _processLauncher.Dispose();
        base.OnFormClosed(e);
    }

    private string GenerateScreenshotPath()
    {
        Directory.CreateDirectory(ScreenshotsRoot);
        return Path.Combine(ScreenshotsRoot, $"page-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
    }

    private void OnPageAttached(PageItem page)
    {
        lstPages.InvokeSafe(() =>
        {
            lstPages.Items.Add(page);
            if (lstPages.SelectedIndex < 0)
            {
                lstPages.SelectedIndex = 0;
            }
        });
    }

    private void OnPageClosed(IPage page)
    {
        lstPages.InvokeSafe(() =>
        {
            for (var i = lstPages.Items.Count - 1; i >= 0; i--)
            {
                if (lstPages.Items[i] is PageItem item && ReferenceEquals(item.Page, page))
                {
                    lstPages.Items.RemoveAt(i);
                }
            }
        });
    }

    private static bool IsDesignMode()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return true;
        }

        try
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            return string.Equals(processName, "devenv", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(processName, "Blend", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private sealed class RuntimeState
    {
        private RuntimeState(
            string storageRoot,
            string logsRoot,
            string screenshotsRoot,
            string downloadsRoot,
            string userDataRoot,
            string runLogPath,
            LoggingManager loggingManager,
            PlaywrightController playwright)
        {
            StorageRoot = storageRoot;
            LogsRoot = logsRoot;
            ScreenshotsRoot = screenshotsRoot;
            DownloadsRoot = downloadsRoot;
            UserDataRoot = userDataRoot;
            RunLogPath = runLogPath;
            LoggingManager = loggingManager;
            Playwright = playwright;
        }

        public string StorageRoot { get; }

        public string LogsRoot { get; }

        public string ScreenshotsRoot { get; }

        public string DownloadsRoot { get; }

        public string UserDataRoot { get; }

        public string RunLogPath { get; }

        public LoggingManager LoggingManager { get; }

        public PlaywrightController Playwright { get; }

        public static RuntimeState Create(Action<string> appendLog)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrWhiteSpace(desktop) || !Directory.Exists(desktop))
            {
                desktop = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            var storageRoot = Path.Combine(desktop, "PlaywrightRemoteBrowserLauncher");
            Directory.CreateDirectory(storageRoot);

            var logsRoot = Path.Combine(storageRoot, "Logs");
            Directory.CreateDirectory(logsRoot);
            var screenshotsRoot = Path.Combine(storageRoot, "Screenshots");
            Directory.CreateDirectory(screenshotsRoot);
            var downloadsRoot = Path.Combine(storageRoot, "Downloads");
            Directory.CreateDirectory(downloadsRoot);
            var userDataRoot = Path.Combine(storageRoot, "UserData");
            Directory.CreateDirectory(userDataRoot);

            var runLogPath = Path.Combine(logsRoot, $"run-{DateTime.Now:yyyyMMdd}.log");
            var loggingManager = new LoggingManager(logsRoot, appendLog);
            var playwright = new PlaywrightController(loggingManager, appendLog);

            return new RuntimeState(
                storageRoot,
                logsRoot,
                screenshotsRoot,
                downloadsRoot,
                userDataRoot,
                runLogPath,
                loggingManager,
                playwright);
        }
    }
}
