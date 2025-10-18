using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private readonly ComboBox cmbProc = new() { Left = 12, Top = 12, Width = 520, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
    private readonly Button btnRefresh = new() { Text = "Refresh", Left = 540, Top = 10, Width = 90, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
    private readonly Button btnDetect = new() { Text = "Detect", Left = 640, Top = 10, Width = 90, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
    private readonly Button btnCopy = new() { Text = "Copy JSON", Left = 740, Top = 10, Width = 90, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
    private readonly Button btnSave = new() { Text = "Save JSON", Left = 836, Top = 10, Width = 90, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right };
    private readonly CheckBox chkPretty = new() { Text = "Pretty JSON", Left = 12, Top = 46, AutoSize = true, Checked = true };

    private readonly TextBox txtJson = new()
    {
        Left = 12,
        Top = 72,
        Width = 914,
        Height = 380,
        Multiline = true,
        ScrollBars = ScrollBars.Both,
        ReadOnly = true,
        WordWrap = false,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
    };

    private readonly Label lblAnalysis = new() { Text = "Analysis Result:", Left = 12, Top = 460, AutoSize = true };
    private readonly TextBox txtAnalysis = new()
    {
        Left = 12,
        Top = 480,
        Width = 914,
        Height = 160,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        ReadOnly = true,
        Font = new Font("Consolas", 10),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
    };

    private ProbeResult? _lastResult;

    public MainForm()
    {
        Text = "Detect Browser Runtime - TARGET Process (WinForms)";
        Width = 960; Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.AddRange(new Control[] { cmbProc, btnRefresh, btnDetect, btnCopy, btnSave, chkPretty, txtJson, lblAnalysis, txtAnalysis });

        btnRefresh.Click += (_, __) => RefreshProcessList();
        btnDetect.Click += (_, __) => DoDetect();
        btnCopy.Click += (_, __) => { if (!string.IsNullOrEmpty(txtJson.Text)) Clipboard.SetText(txtJson.Text); };
        btnSave.Click += (_, __) => SaveJson();
        chkPretty.CheckedChanged += (_, __) => RenderJson();

        RefreshProcessList();
    }

    private void RefreshProcessList()
    {
        cmbProc.Items.Clear();
        try
        {
            var procs = Process.GetProcesses()
                .OrderBy(p => p.ProcessName)
                .Select(p =>
                {
                    string line;
                    try { line = $"{p.ProcessName} (PID:{p.Id}) - {p.MainModule?.FileName}"; }
                    catch { line = $"{p.ProcessName} (PID:{p.Id})"; }
                    return line;
                })
                .ToArray();

            cmbProc.Items.AddRange(procs);
            if (cmbProc.Items.Count > 0) cmbProc.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Enumerate processes failed: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private int? SelectedPid()
    {
        if (cmbProc.SelectedItem == null) return null;
        var s = cmbProc.SelectedItem.ToString() ?? "";
        var idx = s.IndexOf("PID:", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var digits = new string(s.Skip(idx + 4).TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var pid) ? pid : null;
    }

    private void DoDetect()
    {
        txtJson.Clear();
        txtAnalysis.Clear();
        _lastResult = null;

        int? pid = SelectedPid();
        if (pid == null) { txtAnalysis.Text = "No process selected."; return; }

        try
        {
            _lastResult = TargetProbe.DetectProcess(pid.Value);
            RenderJson();
            txtAnalysis.Text = TargetProbe.AnalyzeResult(_lastResult);
        }
        catch (Exception ex)
        {
            txtAnalysis.Text = "Error: " + ex;
        }
    }

    private void RenderJson()
    {
        if (_lastResult == null) { txtJson.Clear(); return; }
        var opts = new JsonSerializerOptions { WriteIndented = chkPretty.Checked };
        txtJson.Text = JsonSerializer.Serialize(_lastResult, opts);
    }

    private void SaveJson()
    {
        if (_lastResult == null && string.IsNullOrEmpty(txtJson.Text))
        { MessageBox.Show(this, "No content.", Text); return; }

        using var sfd = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = $"runtime_probe_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            var opts = new JsonSerializerOptions { WriteIndented = chkPretty.Checked };
            var json = _lastResult != null ? JsonSerializer.Serialize(_lastResult, opts) : txtJson.Text;
            File.WriteAllText(sfd.FileName, json, new UTF8Encoding(false));
            MessageBox.Show(this, "Saved: " + sfd.FileName, Text);
        }
    }
}

internal static class TargetProbe
{
    public static ProbeResult DetectProcess(int pid)
    {
        var procInfo = GetProcessInfo(pid);
        var modules = EnumModulesViaToolHelp(pid);
        string mainExe = modules.FirstOrDefault()?.Path ?? SafeMainModulePath(pid);

        var resources = new Dictionary<string, bool>
        {
            ["\\icudtl.dat"] = FileExistsNear(mainExe, "\\icudtl.dat"),
            ["\\resources.pak"] = FileExistsNear(mainExe, "\\resources.pak"),
            ["\\cef.pak"] = FileExistsNear(mainExe, "\\cef.pak"),
            ["\\resources\\app.asar"] = FileExistsNear(mainExe, "\\resources\\app.asar"),
            ["\\resources\\electron.asar"] = FileExistsNear(mainExe, "\\resources\\electron.asar"),
        };

        bool isCEF = modules.Any(m => m.Name.Equals("libcef.dll", StringComparison.OrdinalIgnoreCase));
        bool isWebView2 = modules.Any(m => m.Name.Contains("webview2", StringComparison.OrdinalIgnoreCase));
        bool isElectron = modules.Any(m => m.Name.Contains("electron", StringComparison.OrdinalIgnoreCase)) ||
                           modules.Any(m => m.Name.Equals("node.dll", StringComparison.OrdinalIgnoreCase));
        bool isChromium = modules.Any(m => m.Name.Equals("chrome.dll", StringComparison.OrdinalIgnoreCase));

        return new ProbeResult
        {
            pid = pid,
            process = procInfo.ProcessName,
            path = mainExe,
            commandLine = procInfo.CommandLine ?? "",
            foundModules = modules,
            resources = resources,
            heuristic = new Heuristic
            {
                isCEF = isCEF,
                isWebView2 = isWebView2,
                isElectron = isElectron,
                isChromium = isChromium
            }
        };
    }

    public static string AnalyzeResult(ProbeResult result)
    {
        var h = result.heuristic;
        var evidence = new List<string>();
        var conclusion = "No browser runtime detected.";
        var confidence = 0;

        if (h.isElectron)
        {
            confidence += 60;
            evidence.Add("Found modules: electron.exe / node.dll / app.asar");
            conclusion = "Detected: Electron (基于 Node.js + Chromium 内核)";
        }
        else if (h.isCEF)
        {
            confidence += 70;
            evidence.Add("Found libcef.dll and cef.pak");
            conclusion = "Detected: CEF (Chromium Embedded Framework)";
        }
        else if (h.isWebView2)
        {
            confidence += 80;
            evidence.Add("Found WebView2Loader.dll or msedgewebview2.exe");
            conclusion = "Detected: WebView2 (Microsoft Edge runtime)";
        }
        else if (h.isChromium)
        {
            confidence += 65;
            evidence.Add("Found chrome.dll or content_shell.dll");
            conclusion = "Detected: Chromium Core";
        }
        else
        {
            confidence = 10;
            evidence.Add("No typical runtime modules found");
        }

        // 构造分析报告
        var sb = new StringBuilder();
        sb.AppendLine($"📊 结论：{conclusion}");
        sb.AppendLine($"🔍 置信度：{confidence}%");
        sb.AppendLine();
        sb.AppendLine("🧩 关键证据：");
        foreach (var ev in evidence)
            sb.AppendLine(" - " + ev);
        sb.AppendLine();
        sb.AppendLine("📁 主程序路径：");
        sb.AppendLine(" " + result.path);
        sb.AppendLine();
        sb.AppendLine("💬 命令行参数：");
        sb.AppendLine(" " + (string.IsNullOrWhiteSpace(result.commandLine) ? "(空)" : result.commandLine));
        sb.AppendLine();
        sb.AppendLine("📦 模块示例：");
        foreach (var m in result.foundModules.Take(5))
            sb.AppendLine($" - {m.Name}");

        return sb.ToString();
    }

    // ====== 基础探测部分 ======
    private static (string ProcessName, string? CommandLine) GetProcessInfo(int pid)
    {
        string name = "";
        try { name = Process.GetProcessById(pid).ProcessName; } catch { }

        string? cmd = null;
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}");
            foreach (ManagementObject mo in searcher.Get())
            {
                cmd = mo["CommandLine"] as string;
                break;
            }
        }
        catch { }

        return (name, cmd);
    }

    private static List<ModEntry> EnumModulesViaToolHelp(int pid)
    {
        var mods = new List<ModEntry>();
        IntPtr hSnap = IntPtr.Zero;
        try
        {
            hSnap = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, (uint)pid);
            if (hSnap == INVALID_HANDLE_VALUE) return mods;

            MODULEENTRY32W me = new MODULEENTRY32W
            {
                dwSize = (uint)Marshal.SizeOf<MODULEENTRY32W>(),
                szModule = new char[256],
                szExePath = new char[260]
            };

            if (Module32FirstW(hSnap, ref me))
            {
                do
                {
                    string name = PtrToString(me.szModule);
                    string path = PtrToString(me.szExePath);
                    string ver = SafeFileVersion(path);
                    mods.Add(new ModEntry { Name = name, Path = path, Version = ver });

                    me.dwSize = (uint)Marshal.SizeOf<MODULEENTRY32W>();
                    Array.Clear(me.szModule, 0, me.szModule.Length);
                    Array.Clear(me.szExePath, 0, me.szExePath.Length);
                }
                while (Module32NextW(hSnap, ref me));
            }
        }
        catch { }
        finally
        {
            if (hSnap != IntPtr.Zero && hSnap != INVALID_HANDLE_VALUE) CloseHandle(hSnap);
        }
        return mods;
    }

    private static string SafeMainModulePath(int pid)
    {
        try { return Process.GetProcessById(pid).MainModule?.FileName ?? ""; } catch { return ""; }
    }

    private static bool FileExistsNear(string basePath, string rel)
    {
        try
        {
            var dir = Path.GetDirectoryName(basePath);
            if (string.IsNullOrEmpty(dir)) return false;
            return File.Exists(Path.Combine(dir, rel.TrimStart('\\', '/')));
        }
        catch { return false; }
    }

    private static string PtrToString(char[] buffer)
    {
        int len = Array.IndexOf(buffer, '\0');
        if (len < 0) len = buffer.Length;
        return new string(buffer, 0, len);
    }

    private static string SafeFileVersion(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "";
        try
        {
            var f = FileVersionInfo.GetVersionInfo(path!);
            var ver = $"{f.FileMajorPart}.{f.FileMinorPart}.{f.FileBuildPart}.{f.FilePrivatePart}";
            var prod = f.ProductName ?? "";
            var comp = f.CompanyName ?? "";
            var desc = f.FileDescription ?? "";

            var parts = new List<string> { $"FileVersion={ver}" };
            if (!string.IsNullOrWhiteSpace(prod)) parts.Add($"ProductName={prod}");
            if (!string.IsNullOrWhiteSpace(comp)) parts.Add($"Company={comp}");
            if (!string.IsNullOrWhiteSpace(desc)) parts.Add($"Description={desc}");
            return string.Join("; ", parts);
        }
        catch { return ""; }
    }

    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    [Flags]
    private enum SnapshotFlags : uint { Module = 0x8, Module32 = 0x10 }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MODULEENTRY32W
    {
        public uint dwSize, th32ModuleID, th32ProcessID, GlblcntUsage, ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public char[] szModule;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)] public char[] szExePath;
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern bool Module32FirstW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern bool Module32NextW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool CloseHandle(IntPtr hObject);
}

// ========== 数据结构 ==========
internal sealed class ProbeResult
{
    public int pid { get; set; }
    public string process { get; set; } = "";
    public string path { get; set; } = "";
    public string commandLine { get; set; } = "";
    public List<ModEntry> foundModules { get; set; } = new();
    public Dictionary<string, bool> resources { get; set; } = new();
    public Heuristic heuristic { get; set; } = new();
}

internal sealed class ModEntry
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Version { get; set; } = "";
}

internal sealed class Heuristic
{
    public bool isCEF { get; set; }
    public bool isWebView2 { get; set; }
    public bool isElectron { get; set; }
    public bool isChromium { get; set; }
}
