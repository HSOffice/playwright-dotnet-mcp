using System.Drawing;
using System.Windows.Forms;

namespace ExternalBrowserWinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TextBox txtExePath;
    private System.Windows.Forms.Button btnBrowseExe;
    private System.Windows.Forms.NumericUpDown numPort;
    private System.Windows.Forms.TextBox txtStartUrl;

    private System.Windows.Forms.Button btnLaunch;
    private System.Windows.Forms.Button btnWaitDevTools;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnNewPage;
    private System.Windows.Forms.Button btnGoto;
    private System.Windows.Forms.Button btnCloseAll;

    private System.Windows.Forms.TextBox txtLog;

    private System.Windows.Forms.Label lblExe;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.Label lblUrl;

    private System.Windows.Forms.Button btnRunAll;
    private System.Windows.Forms.Button btnResetRunAll;
    private System.Windows.Forms.CheckBox chkAutoScreenshot;

    private System.Windows.Forms.Button btnStartLogging;
    private System.Windows.Forms.Button btnStopLogging;
    private System.Windows.Forms.Button btnSaveSnapshot;

    private System.Windows.Forms.ListBox lstPages;
    private System.Windows.Forms.Label lblPages;

    private System.Windows.Forms.Label lblProxy;
    private System.Windows.Forms.TextBox txtProxy;
    private System.Windows.Forms.CheckBox chkIgnoreTls;

    private System.Windows.Forms.CheckBox chkInitScript;
    private System.Windows.Forms.TextBox txtInitScript;

    private System.Windows.Forms.CheckBox chkPostNavScript;
    private System.Windows.Forms.TextBox txtPostNavScript;

    private System.Windows.Forms.CheckBox chkExposeDotnet;
    private System.Windows.Forms.TextBox txtExposeName;

    private System.Windows.Forms.Label lblRetry;
    private System.Windows.Forms.NumericUpDown numRetryCount;
    private System.Windows.Forms.NumericUpDown numRetryDelayMs;

    private System.Windows.Forms.Button btnExportJsonList;
    private System.Windows.Forms.Button btnExportJsonProtocol;

    private System.Windows.Forms.Button btnRefreshPages;
    private System.Windows.Forms.TextBox txtNewTabUrl;
    private System.Windows.Forms.Button btnOpenNewTab;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        txtExePath = new System.Windows.Forms.TextBox();
        btnBrowseExe = new System.Windows.Forms.Button();
        numPort = new System.Windows.Forms.NumericUpDown();
        txtStartUrl = new System.Windows.Forms.TextBox();

        btnLaunch = new System.Windows.Forms.Button();
        btnWaitDevTools = new System.Windows.Forms.Button();
        btnConnect = new System.Windows.Forms.Button();
        btnNewPage = new System.Windows.Forms.Button();
        btnGoto = new System.Windows.Forms.Button();
        btnCloseAll = new System.Windows.Forms.Button();

        txtLog = new System.Windows.Forms.TextBox();

        lblExe = new System.Windows.Forms.Label();
        lblPort = new System.Windows.Forms.Label();
        lblUrl = new System.Windows.Forms.Label();

        btnRunAll = new System.Windows.Forms.Button();
        btnResetRunAll = new System.Windows.Forms.Button();
        chkAutoScreenshot = new System.Windows.Forms.CheckBox();

        btnStartLogging = new System.Windows.Forms.Button();
        btnStopLogging = new System.Windows.Forms.Button();
        btnSaveSnapshot = new System.Windows.Forms.Button();

        lstPages = new System.Windows.Forms.ListBox();
        lblPages = new System.Windows.Forms.Label();

        lblProxy = new System.Windows.Forms.Label();
        txtProxy = new System.Windows.Forms.TextBox();
        chkIgnoreTls = new System.Windows.Forms.CheckBox();

        chkInitScript = new System.Windows.Forms.CheckBox();
        txtInitScript = new System.Windows.Forms.TextBox();

        chkPostNavScript = new System.Windows.Forms.CheckBox();
        txtPostNavScript = new System.Windows.Forms.TextBox();

        chkExposeDotnet = new System.Windows.Forms.CheckBox();
        txtExposeName = new System.Windows.Forms.TextBox();

        lblRetry = new System.Windows.Forms.Label();
        numRetryCount = new System.Windows.Forms.NumericUpDown();
        numRetryDelayMs = new System.Windows.Forms.NumericUpDown();

        btnExportJsonList = new System.Windows.Forms.Button();
        btnExportJsonProtocol = new System.Windows.Forms.Button();

        btnRefreshPages = new System.Windows.Forms.Button();
        txtNewTabUrl = new System.Windows.Forms.TextBox();
        btnOpenNewTab = new System.Windows.Forms.Button();

        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).BeginInit();
        SuspendLayout();

        // Labels
        lblExe.AutoSize = true;
        lblExe.Location = new System.Drawing.Point(12, 15);
        lblExe.Text = "可执行文件：";

        lblPort.AutoSize = true;
        lblPort.Location = new System.Drawing.Point(12, 50);
        lblPort.Text = "调试端口：";

        lblUrl.AutoSize = true;
        lblUrl.Location = new System.Drawing.Point(12, 85);
        lblUrl.Text = "起始 URL：";

        // txtExePath
        txtExePath.Location = new System.Drawing.Point(100, 12);
        txtExePath.Size = new System.Drawing.Size(600, 23);

        // btnBrowseExe
        btnBrowseExe.Location = new System.Drawing.Point(710, 11);
        btnBrowseExe.Size = new System.Drawing.Size(75, 25);
        btnBrowseExe.Text = "浏览…";
        btnBrowseExe.Click += btnBrowseExe_Click;

        // numPort
        numPort.Location = new System.Drawing.Point(100, 47);
        numPort.Minimum = 1;
        numPort.Maximum = 65535;
        numPort.Value = 9222;
        numPort.Size = new System.Drawing.Size(120, 23);

        // txtStartUrl
        txtStartUrl.Location = new System.Drawing.Point(100, 82);
        txtStartUrl.Size = new System.Drawing.Size(685, 23);

        // Buttons row 1 (step-by-step)
        btnLaunch.Location = new System.Drawing.Point(12, 155);
        btnLaunch.Size = new System.Drawing.Size(110, 30);
        btnLaunch.Text = "1) 启动进程";
        btnLaunch.Click += btnLaunch_Click;

        btnWaitDevTools.Location = new System.Drawing.Point(132, 155);
        btnWaitDevTools.Size = new System.Drawing.Size(140, 30);
        btnWaitDevTools.Text = "2) 等待 DevTools";
        btnWaitDevTools.Enabled = false;
        btnWaitDevTools.Click += btnWaitDevTools_Click;

        btnConnect.Location = new System.Drawing.Point(280, 155);
        btnConnect.Size = new System.Drawing.Size(140, 30);
        btnConnect.Text = "3) 连接 Playwright";
        btnConnect.Enabled = false;
        btnConnect.Click += btnConnect_Click;

        btnNewPage.Location = new System.Drawing.Point(428, 155);
        btnNewPage.Size = new System.Drawing.Size(130, 30);
        btnNewPage.Text = "4) 新建 Page";
        btnNewPage.Enabled = false;
        btnNewPage.Click += btnNewPage_Click;

        btnGoto.Location = new System.Drawing.Point(566, 155);
        btnGoto.Size = new System.Drawing.Size(110, 30);
        btnGoto.Text = "5) 访问 URL";
        btnGoto.Enabled = false;
        btnGoto.Click += btnGoto_Click;

        btnCloseAll.Location = new System.Drawing.Point(684, 155);
        btnCloseAll.Size = new System.Drawing.Size(101, 30);
        btnCloseAll.Text = "清理/退出";
        btnCloseAll.Enabled = false;
        btnCloseAll.Click += btnCloseAll_Click;

        // Row 2: Run-all, reset-run-all, screenshot options
        btnRunAll.Location = new System.Drawing.Point(12, 195);
        btnRunAll.Size = new System.Drawing.Size(110, 30);
        btnRunAll.Text = "一键运行";
        btnRunAll.Click += btnRunAll_Click;

        btnResetRunAll.Location = new System.Drawing.Point(132, 195);
        btnResetRunAll.Size = new System.Drawing.Size(140, 30);
        btnResetRunAll.Text = "重置并全跑";
        btnResetRunAll.Click += btnResetRunAll_Click;

        chkAutoScreenshot.Location = new System.Drawing.Point(290, 200);
        chkAutoScreenshot.AutoSize = true;
        chkAutoScreenshot.Text = "导航后自动截图（保存至默认目录）";
        chkAutoScreenshot.Checked = true;

        // Row 3: logging & snapshot
        btnStartLogging.Location = new System.Drawing.Point(12, 235);
        btnStartLogging.Size = new System.Drawing.Size(110, 30);
        btnStartLogging.Text = "开始日志";
        btnStartLogging.Click += btnStartLogging_Click;

        btnStopLogging.Location = new System.Drawing.Point(132, 235);
        btnStopLogging.Size = new System.Drawing.Size(110, 30);
        btnStopLogging.Text = "停止日志";
        btnStopLogging.Enabled = false;
        btnStopLogging.Click += btnStopLogging_Click;

        btnSaveSnapshot.Location = new System.Drawing.Point(252, 235);
        btnSaveSnapshot.Size = new System.Drawing.Size(130, 30);
        btnSaveSnapshot.Text = "保存快照 (HTML+PNG+JSON)";
        btnSaveSnapshot.Click += btnSaveSnapshot_Click;

        // —— 代理 / TLS ——
        lblProxy.AutoSize = true;
        lblProxy.Location = new System.Drawing.Point(12, 270);
        lblProxy.Text = "代理(--proxy-server)：";

        txtProxy.Location = new System.Drawing.Point(150, 267);
        txtProxy.Size = new System.Drawing.Size(250, 23);

        chkIgnoreTls.Location = new System.Drawing.Point(420, 268);
        chkIgnoreTls.AutoSize = true;
        chkIgnoreTls.Text = "忽略 TLS 证书错误 (IgnoreHTTPSErrors)";
        chkIgnoreTls.Checked = false;

        // —— 脚本注入 ——
        chkInitScript.Location = new System.Drawing.Point(12, 330);
        chkInitScript.AutoSize = true;
        chkInitScript.Text = "AddInitScript（所有页面自动注入）";

        txtInitScript.Location = new System.Drawing.Point(12, 350);
        txtInitScript.Size = new System.Drawing.Size(370, 23);

        chkPostNavScript.Location = new System.Drawing.Point(400, 330);
        chkPostNavScript.AutoSize = true;
        chkPostNavScript.Text = "导航后执行 Evaluate";

        txtPostNavScript.Location = new System.Drawing.Point(400, 350);
        txtPostNavScript.Size = new System.Drawing.Size(370, 23);

        chkExposeDotnet.Location = new System.Drawing.Point(12, 380);
        chkExposeDotnet.AutoSize = true;
        chkExposeDotnet.Text = "暴露 .NET 方法到页面 (ExposeFunction)";
        chkExposeDotnet.Checked = false;

        txtExposeName.Location = new System.Drawing.Point(280, 378);
        txtExposeName.Size = new System.Drawing.Size(150, 23);
        txtExposeName.Text = "dotnetPing";

        // —— 重试设置 ——
        lblRetry.AutoSize = true;
        lblRetry.Location = new System.Drawing.Point(450, 382);
        lblRetry.Text = "重试(次/毫秒)：";

        numRetryCount.Location = new System.Drawing.Point(540, 380);
        numRetryCount.Minimum = 0;
        numRetryCount.Maximum = 10;
        numRetryCount.Value = 2;

        numRetryDelayMs.Location = new System.Drawing.Point(610, 380);
        numRetryDelayMs.Minimum = 0;
        numRetryDelayMs.Maximum = 30000;
        numRetryDelayMs.Increment = 250;
        numRetryDelayMs.Value = 1000;

        // —— /json 导出 ——
        btnExportJsonList.Location = new System.Drawing.Point(12, 410);
        btnExportJsonList.Size = new System.Drawing.Size(140, 28);
        btnExportJsonList.Text = "导出 /json/list";
        btnExportJsonList.Click += btnExportJsonList_Click;

        btnExportJsonProtocol.Location = new System.Drawing.Point(160, 410);
        btnExportJsonProtocol.Size = new System.Drawing.Size(140, 28);
        btnExportJsonProtocol.Text = "导出 /json/protocol";
        btnExportJsonProtocol.Click += btnExportJsonProtocol_Click;

        // —— 多页辅助 ——
        btnRefreshPages.Location = new System.Drawing.Point(310, 410);
        btnRefreshPages.Size = new System.Drawing.Size(110, 28);
        btnRefreshPages.Text = "刷新页面列表";
        btnRefreshPages.Click += btnRefreshPages_Click;

        txtNewTabUrl.Location = new System.Drawing.Point(430, 412);
        txtNewTabUrl.Size = new System.Drawing.Size(260, 23);
        txtNewTabUrl.Text = "https://example.com";

        btnOpenNewTab.Location = new System.Drawing.Point(700, 410);
        btnOpenNewTab.Size = new System.Drawing.Size(70, 28);
        btnOpenNewTab.Text = "新开Tab";
        btnOpenNewTab.Click += btnOpenNewTab_Click;

        // Page list & log (moved down)
        lblPages.AutoSize = true;
        lblPages.Location = new System.Drawing.Point(12, 445);
        lblPages.Text = "页面 (Pages)：";

        lstPages.Location = new System.Drawing.Point(12, 465);
        lstPages.Size = new System.Drawing.Size(240, 250);
        lstPages.SelectedIndexChanged += lstPages_SelectedIndexChanged;

        txtLog.Location = new System.Drawing.Point(260, 465);
        txtLog.Multiline = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        txtLog.WordWrap = false;
        txtLog.Size = new System.Drawing.Size(525, 250);
        txtLog.ReadOnly = true;

        // Form
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 740);
        Controls.Add(lblExe);
        Controls.Add(lblPort);
        Controls.Add(lblUrl);
        Controls.Add(txtExePath);
        Controls.Add(btnBrowseExe);
        Controls.Add(numPort);
        Controls.Add(txtStartUrl);

        Controls.Add(btnLaunch);
        Controls.Add(btnWaitDevTools);
        Controls.Add(btnConnect);
        Controls.Add(btnNewPage);
        Controls.Add(btnGoto);
        Controls.Add(btnCloseAll);

        Controls.Add(btnRunAll);
        Controls.Add(btnResetRunAll);
        Controls.Add(chkAutoScreenshot);

        Controls.Add(btnStartLogging);
        Controls.Add(btnStopLogging);
        Controls.Add(btnSaveSnapshot);

        Controls.Add(lblProxy);
        Controls.Add(txtProxy);
        Controls.Add(chkIgnoreTls);

        Controls.Add(chkInitScript);
        Controls.Add(txtInitScript);
        Controls.Add(chkPostNavScript);
        Controls.Add(txtPostNavScript);

        Controls.Add(chkExposeDotnet);
        Controls.Add(txtExposeName);

        Controls.Add(lblRetry);
        Controls.Add(numRetryCount);
        Controls.Add(numRetryDelayMs);

        Controls.Add(btnExportJsonList);
        Controls.Add(btnExportJsonProtocol);

        Controls.Add(btnRefreshPages);
        Controls.Add(txtNewTabUrl);
        Controls.Add(btnOpenNewTab);

        Controls.Add(lblPages);
        Controls.Add(lstPages);
        Controls.Add(txtLog);

        Text = "External Browser (CDP) Launcher - WinForms";
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
