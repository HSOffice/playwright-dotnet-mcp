using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Ĭ�ϲ���
        int debugPort = 9222;
        string? userDataDir = null;
        string startUrl = "https://example.com";

        // ���ײ���������--remote-debugging-port=XXXX  --user-data-dir="PATH"  --url="https://..."
        foreach (var arg in args)
        {
            if (arg.StartsWith("--remote-debugging-port=", StringComparison.OrdinalIgnoreCase))
            {
                var s = arg.Split('=', 2)[1].Trim('"');
                if (int.TryParse(s, out var p)) debugPort = p;
            }
            else if (arg.StartsWith("--user-data-dir=", StringComparison.OrdinalIgnoreCase))
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
        Application.Run(new BrowserForm(debugPort, userDataDir!, startUrl));
    }
}

public class BrowserForm : Form
{
    private readonly int _debugPort;
    private readonly string _userDataDir;
    private readonly string _startUrl;
    private readonly WebView2 _webView;

    public BrowserForm(int debugPort, string userDataDir, string startUrl)
    {
        _debugPort = debugPort;
        _userDataDir = userDataDir;
        _startUrl = startUrl;

        Text = $"Mini WebView2 Browser - DevTools:{_debugPort}";
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
            // �ؼ����� --remote-debugging-port ע�뵽 WebView2 �������������
            var additionalArgs = new StringBuilder();
            additionalArgs.Append($"--remote-debugging-port={_debugPort}");

            // ��Ҳ����������׷������ Chromium ������������ð�ȫ���Եȣ����裩��
            // additionalArgs.Append(" --disable-web-security");

            var envOptions = new CoreWebView2EnvironmentOptions(additionalArgs.ToString());
            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: _userDataDir,
                options: envOptions);

            await _webView.EnsureCoreWebView2Async(env);

            // ��������ʼ��ַ
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.Source = new Uri(_startUrl);

            // �� UI���� F5 ˢ�¡�Ctrl+L �򿪵�ַ�����
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
                        "����Ҫ��ת�� URL��", "������", _webView.Source?.ToString() ?? "https://");
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        try { _webView.Source = new Uri(input); } catch { }
                    }
                    ev.Handled = true;
                }
            };

            // ���ڱ�����ʾҳ�����
            _webView.CoreWebView2.DocumentTitleChanged += (s, ev) =>
            {
                Text = $"{_webView.CoreWebView2.DocumentTitle} - DevTools:{_debugPort}";
            };

            // ����̨��Ϣ�򵥴�ӡ
            _webView.CoreWebView2.WebMessageReceived += (s, ev) =>
            {
                Console.WriteLine($"[WebMessage] {ev.TryGetWebMessageAsString()}");
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"��ʼ�� WebView2 ʧ�ܣ�\r\n{ex}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void BrowserForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        try { _webView?.Dispose(); } catch { }
    }
}
