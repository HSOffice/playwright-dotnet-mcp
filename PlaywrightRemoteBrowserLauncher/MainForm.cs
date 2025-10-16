using PlaywrightRemoteBrowserLauncher.Extensions;
using PlaywrightRemoteBrowserLauncher.Models;
using PlaywrightRemoteBrowserLauncher.Services;
using Microsoft.Playwright;

namespace PlaywrightRemoteBrowserLauncher;

public partial class MainForm : Form
{
    private readonly string _storageRoot;
    private readonly string _logsRoot;
    private readonly string _screenshotsRoot;
    private readonly string _downloadsRoot;
    private readonly string _userDataRoot;
    private readonly string _runLogPath;
    private readonly LoggingManager _loggingManager;
    private readonly BrowserProcessLauncher _processLauncher = new();
    private readonly PlaywrightController _playwright;

    private CancellationTokenSource? _waitCancellation;

    public MainForm()
    {
        InitializeComponent();

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktop) || !Directory.Exists(desktop))
        {
            desktop = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        _storageRoot = Path.Combine(desktop, "PlaywrightRemoteBrowserLauncher");
        Directory.CreateDirectory(_storageRoot);

        _logsRoot = Path.Combine(_storageRoot, "Logs");
        Directory.CreateDirectory(_logsRoot);
        _screenshotsRoot = Path.Combine(_storageRoot, "Screenshots");
        Directory.CreateDirectory(_screenshotsRoot);
        _downloadsRoot = Path.Combine(_storageRoot, "Downloads");
        Directory.CreateDirectory(_downloadsRoot);
        _userDataRoot = Path.Combine(_storageRoot, "UserData");
        Directory.CreateDirectory(_userDataRoot);

        _runLogPath = Path.Combine(_logsRoot, $"run-{DateTime.Now:yyyyMMdd}.log");
        _loggingManager = new LoggingManager(_logsRoot, AppendLog);
        _playwright = new PlaywrightController(_loggingManager, AppendLog);
        _playwright.PageAttached += page => lstPages.InvokeSafe(() =>
        {
            lstPages.Items.Add(page);
            if (lstPages.SelectedIndex < 0)
            {
                lstPages.SelectedIndex = 0;
            }
        });
        _playwright.PageClosed += page => lstPages.InvokeSafe(() =>
        {
            for (var i = lstPages.Items.Count - 1; i >= 0; i--)
            {
                if (lstPages.Items[i] is PageItem item && ReferenceEquals(item.Page, page))
                {
                    lstPages.Items.RemoveAt(i);
                }
            }
        });

        txtExePath.Text = @"D:\\00-培训材料\\MCP\\MyMcpHost\\McpServer\\WinFormsApp\\bin\\Debug\\net8.0-windows\\WinFormsApp.exe";
        numPort.Value = 9222;
        txtStartUrl.Text = "https://example.com";
    }

    private PageItem? SelectedItem => lstPages.SelectedItem as PageItem;

    private IPage? SelectedPage => SelectedItem?.Page ?? _playwright.CurrentPage;

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        txtLog.InvokeSafe(() =>
        {
            txtLog.AppendText(line + Environment.NewLine);
        });

        try
        {
            File.AppendAllText(_runLogPath, line + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }

    private async void btnLaunch_Click(object sender, EventArgs e)
    {
        Directory.CreateDirectory(_userDataRoot);
        var started = _processLauncher.Start(
            txtExePath.Text.Trim(),
            (int)numPort.Value,
            _userDataRoot,
            txtProxy.Text.Trim(),
            AppendLog);

        if (started)
        {
            btnWaitDevTools.Enabled = true;
            btnCloseAll.Enabled = true;
        }
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
            await _playwright.ConnectAsync((int)numPort.Value);
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
            var harPath = Path.Combine(_logsRoot, $"har-{DateTime.Now:yyyyMMdd-HHmmss}.har");
            var config = new ContextConfiguration
            {
                IgnoreHttpsErrors = chkIgnoreTls.Checked,
                InitScript = chkInitScript.Checked ? txtInitScript.Text : null,
                ExposeDotnet = chkExposeDotnet.Checked,
                ExposedFunctionName = string.IsNullOrWhiteSpace(txtExposeName.Text) ? "dotnetPing" : txtExposeName.Text.Trim(),
                RecordHar = false,
                RecordHarPath = harPath
            };

            await _playwright.EnsureContextAsync(config);
            Directory.CreateDirectory(_downloadsRoot);
            await _playwright.CreatePageAsync(_downloadsRoot);
            btnGoto.Enabled = true;
        }
        catch (Exception ex)
        {
            AppendLog("创建页面失败：" + ex);
        }
    }

    private async void btnGoto_Click(object sender, EventArgs e)
    {
        var page = SelectedPage;
        if (page is null)
        {
            AppendLog("尚未创建 Page。");
            return;
        }

        var url = string.IsNullOrWhiteSpace(txtStartUrl.Text) ? "https://example.com" : txtStartUrl.Text.Trim();
        var success = await _playwright.NavigateAsync(page, url, (int)numRetryCount.Value, (int)numRetryDelayMs.Value, chkPostNavScript.Checked ? txtPostNavScript.Text : null);
        if (success && chkAutoScreenshot.Checked)
        {
            await _playwright.CaptureScreenshotAsync(GenerateScreenshotPath());
        }
    }

    private async void btnRunAll_Click(object sender, EventArgs e)
    {
        var state = ProtectButtonsDuringRunAll();
        try
        {
            Directory.CreateDirectory(_userDataRoot);
            if (!_processLauncher.Start(txtExePath.Text.Trim(), (int)numPort.Value, _userDataRoot, txtProxy.Text.Trim(), AppendLog))
            {
                return;
            }

            btnWaitDevTools_Click(sender, e);
            await _playwright.ConnectAsync((int)numPort.Value);
            var config = new ContextConfiguration
            {
                IgnoreHttpsErrors = chkIgnoreTls.Checked,
                InitScript = chkInitScript.Checked ? txtInitScript.Text : null,
                ExposeDotnet = chkExposeDotnet.Checked,
                ExposedFunctionName = string.IsNullOrWhiteSpace(txtExposeName.Text) ? "dotnetPing" : txtExposeName.Text.Trim(),
                RecordHar = false
            };
            await _playwright.EnsureContextAsync(config);
            Directory.CreateDirectory(_downloadsRoot);
            var page = await _playwright.CreatePageAsync(_downloadsRoot);
            var url = string.IsNullOrWhiteSpace(txtStartUrl.Text) ? "https://example.com" : txtStartUrl.Text.Trim();
            if (await _playwright.NavigateAsync(page, url, (int)numRetryCount.Value, (int)numRetryDelayMs.Value, chkPostNavScript.Checked ? txtPostNavScript.Text : null) && chkAutoScreenshot.Checked)
            {
                await _playwright.CaptureScreenshotAsync(GenerateScreenshotPath());
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
            _loggingManager.Start();
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
        _loggingManager.Stop();
        btnStartLogging.Enabled = true;
        btnStopLogging.Enabled = false;
    }

    private void btnSaveSnapshot_Click(object sender, EventArgs e)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var directory = Path.Combine(_logsRoot, $"snapshot-{stamp}");
        try
        {
            _ = _playwright.SaveSnapshotAsync(directory, (int)numPort.Value);
        }
        catch (Exception ex)
        {
            AppendLog("保存快照失败：" + ex.Message);
        }
    }

    private void btnExportJsonList_Click(object sender, EventArgs e)
    {
        var path = Path.Combine(_logsRoot, $"json-list-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        _ = _playwright.ExportJsonAsync($"http://127.0.0.1:{(int)numPort.Value}/json/list", path);
    }

    private void btnExportJsonProtocol_Click(object sender, EventArgs e)
    {
        var path = Path.Combine(_logsRoot, $"json-protocol-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        _ = _playwright.ExportJsonAsync($"http://127.0.0.1:{(int)numPort.Value}/json/protocol", path);
    }

    private async void btnRefreshPages_Click(object sender, EventArgs e)
    {
        lstPages.Items.Clear();
        if (_playwright.Context is null)
        {
            return;
        }

        foreach (var page in _playwright.Context.Pages)
        {
            await Task.Yield();
            lstPages.Items.Add(new PageItem(page, await page.TitleAsync()));
        }
    }

    private async void btnOpenNewTab_Click(object sender, EventArgs e)
    {
        if (_playwright.Context is null)
        {
            AppendLog("尚未连接 / 创建 Context。");
            return;
        }

        var page = await _playwright.Context.NewPageAsync();
        if (!string.IsNullOrWhiteSpace(txtNewTabUrl.Text))
        {
            await page.GotoAsync(txtNewTabUrl.Text.Trim());
        }

        _playwright.SelectPage(page);
        AppendLog("新开标签页");
    }

    private void lstPages_SelectedIndexChanged(object sender, EventArgs e)
    {
        _playwright.SelectPage(SelectedPage);
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
        await _playwright.CleanupAsync();
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
        Directory.CreateDirectory(_screenshotsRoot);
        return Path.Combine(_screenshotsRoot, $"page-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
    }
}
