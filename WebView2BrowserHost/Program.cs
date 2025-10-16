using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Defaults
        string? userDataDir = null;
        string startUrl = "https://example.com";

        foreach (var arg in args)
        {
            if (arg.StartsWith("--user-data-dir=", StringComparison.OrdinalIgnoreCase))
            {
                userDataDir = arg.Split('=', 2)[1].Trim('"');
            }
            else if (arg.StartsWith("--url=", StringComparison.OrdinalIgnoreCase))
            {
                startUrl = arg.Split('=', 2)[1].Trim('"');
            }
        }

        if (string.IsNullOrWhiteSpace(userDataDir))
        {
            userDataDir = Path.Combine(Path.GetTempPath(),
                "WebView2Profile_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(userDataDir);
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new BrowserForm(userDataDir!, startUrl));
    }
}

public class BrowserForm : Form
{
    private readonly string _userDataDir;
    private readonly string _startUrl;
    private readonly WebView2 _webView;

    public BrowserForm(string userDataDir, string startUrl)
    {
        _userDataDir = userDataDir;
        _startUrl = startUrl;

        Text = "Mini WebView2 Browser";
        Width = 1200;
        Height = 800;

        _webView = new WebView2
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_webView);

        Load += BrowserForm_Load;
        FormClosed += BrowserForm_FormClosed;
    }

    private async void BrowserForm_Load(object? sender, EventArgs e)
    {
        try
        {

            // Create the WebView2 environment
            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: _userDataDir,
                options: null);

            await _webView.EnsureCoreWebView2Async(env);

            // Initialize the starting URL
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.Source = new Uri(_startUrl);

            // Handle UI shortcuts: F5 to refresh, Ctrl+L to open the address bar
            _webView.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.F5)
                {
                    _webView.Reload();
                    ev.Handled = true;
                }
                if (ev.Control && ev.KeyCode == Keys.L)
                {
                    var input = Microsoft.VisualBasic.Interaction.InputBox(
                        "Enter the URL", "", _webView.Source?.ToString() ?? "https://");
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        try { _webView.Source = new Uri(input); } catch { }
                    }
                    ev.Handled = true;
                }
            };

            // Update window title with the page title
            _webView.CoreWebView2.DocumentTitleChanged += (s, ev) =>
            {
                Text = _webView.CoreWebView2.DocumentTitle;
            };

            // Log web messages to the console
            _webView.CoreWebView2.WebMessageReceived += (s, ev) =>
            {
                Console.WriteLine($"[WebMessage] {ev.TryGetWebMessageAsString()}");
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to initialize WebView2\r\n{ex}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void BrowserForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        try { _webView?.Dispose(); } catch { }
    }
}
