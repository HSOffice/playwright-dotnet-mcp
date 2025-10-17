using System;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
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
                "CefBrowserProfile_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(userDataDir);
        }

        var settings = new CefSettings
        {
            CachePath = Path.Combine(userDataDir!, "cache"),
            PersistSessionCookies = true,
            PersistUserPreferences = true,
            LogFile = Path.Combine(userDataDir!, "cef.log")
        };

        if (!Directory.Exists(settings.CachePath))
        {
            Directory.CreateDirectory(settings.CachePath);
        }

        if (!Cef.IsInitialized)
        {
            Cef.EnableHighDPISupport();
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        ApplicationConfiguration.Initialize();
        try
        {
            Application.Run(new BrowserForm(startUrl));
        }
        finally
        {
            if (Cef.IsInitialized)
            {
                Cef.Shutdown();
            }
        }
    }
}

public class BrowserForm : Form
{
    private readonly string _startUrl;
    private readonly ChromiumWebBrowser _browser;
    private readonly TextBox _addressBar;
    private string? _pendingInitialUrl;

    public BrowserForm(string startUrl)
    {
        _startUrl = startUrl;

        Text = "Mini CEF Browser";
        Width = 1200;
        Height = 800;

        _addressBar = new TextBox
        {
            Dock = DockStyle.Top
        };
        _addressBar.KeyDown += AddressBar_KeyDown;

        _browser = new ChromiumWebBrowser("about:blank")
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(_browser);
        Controls.Add(_addressBar);

        KeyPreview = true;
        KeyDown += BrowserForm_KeyDown;
        Load += BrowserForm_Load;
        FormClosed += BrowserForm_FormClosed;

        _browser.TitleChanged += Browser_TitleChanged;
        _browser.AddressChanged += Browser_AddressChanged;
        _browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
        _browser.ConsoleMessage += Browser_ConsoleMessage;
        _browser.KeyboardHandler = new ShortcutKeyboardHandler();
    }

    private void BrowserForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.L)
        {
            _addressBar.Focus();
            _addressBar.SelectAll();
            e.Handled = true;
        }
    }

    private void BrowserForm_Load(object? sender, EventArgs e)
    {
        NavigateTo(_startUrl);
    }

    private void BrowserForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        try
        {
            _browser.Dispose();
        }
        catch
        {
            // ignored
        }
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
            NavigateTo(targetUri.ToString());
        }
    }

    private void Browser_TitleChanged(object? sender, TitleChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Text = e.Title));
            return;
        }

        Text = e.Title;
    }

    private void Browser_AddressChanged(object? sender, AddressChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => _addressBar.Text = e.Address));
            return;
        }

        _addressBar.Text = e.Address;
    }

    private void Browser_IsBrowserInitializedChanged(object? sender, EventArgs e)
    {
        if (_browser.IsBrowserInitialized && _pendingInitialUrl is { } url)
        {
            _pendingInitialUrl = null;
            _browser.Load(url);
        }
    }

    private void Browser_ConsoleMessage(object? sender, ConsoleMessageEventArgs e)
    {
        Console.WriteLine($"[Console:{e.Level}] {e.Message}");
    }

    private void NavigateTo(string url)
    {
        _addressBar.Text = url;

        if (_browser.IsBrowserInitialized)
        {
            _browser.Load(url);
        }
        else
        {
            _pendingInitialUrl = url;
            _browser.Address = url;
        }
    }

    private static Uri? BuildUriFromInput(string input)
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

internal sealed class ShortcutKeyboardHandler : IKeyboardHandler
{
    public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode,
        int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
    {
        if (type != KeyType.KeyUp)
        {
            return false;
        }

        if ((Keys)windowsKeyCode == Keys.F5)
        {
            browser.Reload();
            return true;
        }

        return false;
    }

    public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode,
        int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
    {
        return false;
    }
}
