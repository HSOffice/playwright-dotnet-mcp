using System.Drawing;
using System.Windows.Forms;

namespace PlaywrightRemoteBrowserLauncher;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.TextBox txtExePath;
    private System.Windows.Forms.Button btnBrowseExe;
    private System.Windows.Forms.NumericUpDown numPort;
    private System.Windows.Forms.TextBox txtStartUrl;
    private System.Windows.Forms.Label lblExe;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.Label lblUrl;

    private System.Windows.Forms.GroupBox grpWorkflow;
    private System.Windows.Forms.Button btnLaunch;
    private System.Windows.Forms.Button btnWaitDevTools;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnNewPage;
    private System.Windows.Forms.Button btnGoto;
    private System.Windows.Forms.Button btnCloseAll;

    private System.Windows.Forms.GroupBox grpAutomation;
    private System.Windows.Forms.Button btnRunAll;
    private System.Windows.Forms.Button btnResetRunAll;
    private System.Windows.Forms.CheckBox chkAutoScreenshot;
    private System.Windows.Forms.Button btnStartLogging;
    private System.Windows.Forms.Button btnStopLogging;
    private System.Windows.Forms.Button btnSaveSnapshot;

    private System.Windows.Forms.GroupBox grpNetwork;
    private System.Windows.Forms.Label lblProxy;
    private System.Windows.Forms.TextBox txtProxy;
    private System.Windows.Forms.CheckBox chkIgnoreTls;

    private System.Windows.Forms.GroupBox grpAdvanced;
    private System.Windows.Forms.CheckBox chkExposeDotnet;
    private System.Windows.Forms.TextBox txtExposeName;
    private System.Windows.Forms.Label lblRetry;
    private System.Windows.Forms.NumericUpDown numRetryCount;
    private System.Windows.Forms.NumericUpDown numRetryDelayMs;

    private System.Windows.Forms.GroupBox grpPagesLog;
    private System.Windows.Forms.Label lblPages;
    private System.Windows.Forms.ListBox lstPages;
    private System.Windows.Forms.Label lblLog;
    private System.Windows.Forms.TextBox txtLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        grpConnection = new GroupBox();
        lblExe = new Label();
        txtExePath = new TextBox();
        btnBrowseExe = new Button();
        lblPort = new Label();
        numPort = new NumericUpDown();
        lblUrl = new Label();
        txtStartUrl = new TextBox();
        grpWorkflow = new GroupBox();
        btnLaunch = new Button();
        btnWaitDevTools = new Button();
        btnConnect = new Button();
        btnNewPage = new Button();
        btnGoto = new Button();
        btnCloseAll = new Button();
        grpAutomation = new GroupBox();
        btnRunAll = new Button();
        btnResetRunAll = new Button();
        chkAutoScreenshot = new CheckBox();
        btnStartLogging = new Button();
        btnStopLogging = new Button();
        btnSaveSnapshot = new Button();
        grpNetwork = new GroupBox();
        lblProxy = new Label();
        txtProxy = new TextBox();
        chkIgnoreTls = new CheckBox();
        grpAdvanced = new GroupBox();
        chkExposeDotnet = new CheckBox();
        txtExposeName = new TextBox();
        lblRetry = new Label();
        numRetryCount = new NumericUpDown();
        numRetryDelayMs = new NumericUpDown();
        grpPagesLog = new GroupBox();
        txtLog = new TextBox();
        lblLog = new Label();
        lstPages = new ListBox();
        lblPages = new Label();
        txtPostNavScript = new TextBox();
        chkPostNavScript = new CheckBox();
        txtInitScript = new TextBox();
        chkInitScript = new CheckBox();
        grpScripts = new GroupBox();
        btnOpenNewTab = new Button();
        txtNewTabUrl = new TextBox();
        btnRefreshPages = new Button();
        btnExportJsonProtocol = new Button();
        btnExportJsonList = new Button();
        grpUtilities = new GroupBox();
        grpConnection.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        grpWorkflow.SuspendLayout();
        grpAutomation.SuspendLayout();
        grpNetwork.SuspendLayout();
        grpAdvanced.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).BeginInit();
        grpPagesLog.SuspendLayout();
        grpScripts.SuspendLayout();
        grpUtilities.SuspendLayout();
        SuspendLayout();
        // 
        // grpConnection
        // 
        grpConnection.Controls.Add(lblExe);
        grpConnection.Controls.Add(txtExePath);
        grpConnection.Controls.Add(btnBrowseExe);
        grpConnection.Controls.Add(lblPort);
        grpConnection.Controls.Add(numPort);
        grpConnection.Controls.Add(lblUrl);
        grpConnection.Controls.Add(txtStartUrl);
        grpConnection.Location = new Point(13, 13);
        grpConnection.Margin = new Padding(4);
        grpConnection.Name = "grpConnection";
        grpConnection.Padding = new Padding(4);
        grpConnection.Size = new Size(674, 112);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "浏览器连接";
        // 
        // lblExe
        // 
        lblExe.AutoSize = true;
        lblExe.Location = new Point(19, 33);
        lblExe.Margin = new Padding(4, 0, 4, 0);
        lblExe.Name = "lblExe";
        lblExe.Size = new Size(99, 20);
        lblExe.TabIndex = 0;
        lblExe.Text = "可执行文件：";
        // 
        // txtExePath
        // 
        txtExePath.Location = new Point(130, 28);
        txtExePath.Margin = new Padding(4);
        txtExePath.Name = "txtExePath";
        txtExePath.Size = new Size(327, 27);
        txtExePath.TabIndex = 1;
        // 
        // btnBrowseExe
        // 
        btnBrowseExe.Location = new Point(465, 27);
        btnBrowseExe.Margin = new Padding(4);
        btnBrowseExe.Name = "btnBrowseExe";
        btnBrowseExe.Size = new Size(65, 33);
        btnBrowseExe.TabIndex = 2;
        btnBrowseExe.Text = "浏览…";
        btnBrowseExe.UseVisualStyleBackColor = true;
        btnBrowseExe.Click += btnBrowseExe_Click;
        // 
        // lblPort
        // 
        lblPort.AutoSize = true;
        lblPort.Location = new Point(538, 33);
        lblPort.Margin = new Padding(4, 0, 4, 0);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(54, 20);
        lblPort.TabIndex = 3;
        lblPort.Text = "端口：";
        // 
        // numPort
        // 
        numPort.Location = new Point(600, 31);
        numPort.Margin = new Padding(4);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new Size(62, 27);
        numPort.TabIndex = 4;
        numPort.Value = new decimal(new int[] { 9222, 0, 0, 0 });
        // 
        // lblUrl
        // 
        lblUrl.AutoSize = true;
        lblUrl.Location = new Point(16, 69);
        lblUrl.Margin = new Padding(4, 0, 4, 0);
        lblUrl.Name = "lblUrl";
        lblUrl.Size = new Size(87, 20);
        lblUrl.TabIndex = 5;
        lblUrl.Text = "起始 URL：";
        // 
        // txtStartUrl
        // 
        txtStartUrl.Location = new Point(130, 66);
        txtStartUrl.Margin = new Padding(4);
        txtStartUrl.Name = "txtStartUrl";
        txtStartUrl.Size = new Size(532, 27);
        txtStartUrl.TabIndex = 6;
        // 
        // grpWorkflow
        // 
        grpWorkflow.Controls.Add(btnLaunch);
        grpWorkflow.Controls.Add(btnWaitDevTools);
        grpWorkflow.Controls.Add(btnConnect);
        grpWorkflow.Controls.Add(btnNewPage);
        grpWorkflow.Controls.Add(btnGoto);
        grpWorkflow.Controls.Add(btnCloseAll);
        grpWorkflow.Location = new Point(13, 134);
        grpWorkflow.Margin = new Padding(4);
        grpWorkflow.Name = "grpWorkflow";
        grpWorkflow.Padding = new Padding(4);
        grpWorkflow.Size = new Size(674, 78);
        grpWorkflow.TabIndex = 1;
        grpWorkflow.TabStop = false;
        grpWorkflow.Text = "分步操作";
        // 
        // btnLaunch
        // 
        btnLaunch.Location = new Point(14, 24);
        btnLaunch.Margin = new Padding(4);
        btnLaunch.Name = "btnLaunch";
        btnLaunch.Size = new Size(120, 40);
        btnLaunch.TabIndex = 0;
        btnLaunch.Text = "1-启动/获取进程";
        btnLaunch.UseVisualStyleBackColor = true;
        btnLaunch.Click += btnLaunch_Click;
        //
        // btnWaitDevTools
        //
        btnWaitDevTools.Enabled = false;
        btnWaitDevTools.Location = new Point(138, 24);
        btnWaitDevTools.Margin = new Padding(4);
        btnWaitDevTools.Name = "btnWaitDevTools";
        btnWaitDevTools.Size = new Size(100, 40);
        btnWaitDevTools.TabIndex = 1;
        btnWaitDevTools.Text = "2-DevTools";
        btnWaitDevTools.UseVisualStyleBackColor = true;
        btnWaitDevTools.Click += btnWaitDevTools_Click;
        // 
        // btnConnect
        // 
        btnConnect.Enabled = false;
        btnConnect.Location = new Point(242, 24);
        btnConnect.Margin = new Padding(4);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(110, 40);
        btnConnect.TabIndex = 2;
        btnConnect.Text = "3-Playwright";
        btnConnect.UseVisualStyleBackColor = true;
        btnConnect.Click += btnConnect_Click;
        // 
        // btnNewPage
        // 
        btnNewPage.Enabled = false;
        btnNewPage.Location = new Point(356, 24);
        btnNewPage.Margin = new Padding(4);
        btnNewPage.Name = "btnNewPage";
        btnNewPage.Size = new Size(105, 40);
        btnNewPage.TabIndex = 3;
        btnNewPage.Text = "4-获取 Pages";
        btnNewPage.UseVisualStyleBackColor = true;
        btnNewPage.Click += btnNewPage_Click;
        // 
        // btnGoto
        // 
        btnGoto.Enabled = false;
        btnGoto.Location = new Point(465, 24);
        btnGoto.Margin = new Padding(4);
        btnGoto.Name = "btnGoto";
        btnGoto.Size = new Size(100, 40);
        btnGoto.TabIndex = 4;
        btnGoto.Text = "5-访问 URL";
        btnGoto.UseVisualStyleBackColor = true;
        btnGoto.Click += btnGoto_Click;
        // 
        // btnCloseAll
        // 
        btnCloseAll.Enabled = false;
        btnCloseAll.Location = new Point(569, 24);
        btnCloseAll.Margin = new Padding(4);
        btnCloseAll.Name = "btnCloseAll";
        btnCloseAll.Size = new Size(100, 40);
        btnCloseAll.TabIndex = 5;
        btnCloseAll.Text = "清理 / 退出";
        btnCloseAll.UseVisualStyleBackColor = true;
        btnCloseAll.Click += btnCloseAll_Click;
        // 
        // grpAutomation
        // 
        grpAutomation.Controls.Add(btnRunAll);
        grpAutomation.Controls.Add(btnResetRunAll);
        grpAutomation.Controls.Add(chkAutoScreenshot);
        grpAutomation.Controls.Add(btnStartLogging);
        grpAutomation.Controls.Add(btnStopLogging);
        grpAutomation.Controls.Add(btnSaveSnapshot);
        grpAutomation.Location = new Point(13, 220);
        grpAutomation.Margin = new Padding(4);
        grpAutomation.Name = "grpAutomation";
        grpAutomation.Padding = new Padding(4);
        grpAutomation.Size = new Size(674, 78);
        grpAutomation.TabIndex = 2;
        grpAutomation.TabStop = false;
        grpAutomation.Text = "自动化与日志";
        // 
        // btnRunAll
        // 
        btnRunAll.Location = new Point(16, 28);
        btnRunAll.Margin = new Padding(4);
        btnRunAll.Name = "btnRunAll";
        btnRunAll.Size = new Size(90, 40);
        btnRunAll.TabIndex = 0;
        btnRunAll.Text = "一键运行";
        btnRunAll.UseVisualStyleBackColor = true;
        btnRunAll.Click += btnRunAll_Click;
        // 
        // btnResetRunAll
        // 
        btnResetRunAll.Location = new Point(114, 28);
        btnResetRunAll.Margin = new Padding(4);
        btnResetRunAll.Name = "btnResetRunAll";
        btnResetRunAll.Size = new Size(90, 40);
        btnResetRunAll.TabIndex = 1;
        btnResetRunAll.Text = "重置全跑";
        btnResetRunAll.UseVisualStyleBackColor = true;
        btnResetRunAll.Click += btnResetRunAll_Click;
        // 
        // chkAutoScreenshot
        // 
        chkAutoScreenshot.AutoSize = true;
        chkAutoScreenshot.Checked = true;
        chkAutoScreenshot.CheckState = CheckState.Checked;
        chkAutoScreenshot.Location = new Point(523, 37);
        chkAutoScreenshot.Margin = new Padding(4);
        chkAutoScreenshot.Name = "chkAutoScreenshot";
        chkAutoScreenshot.Size = new Size(136, 24);
        chkAutoScreenshot.TabIndex = 2;
        chkAutoScreenshot.Text = "导航后自动截图";
        chkAutoScreenshot.UseVisualStyleBackColor = true;
        // 
        // btnStartLogging
        // 
        btnStartLogging.Location = new Point(212, 28);
        btnStartLogging.Margin = new Padding(4);
        btnStartLogging.Name = "btnStartLogging";
        btnStartLogging.Size = new Size(90, 40);
        btnStartLogging.TabIndex = 3;
        btnStartLogging.Text = "开始日志";
        btnStartLogging.UseVisualStyleBackColor = true;
        btnStartLogging.Click += btnStartLogging_Click;
        // 
        // btnStopLogging
        // 
        btnStopLogging.Enabled = false;
        btnStopLogging.Location = new Point(310, 28);
        btnStopLogging.Margin = new Padding(4);
        btnStopLogging.Name = "btnStopLogging";
        btnStopLogging.Size = new Size(90, 40);
        btnStopLogging.TabIndex = 4;
        btnStopLogging.Text = "停止日志";
        btnStopLogging.UseVisualStyleBackColor = true;
        btnStopLogging.Click += btnStopLogging_Click;
        // 
        // btnSaveSnapshot
        // 
        btnSaveSnapshot.Location = new Point(408, 28);
        btnSaveSnapshot.Margin = new Padding(4);
        btnSaveSnapshot.Name = "btnSaveSnapshot";
        btnSaveSnapshot.Size = new Size(90, 40);
        btnSaveSnapshot.TabIndex = 5;
        btnSaveSnapshot.Text = "保存快照";
        btnSaveSnapshot.UseVisualStyleBackColor = true;
        btnSaveSnapshot.Click += btnSaveSnapshot_Click;
        // 
        // grpNetwork
        // 
        grpNetwork.Controls.Add(lblProxy);
        grpNetwork.Controls.Add(txtProxy);
        grpNetwork.Controls.Add(chkIgnoreTls);
        grpNetwork.Location = new Point(13, 306);
        grpNetwork.Margin = new Padding(4);
        grpNetwork.Name = "grpNetwork";
        grpNetwork.Padding = new Padding(4);
        grpNetwork.Size = new Size(353, 100);
        grpNetwork.TabIndex = 3;
        grpNetwork.TabStop = false;
        grpNetwork.Text = "网络设置";
        // 
        // lblProxy
        // 
        lblProxy.AutoSize = true;
        lblProxy.Location = new Point(19, 40);
        lblProxy.Margin = new Padding(4, 0, 4, 0);
        lblProxy.Name = "lblProxy";
        lblProxy.Size = new Size(173, 20);
        lblProxy.TabIndex = 0;
        lblProxy.Text = "代理 (--proxy-server)：";
        // 
        // txtProxy
        // 
        txtProxy.Location = new Point(200, 35);
        txtProxy.Margin = new Padding(4);
        txtProxy.Name = "txtProxy";
        txtProxy.Size = new Size(104, 27);
        txtProxy.TabIndex = 1;
        // 
        // chkIgnoreTls
        // 
        chkIgnoreTls.AutoSize = true;
        chkIgnoreTls.Location = new Point(19, 70);
        chkIgnoreTls.Margin = new Padding(4);
        chkIgnoreTls.Name = "chkIgnoreTls";
        chkIgnoreTls.Size = new Size(308, 24);
        chkIgnoreTls.TabIndex = 2;
        chkIgnoreTls.Text = "忽略 TLS 证书错误 (IgnoreHTTPSErrors)";
        chkIgnoreTls.UseVisualStyleBackColor = true;
        // 
        // grpAdvanced
        // 
        grpAdvanced.Controls.Add(chkExposeDotnet);
        grpAdvanced.Controls.Add(txtExposeName);
        grpAdvanced.Controls.Add(lblRetry);
        grpAdvanced.Controls.Add(numRetryCount);
        grpAdvanced.Controls.Add(numRetryDelayMs);
        grpAdvanced.Location = new Point(374, 488);
        grpAdvanced.Margin = new Padding(4);
        grpAdvanced.Name = "grpAdvanced";
        grpAdvanced.Padding = new Padding(4);
        grpAdvanced.Size = new Size(313, 103);
        grpAdvanced.TabIndex = 5;
        grpAdvanced.TabStop = false;
        grpAdvanced.Text = "高级选项";
        // 
        // chkExposeDotnet
        // 
        chkExposeDotnet.AutoSize = true;
        chkExposeDotnet.Location = new Point(22, 28);
        chkExposeDotnet.Margin = new Padding(4);
        chkExposeDotnet.Name = "chkExposeDotnet";
        chkExposeDotnet.Size = new Size(177, 24);
        chkExposeDotnet.TabIndex = 0;
        chkExposeDotnet.Text = "暴露 .NET 方法到页面";
        chkExposeDotnet.UseVisualStyleBackColor = true;
        // 
        // txtExposeName
        // 
        txtExposeName.Location = new Point(207, 25);
        txtExposeName.Margin = new Padding(4);
        txtExposeName.Name = "txtExposeName";
        txtExposeName.Size = new Size(91, 27);
        txtExposeName.TabIndex = 1;
        txtExposeName.Text = "dotnetPing";
        // 
        // lblRetry
        // 
        lblRetry.AutoSize = true;
        lblRetry.Location = new Point(21, 63);
        lblRetry.Margin = new Padding(4, 0, 4, 0);
        lblRetry.Name = "lblRetry";
        lblRetry.Size = new Size(115, 20);
        lblRetry.TabIndex = 2;
        lblRetry.Text = "重试(次/毫秒)：";
        // 
        // numRetryCount
        // 
        numRetryCount.Location = new Point(143, 60);
        numRetryCount.Margin = new Padding(4);
        numRetryCount.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        numRetryCount.Name = "numRetryCount";
        numRetryCount.Size = new Size(48, 27);
        numRetryCount.TabIndex = 3;
        numRetryCount.Value = new decimal(new int[] { 2, 0, 0, 0 });
        // 
        // numRetryDelayMs
        // 
        numRetryDelayMs.Increment = new decimal(new int[] { 250, 0, 0, 0 });
        numRetryDelayMs.Location = new Point(207, 60);
        numRetryDelayMs.Margin = new Padding(4);
        numRetryDelayMs.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
        numRetryDelayMs.Name = "numRetryDelayMs";
        numRetryDelayMs.Size = new Size(91, 27);
        numRetryDelayMs.TabIndex = 4;
        numRetryDelayMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });
        // 
        // grpPagesLog
        // 
        grpPagesLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpPagesLog.Controls.Add(txtLog);
        grpPagesLog.Controls.Add(lblLog);
        grpPagesLog.Controls.Add(lstPages);
        grpPagesLog.Controls.Add(lblPages);
        grpPagesLog.Location = new Point(695, 13);
        grpPagesLog.Margin = new Padding(4);
        grpPagesLog.Name = "grpPagesLog";
        grpPagesLog.Padding = new Padding(4);
        grpPagesLog.Size = new Size(388, 578);
        grpPagesLog.TabIndex = 7;
        grpPagesLog.TabStop = false;
        grpPagesLog.Text = "页面与日志";
        // 
        // txtLog
        // 
        txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.Location = new Point(19, 207);
        txtLog.Margin = new Padding(4);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(348, 364);
        txtLog.TabIndex = 3;
        txtLog.WordWrap = false;
        // 
        // lblLog
        // 
        lblLog.AutoSize = true;
        lblLog.Location = new Point(19, 180);
        lblLog.Margin = new Padding(4, 0, 4, 0);
        lblLog.Name = "lblLog";
        lblLog.Size = new Size(84, 20);
        lblLog.TabIndex = 2;
        lblLog.Text = "日志输出：";
        // 
        // lstPages
        // 
        lstPages.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lstPages.FormattingEnabled = true;
        lstPages.Location = new Point(19, 67);
        lstPages.Margin = new Padding(4);
        lstPages.Name = "lstPages";
        lstPages.Size = new Size(348, 104);
        lstPages.TabIndex = 1;
        lstPages.SelectedIndexChanged += lstPages_SelectedIndexChanged;
        // 
        // lblPages
        // 
        lblPages.AutoSize = true;
        lblPages.Location = new Point(19, 40);
        lblPages.Margin = new Padding(4, 0, 4, 0);
        lblPages.Name = "lblPages";
        lblPages.Size = new Size(111, 20);
        lblPages.TabIndex = 0;
        lblPages.Text = "页面 (Pages)：";
        // 
        // txtPostNavScript
        // 
        txtPostNavScript.Location = new Point(22, 131);
        txtPostNavScript.Margin = new Padding(4);
        txtPostNavScript.Name = "txtPostNavScript";
        txtPostNavScript.Size = new Size(276, 27);
        txtPostNavScript.TabIndex = 3;
        // 
        // chkPostNavScript
        // 
        chkPostNavScript.AutoSize = true;
        chkPostNavScript.Location = new Point(23, 99);
        chkPostNavScript.Margin = new Padding(4);
        chkPostNavScript.Name = "chkPostNavScript";
        chkPostNavScript.Size = new Size(170, 24);
        chkPostNavScript.TabIndex = 2;
        chkPostNavScript.Text = "导航后执行 Evaluate";
        chkPostNavScript.UseVisualStyleBackColor = true;
        // 
        // txtInitScript
        // 
        txtInitScript.Location = new Point(23, 64);
        txtInitScript.Margin = new Padding(4);
        txtInitScript.Name = "txtInitScript";
        txtInitScript.Size = new Size(275, 27);
        txtInitScript.TabIndex = 1;
        // 
        // chkInitScript
        // 
        chkInitScript.AutoSize = true;
        chkInitScript.Location = new Point(23, 32);
        chkInitScript.Margin = new Padding(4);
        chkInitScript.Name = "chkInitScript";
        chkInitScript.Size = new Size(278, 24);
        chkInitScript.TabIndex = 0;
        chkInitScript.Text = "AddInitScript（所有页面自动注入）";
        chkInitScript.UseVisualStyleBackColor = true;
        // 
        // grpScripts
        // 
        grpScripts.Controls.Add(chkInitScript);
        grpScripts.Controls.Add(txtInitScript);
        grpScripts.Controls.Add(chkPostNavScript);
        grpScripts.Controls.Add(txtPostNavScript);
        grpScripts.Location = new Point(374, 306);
        grpScripts.Margin = new Padding(4);
        grpScripts.Name = "grpScripts";
        grpScripts.Padding = new Padding(4);
        grpScripts.Size = new Size(313, 174);
        grpScripts.TabIndex = 4;
        grpScripts.TabStop = false;
        grpScripts.Text = "脚本注入";
        // 
        // btnOpenNewTab
        // 
        btnOpenNewTab.Location = new Point(171, 76);
        btnOpenNewTab.Margin = new Padding(4);
        btnOpenNewTab.Name = "btnOpenNewTab";
        btnOpenNewTab.Size = new Size(166, 36);
        btnOpenNewTab.TabIndex = 4;
        btnOpenNewTab.Text = "新开 Tab";
        btnOpenNewTab.UseVisualStyleBackColor = true;
        btnOpenNewTab.Click += btnOpenNewTab_Click;
        // 
        // txtNewTabUrl
        // 
        txtNewTabUrl.Location = new Point(14, 124);
        txtNewTabUrl.Margin = new Padding(4);
        txtNewTabUrl.Name = "txtNewTabUrl";
        txtNewTabUrl.Size = new Size(323, 27);
        txtNewTabUrl.TabIndex = 3;
        txtNewTabUrl.Text = "https://example.com";
        // 
        // btnRefreshPages
        // 
        btnRefreshPages.Location = new Point(14, 76);
        btnRefreshPages.Margin = new Padding(4);
        btnRefreshPages.Name = "btnRefreshPages";
        btnRefreshPages.Size = new Size(132, 40);
        btnRefreshPages.TabIndex = 2;
        btnRefreshPages.Text = "刷新页面列表";
        btnRefreshPages.UseVisualStyleBackColor = true;
        btnRefreshPages.Click += btnRefreshPages_Click;
        // 
        // btnExportJsonProtocol
        // 
        btnExportJsonProtocol.Location = new Point(171, 28);
        btnExportJsonProtocol.Margin = new Padding(4);
        btnExportJsonProtocol.Name = "btnExportJsonProtocol";
        btnExportJsonProtocol.Size = new Size(166, 40);
        btnExportJsonProtocol.TabIndex = 1;
        btnExportJsonProtocol.Text = "导出 /json/protocol";
        btnExportJsonProtocol.UseVisualStyleBackColor = true;
        btnExportJsonProtocol.Click += btnExportJsonProtocol_Click;
        // 
        // btnExportJsonList
        // 
        btnExportJsonList.Location = new Point(14, 28);
        btnExportJsonList.Margin = new Padding(4);
        btnExportJsonList.Name = "btnExportJsonList";
        btnExportJsonList.Size = new Size(132, 40);
        btnExportJsonList.TabIndex = 0;
        btnExportJsonList.Text = "导出 /json/list";
        btnExportJsonList.UseVisualStyleBackColor = true;
        btnExportJsonList.Click += btnExportJsonList_Click;
        // 
        // grpUtilities
        // 
        grpUtilities.Controls.Add(btnExportJsonList);
        grpUtilities.Controls.Add(btnExportJsonProtocol);
        grpUtilities.Controls.Add(btnRefreshPages);
        grpUtilities.Controls.Add(txtNewTabUrl);
        grpUtilities.Controls.Add(btnOpenNewTab);
        grpUtilities.Location = new Point(13, 414);
        grpUtilities.Margin = new Padding(4);
        grpUtilities.Name = "grpUtilities";
        grpUtilities.Padding = new Padding(4);
        grpUtilities.Size = new Size(353, 177);
        grpUtilities.TabIndex = 6;
        grpUtilities.TabStop = false;
        grpUtilities.Text = "实用工具";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1096, 599);
        Controls.Add(grpPagesLog);
        Controls.Add(grpUtilities);
        Controls.Add(grpAdvanced);
        Controls.Add(grpScripts);
        Controls.Add(grpNetwork);
        Controls.Add(grpAutomation);
        Controls.Add(grpWorkflow);
        Controls.Add(grpConnection);
        Margin = new Padding(4);
        Name = "MainForm";
        Text = "External Browser (CDP) Launcher - WinForms";
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        grpWorkflow.ResumeLayout(false);
        grpAutomation.ResumeLayout(false);
        grpAutomation.PerformLayout();
        grpNetwork.ResumeLayout(false);
        grpNetwork.PerformLayout();
        grpAdvanced.ResumeLayout(false);
        grpAdvanced.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).EndInit();
        grpPagesLog.ResumeLayout(false);
        grpPagesLog.PerformLayout();
        grpScripts.ResumeLayout(false);
        grpScripts.PerformLayout();
        grpUtilities.ResumeLayout(false);
        grpUtilities.PerformLayout();
        ResumeLayout(false);
    }
    private TextBox txtPostNavScript;
    private CheckBox chkPostNavScript;
    private TextBox txtInitScript;
    private CheckBox chkInitScript;
    private GroupBox grpScripts;
    private Button btnOpenNewTab;
    private TextBox txtNewTabUrl;
    private Button btnRefreshPages;
    private Button btnExportJsonProtocol;
    private Button btnExportJsonList;
    private GroupBox grpUtilities;
}
