using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExternalBrowserWinForms.Core.Models;
using ExternalBrowserWinForms.Core.Services;

namespace ExternalBrowserWinForms.App.Forms;

public partial class MainForm : Form
{
    private readonly IBrowserLaunchService _browserLaunchService;

    public MainForm(IBrowserLaunchService browserLaunchService)
    {
        InitializeComponent();
        _browserLaunchService = browserLaunchService;

        chkUseDefaultBrowser.CheckedChanged += (_, _) => UpdateBrowserControlsState();
        UpdateBrowserControlsState();
    }

    private async void BtnLaunchClick(object? sender, EventArgs e)
    {
        await LaunchBrowserAsync();
    }

    private async Task LaunchBrowserAsync()
    {
        ToggleControls(false);
        lblStatus.Text = "正在启动浏览器...";

        try
        {
            var request = BuildRequest();
            var result = await _browserLaunchService.LaunchAsync(request);
            lblStatus.Text = result.Message;
        }
        catch (Exception ex)
        {
            lblStatus.Text = ex.Message;
        }
        finally
        {
            ToggleControls(true);
        }
    }

    private BrowserLaunchRequest BuildRequest()
    {
        var url = new Uri(txtUrl.Text, UriKind.RelativeOrAbsolute);
        if (!url.IsAbsoluteUri)
        {
            url = new UriBuilder("https", txtUrl.Text).Uri;
        }

        return new BrowserLaunchRequest(
            url,
            chkUseDefaultBrowser.Checked,
            string.IsNullOrWhiteSpace(txtBrowserPath.Text) ? null : txtBrowserPath.Text,
            string.IsNullOrWhiteSpace(txtArguments.Text) ? null : txtArguments.Text);
    }

    private void UpdateBrowserControlsState()
    {
        var useDefault = chkUseDefaultBrowser.Checked;
        txtBrowserPath.Enabled = !useDefault;
        btnBrowse.Enabled = !useDefault;
        txtArguments.Enabled = !useDefault;
    }

    private void ToggleControls(bool enabled)
    {
        txtUrl.Enabled = enabled;
        chkUseDefaultBrowser.Enabled = enabled;
        UpdateBrowserControlsState();
        btnLaunch.Enabled = enabled;
    }

    private void BtnBrowseClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择浏览器可执行文件",
            Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtBrowserPath.Text = dialog.FileName;
        }
    }
}
