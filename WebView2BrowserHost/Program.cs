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
    private readonly TextBox _addressBar;

    public BrowserForm(string userDataDir, string startUrl)
    {
        _userDataDir = userDataDir;
        _startUrl = startUrl;

        Text = "Mini WebView2 Browser";
        Width = 1200;
        Height = 800;

        _addressBar = new TextBox
        {
            Dock = DockStyle.Top
        };
        _addressBar.KeyDown += AddressBar_KeyDown;

        _webView = new WebView2
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_webView);
        Controls.Add(_addressBar);

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

            // Handle UI shortcuts: F5 to refresh
            _webView.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.F5)
                {
                    _webView.Reload();
                    ev.Handled = true;
                }
            };

            // Update window title with the page title
            _webView.CoreWebView2.DocumentTitleChanged += (s, ev) =>
            {
                Text = _webView.CoreWebView2.DocumentTitle;
            };

            _webView.CoreWebView2.SourceChanged += (s, ev) =>
            {
                var currentUri = _webView.Source;
                if (currentUri != null)
                {
                    _addressBar.Text = currentUri.ToString();
                }
                else
                {
                    _addressBar.Text = string.Empty;
                }
            };

            // Log web messages to the console
            _webView.CoreWebView2.WebMessageReceived += (s, ev) =>
            {
                Console.WriteLine($"[WebMessage] {ev.TryGetWebMessageAsString()}");
            };

            _webView.Source = new Uri(_startUrl);
            _addressBar.Text = _startUrl;
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

    private void AddressBar_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;

        var input = _addressBar.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        var targetUri = BuildUriFromInput(input);
        if (targetUri != null)
        {
            _webView.Source = targetUri;
        }
    }

    private static Uri BuildUriFromInput(string input)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return absoluteUri;
        }

        if (!input.Contains(' '))
        {
            var withHttps = $"https://{input}";
            if (Uri.TryCreate(withHttps, UriKind.Absolute, out absoluteUri) &&
                (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
            {
                return absoluteUri;
            }
        }

        var searchQuery = Uri.EscapeDataString(input);
        return new Uri($"https://www.bing.com/search?q={searchQuery}");
    }
}
