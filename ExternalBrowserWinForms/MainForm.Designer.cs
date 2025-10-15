using System.Drawing;
using System.Windows.Forms;

namespace ExternalBrowserWinForms;

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

    private System.Windows.Forms.GroupBox grpScripts;
    private System.Windows.Forms.CheckBox chkInitScript;
    private System.Windows.Forms.TextBox txtInitScript;
    private System.Windows.Forms.CheckBox chkPostNavScript;
    private System.Windows.Forms.TextBox txtPostNavScript;

    private System.Windows.Forms.GroupBox grpAdvanced;
    private System.Windows.Forms.CheckBox chkExposeDotnet;
    private System.Windows.Forms.TextBox txtExposeName;
    private System.Windows.Forms.Label lblRetry;
    private System.Windows.Forms.NumericUpDown numRetryCount;
    private System.Windows.Forms.NumericUpDown numRetryDelayMs;

    private System.Windows.Forms.GroupBox grpUtilities;
    private System.Windows.Forms.Button btnExportJsonList;
    private System.Windows.Forms.Button btnExportJsonProtocol;
    private System.Windows.Forms.Button btnRefreshPages;
    private System.Windows.Forms.TextBox txtNewTabUrl;
    private System.Windows.Forms.Button btnOpenNewTab;

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
        components = new System.ComponentModel.Container();
        grpConnection = new System.Windows.Forms.GroupBox();
        grpWorkflow = new System.Windows.Forms.GroupBox();
        grpAutomation = new System.Windows.Forms.GroupBox();
        grpNetwork = new System.Windows.Forms.GroupBox();
        grpScripts = new System.Windows.Forms.GroupBox();
        grpAdvanced = new System.Windows.Forms.GroupBox();
        grpUtilities = new System.Windows.Forms.GroupBox();
        grpPagesLog = new System.Windows.Forms.GroupBox();
        txtExePath = new System.Windows.Forms.TextBox();
        btnBrowseExe = new System.Windows.Forms.Button();
        numPort = new System.Windows.Forms.NumericUpDown();
        txtStartUrl = new System.Windows.Forms.TextBox();
        lblExe = new System.Windows.Forms.Label();
        lblPort = new System.Windows.Forms.Label();
        lblUrl = new System.Windows.Forms.Label();
        btnLaunch = new System.Windows.Forms.Button();
        btnWaitDevTools = new System.Windows.Forms.Button();
        btnConnect = new System.Windows.Forms.Button();
        btnNewPage = new System.Windows.Forms.Button();
        btnGoto = new System.Windows.Forms.Button();
        btnCloseAll = new System.Windows.Forms.Button();
        btnRunAll = new System.Windows.Forms.Button();
        btnResetRunAll = new System.Windows.Forms.Button();
        chkAutoScreenshot = new System.Windows.Forms.CheckBox();
        btnStartLogging = new System.Windows.Forms.Button();
        btnStopLogging = new System.Windows.Forms.Button();
        btnSaveSnapshot = new System.Windows.Forms.Button();
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
        lblPages = new System.Windows.Forms.Label();
        lstPages = new System.Windows.Forms.ListBox();
        lblLog = new System.Windows.Forms.Label();
        txtLog = new System.Windows.Forms.TextBox();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).BeginInit();
        SuspendLayout();
        grpConnection.SuspendLayout();
        grpWorkflow.SuspendLayout();
        grpAutomation.SuspendLayout();
        grpNetwork.SuspendLayout();
        grpScripts.SuspendLayout();
        grpAdvanced.SuspendLayout();
        grpUtilities.SuspendLayout();
        grpPagesLog.SuspendLayout();

        // grpConnection
        grpConnection.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpConnection.Controls.Add(lblExe);
        grpConnection.Controls.Add(txtExePath);
        grpConnection.Controls.Add(btnBrowseExe);
        grpConnection.Controls.Add(lblPort);
        grpConnection.Controls.Add(numPort);
        grpConnection.Controls.Add(lblUrl);
        grpConnection.Controls.Add(txtStartUrl);
        grpConnection.Location = new System.Drawing.Point(10, 10);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new System.Drawing.Size(880, 110);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "浏览器连接";

        // lblExe
        lblExe.AutoSize = true;
        lblExe.Location = new System.Drawing.Point(15, 30);
        lblExe.Name = "lblExe";
        lblExe.Size = new System.Drawing.Size(80, 15);
        lblExe.TabIndex = 0;
        lblExe.Text = "可执行文件：";

        // txtExePath
        txtExePath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtExePath.Location = new System.Drawing.Point(101, 26);
        txtExePath.Name = "txtExePath";
        txtExePath.Size = new System.Drawing.Size(679, 23);
        txtExePath.TabIndex = 1;

        // btnBrowseExe
        btnBrowseExe.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnBrowseExe.Location = new System.Drawing.Point(790, 25);
        btnBrowseExe.Name = "btnBrowseExe";
        btnBrowseExe.Size = new System.Drawing.Size(75, 25);
        btnBrowseExe.TabIndex = 2;
        btnBrowseExe.Text = "浏览…";
        btnBrowseExe.UseVisualStyleBackColor = true;
        btnBrowseExe.Click += btnBrowseExe_Click;

        // lblPort
        lblPort.AutoSize = true;
        lblPort.Location = new System.Drawing.Point(15, 65);
        lblPort.Name = "lblPort";
        lblPort.Size = new System.Drawing.Size(68, 15);
        lblPort.TabIndex = 3;
        lblPort.Text = "调试端口：";

        // numPort
        numPort.Location = new System.Drawing.Point(101, 63);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new System.Drawing.Size(120, 23);
        numPort.TabIndex = 4;
        numPort.Value = new decimal(new int[] { 9222, 0, 0, 0 });

        // lblUrl
        lblUrl.AutoSize = true;
        lblUrl.Location = new System.Drawing.Point(250, 65);
        lblUrl.Name = "lblUrl";
        lblUrl.Size = new System.Drawing.Size(64, 15);
        lblUrl.TabIndex = 5;
        lblUrl.Text = "起始 URL：";

        // txtStartUrl
        txtStartUrl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtStartUrl.Location = new System.Drawing.Point(320, 63);
        txtStartUrl.Name = "txtStartUrl";
        txtStartUrl.Size = new System.Drawing.Size(545, 23);
        txtStartUrl.TabIndex = 6;

        // grpWorkflow
        grpWorkflow.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpWorkflow.Controls.Add(btnLaunch);
        grpWorkflow.Controls.Add(btnWaitDevTools);
        grpWorkflow.Controls.Add(btnConnect);
        grpWorkflow.Controls.Add(btnNewPage);
        grpWorkflow.Controls.Add(btnGoto);
        grpWorkflow.Controls.Add(btnCloseAll);
        grpWorkflow.Location = new System.Drawing.Point(10, 130);
        grpWorkflow.Name = "grpWorkflow";
        grpWorkflow.Size = new System.Drawing.Size(880, 70);
        grpWorkflow.TabIndex = 1;
        grpWorkflow.TabStop = false;
        grpWorkflow.Text = "分步操作";

        // btnLaunch
        btnLaunch.Location = new System.Drawing.Point(15, 25);
        btnLaunch.Name = "btnLaunch";
        btnLaunch.Size = new System.Drawing.Size(120, 30);
        btnLaunch.TabIndex = 0;
        btnLaunch.Text = "1) 启动进程";
        btnLaunch.UseVisualStyleBackColor = true;
        btnLaunch.Click += btnLaunch_Click;

        // btnWaitDevTools
        btnWaitDevTools.Enabled = false;
        btnWaitDevTools.Location = new System.Drawing.Point(145, 25);
        btnWaitDevTools.Name = "btnWaitDevTools";
        btnWaitDevTools.Size = new System.Drawing.Size(140, 30);
        btnWaitDevTools.TabIndex = 1;
        btnWaitDevTools.Text = "2) 等待 DevTools";
        btnWaitDevTools.UseVisualStyleBackColor = true;
        btnWaitDevTools.Click += btnWaitDevTools_Click;

        // btnConnect
        btnConnect.Enabled = false;
        btnConnect.Location = new System.Drawing.Point(295, 25);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new System.Drawing.Size(150, 30);
        btnConnect.TabIndex = 2;
        btnConnect.Text = "3) 连接 Playwright";
        btnConnect.UseVisualStyleBackColor = true;
        btnConnect.Click += btnConnect_Click;

        // btnNewPage
        btnNewPage.Enabled = false;
        btnNewPage.Location = new System.Drawing.Point(455, 25);
        btnNewPage.Name = "btnNewPage";
        btnNewPage.Size = new System.Drawing.Size(130, 30);
        btnNewPage.TabIndex = 3;
        btnNewPage.Text = "4) 新建 Page";
        btnNewPage.UseVisualStyleBackColor = true;
        btnNewPage.Click += btnNewPage_Click;

        // btnGoto
        btnGoto.Enabled = false;
        btnGoto.Location = new System.Drawing.Point(595, 25);
        btnGoto.Name = "btnGoto";
        btnGoto.Size = new System.Drawing.Size(120, 30);
        btnGoto.TabIndex = 4;
        btnGoto.Text = "5) 访问 URL";
        btnGoto.UseVisualStyleBackColor = true;
        btnGoto.Click += btnGoto_Click;

        // btnCloseAll
        btnCloseAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnCloseAll.Enabled = false;
        btnCloseAll.Location = new System.Drawing.Point(730, 25);
        btnCloseAll.Name = "btnCloseAll";
        btnCloseAll.Size = new System.Drawing.Size(130, 30);
        btnCloseAll.TabIndex = 5;
        btnCloseAll.Text = "清理 / 退出";
        btnCloseAll.UseVisualStyleBackColor = true;
        btnCloseAll.Click += btnCloseAll_Click;

        // grpAutomation
        grpAutomation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpAutomation.Controls.Add(btnRunAll);
        grpAutomation.Controls.Add(btnResetRunAll);
        grpAutomation.Controls.Add(chkAutoScreenshot);
        grpAutomation.Controls.Add(btnStartLogging);
        grpAutomation.Controls.Add(btnStopLogging);
        grpAutomation.Controls.Add(btnSaveSnapshot);
        grpAutomation.Location = new System.Drawing.Point(10, 210);
        grpAutomation.Name = "grpAutomation";
        grpAutomation.Size = new System.Drawing.Size(880, 95);
        grpAutomation.TabIndex = 2;
        grpAutomation.TabStop = false;
        grpAutomation.Text = "自动化与日志";

        // btnRunAll
        btnRunAll.Location = new System.Drawing.Point(15, 25);
        btnRunAll.Name = "btnRunAll";
        btnRunAll.Size = new System.Drawing.Size(120, 30);
        btnRunAll.TabIndex = 0;
        btnRunAll.Text = "一键运行";
        btnRunAll.UseVisualStyleBackColor = true;
        btnRunAll.Click += btnRunAll_Click;

        // btnResetRunAll
        btnResetRunAll.Location = new System.Drawing.Point(145, 25);
        btnResetRunAll.Name = "btnResetRunAll";
        btnResetRunAll.Size = new System.Drawing.Size(140, 30);
        btnResetRunAll.TabIndex = 1;
        btnResetRunAll.Text = "重置并全跑";
        btnResetRunAll.UseVisualStyleBackColor = true;
        btnResetRunAll.Click += btnResetRunAll_Click;

        // chkAutoScreenshot
        chkAutoScreenshot.AutoSize = true;
        chkAutoScreenshot.Checked = true;
        chkAutoScreenshot.CheckState = System.Windows.Forms.CheckState.Checked;
        chkAutoScreenshot.Location = new System.Drawing.Point(300, 30);
        chkAutoScreenshot.Name = "chkAutoScreenshot";
        chkAutoScreenshot.Size = new System.Drawing.Size(219, 19);
        chkAutoScreenshot.TabIndex = 2;
        chkAutoScreenshot.Text = "导航后自动截图（保存至默认目录）";
        chkAutoScreenshot.UseVisualStyleBackColor = true;

        // btnStartLogging
        btnStartLogging.Location = new System.Drawing.Point(15, 60);
        btnStartLogging.Name = "btnStartLogging";
        btnStartLogging.Size = new System.Drawing.Size(120, 27);
        btnStartLogging.TabIndex = 3;
        btnStartLogging.Text = "开始日志";
        btnStartLogging.UseVisualStyleBackColor = true;
        btnStartLogging.Click += btnStartLogging_Click;

        // btnStopLogging
        btnStopLogging.Enabled = false;
        btnStopLogging.Location = new System.Drawing.Point(145, 60);
        btnStopLogging.Name = "btnStopLogging";
        btnStopLogging.Size = new System.Drawing.Size(120, 27);
        btnStopLogging.TabIndex = 4;
        btnStopLogging.Text = "停止日志";
        btnStopLogging.UseVisualStyleBackColor = true;
        btnStopLogging.Click += btnStopLogging_Click;

        // btnSaveSnapshot
        btnSaveSnapshot.Location = new System.Drawing.Point(275, 60);
        btnSaveSnapshot.Name = "btnSaveSnapshot";
        btnSaveSnapshot.Size = new System.Drawing.Size(180, 27);
        btnSaveSnapshot.TabIndex = 5;
        btnSaveSnapshot.Text = "保存快照 (HTML+PNG+JSON)";
        btnSaveSnapshot.UseVisualStyleBackColor = true;
        btnSaveSnapshot.Click += btnSaveSnapshot_Click;

        // grpNetwork
        grpNetwork.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
        grpNetwork.Controls.Add(lblProxy);
        grpNetwork.Controls.Add(txtProxy);
        grpNetwork.Controls.Add(chkIgnoreTls);
        grpNetwork.Location = new System.Drawing.Point(10, 315);
        grpNetwork.Name = "grpNetwork";
        grpNetwork.Size = new System.Drawing.Size(430, 100);
        grpNetwork.TabIndex = 3;
        grpNetwork.TabStop = false;
        grpNetwork.Text = "网络设置";

        // lblProxy
        lblProxy.AutoSize = true;
        lblProxy.Location = new System.Drawing.Point(15, 30);
        lblProxy.Name = "lblProxy";
        lblProxy.Size = new System.Drawing.Size(121, 15);
        lblProxy.TabIndex = 0;
        lblProxy.Text = "代理 (--proxy-server)：";

        // txtProxy
        txtProxy.Location = new System.Drawing.Point(142, 26);
        txtProxy.Name = "txtProxy";
        txtProxy.Size = new System.Drawing.Size(270, 23);
        txtProxy.TabIndex = 1;

        // chkIgnoreTls
        chkIgnoreTls.AutoSize = true;
        chkIgnoreTls.Location = new System.Drawing.Point(18, 65);
        chkIgnoreTls.Name = "chkIgnoreTls";
        chkIgnoreTls.Size = new System.Drawing.Size(217, 19);
        chkIgnoreTls.TabIndex = 2;
        chkIgnoreTls.Text = "忽略 TLS 证书错误 (IgnoreHTTPSErrors)";
        chkIgnoreTls.UseVisualStyleBackColor = true;

        // grpScripts
        grpScripts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpScripts.Controls.Add(chkInitScript);
        grpScripts.Controls.Add(txtInitScript);
        grpScripts.Controls.Add(chkPostNavScript);
        grpScripts.Controls.Add(txtPostNavScript);
        grpScripts.Location = new System.Drawing.Point(460, 315);
        grpScripts.Name = "grpScripts";
        grpScripts.Size = new System.Drawing.Size(430, 150);
        grpScripts.TabIndex = 4;
        grpScripts.TabStop = false;
        grpScripts.Text = "脚本注入";

        // chkInitScript
        chkInitScript.AutoSize = true;
        chkInitScript.Location = new System.Drawing.Point(18, 30);
        chkInitScript.Name = "chkInitScript";
        chkInitScript.Size = new System.Drawing.Size(202, 19);
        chkInitScript.TabIndex = 0;
        chkInitScript.Text = "AddInitScript（所有页面自动注入）";
        chkInitScript.UseVisualStyleBackColor = true;

        // txtInitScript
        txtInitScript.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtInitScript.Location = new System.Drawing.Point(18, 55);
        txtInitScript.Name = "txtInitScript";
        txtInitScript.Size = new System.Drawing.Size(394, 23);
        txtInitScript.TabIndex = 1;

        // chkPostNavScript
        chkPostNavScript.AutoSize = true;
        chkPostNavScript.Location = new System.Drawing.Point(18, 90);
        chkPostNavScript.Name = "chkPostNavScript";
        chkPostNavScript.Size = new System.Drawing.Size(138, 19);
        chkPostNavScript.TabIndex = 2;
        chkPostNavScript.Text = "导航后执行 Evaluate";
        chkPostNavScript.UseVisualStyleBackColor = true;

        // txtPostNavScript
        txtPostNavScript.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtPostNavScript.Location = new System.Drawing.Point(18, 115);
        txtPostNavScript.Name = "txtPostNavScript";
        txtPostNavScript.Size = new System.Drawing.Size(394, 23);
        txtPostNavScript.TabIndex = 3;

        // grpAdvanced
        grpAdvanced.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
        grpAdvanced.Controls.Add(chkExposeDotnet);
        grpAdvanced.Controls.Add(txtExposeName);
        grpAdvanced.Controls.Add(lblRetry);
        grpAdvanced.Controls.Add(numRetryCount);
        grpAdvanced.Controls.Add(numRetryDelayMs);
        grpAdvanced.Location = new System.Drawing.Point(10, 425);
        grpAdvanced.Name = "grpAdvanced";
        grpAdvanced.Size = new System.Drawing.Size(430, 120);
        grpAdvanced.TabIndex = 5;
        grpAdvanced.TabStop = false;
        grpAdvanced.Text = "高级选项";

        // chkExposeDotnet
        chkExposeDotnet.AutoSize = true;
        chkExposeDotnet.Location = new System.Drawing.Point(18, 30);
        chkExposeDotnet.Name = "chkExposeDotnet";
        chkExposeDotnet.Size = new System.Drawing.Size(220, 19);
        chkExposeDotnet.TabIndex = 0;
        chkExposeDotnet.Text = "暴露 .NET 方法到页面 (ExposeFunction)";
        chkExposeDotnet.UseVisualStyleBackColor = true;

        // txtExposeName
        txtExposeName.Location = new System.Drawing.Point(244, 28);
        txtExposeName.Name = "txtExposeName";
        txtExposeName.Size = new System.Drawing.Size(170, 23);
        txtExposeName.TabIndex = 1;
        txtExposeName.Text = "dotnetPing";

        // lblRetry
        lblRetry.AutoSize = true;
        lblRetry.Location = new System.Drawing.Point(18, 75);
        lblRetry.Name = "lblRetry";
        lblRetry.Size = new System.Drawing.Size(95, 15);
        lblRetry.TabIndex = 2;
        lblRetry.Text = "重试 (次 / 毫秒)：";

        // numRetryCount
        numRetryCount.Location = new System.Drawing.Point(119, 73);
        numRetryCount.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        numRetryCount.Name = "numRetryCount";
        numRetryCount.Size = new System.Drawing.Size(60, 23);
        numRetryCount.TabIndex = 3;
        numRetryCount.Value = new decimal(new int[] { 2, 0, 0, 0 });

        // numRetryDelayMs
        numRetryDelayMs.Increment = new decimal(new int[] { 250, 0, 0, 0 });
        numRetryDelayMs.Location = new System.Drawing.Point(185, 73);
        numRetryDelayMs.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
        numRetryDelayMs.Name = "numRetryDelayMs";
        numRetryDelayMs.Size = new System.Drawing.Size(80, 23);
        numRetryDelayMs.TabIndex = 4;
        numRetryDelayMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });

        // grpUtilities
        grpUtilities.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpUtilities.Controls.Add(btnExportJsonList);
        grpUtilities.Controls.Add(btnExportJsonProtocol);
        grpUtilities.Controls.Add(btnRefreshPages);
        grpUtilities.Controls.Add(txtNewTabUrl);
        grpUtilities.Controls.Add(btnOpenNewTab);
        grpUtilities.Location = new System.Drawing.Point(460, 475);
        grpUtilities.Name = "grpUtilities";
        grpUtilities.Size = new System.Drawing.Size(430, 120);
        grpUtilities.TabIndex = 6;
        grpUtilities.TabStop = false;
        grpUtilities.Text = "实用工具";

        // btnExportJsonList
        btnExportJsonList.Location = new System.Drawing.Point(18, 30);
        btnExportJsonList.Name = "btnExportJsonList";
        btnExportJsonList.Size = new System.Drawing.Size(150, 30);
        btnExportJsonList.TabIndex = 0;
        btnExportJsonList.Text = "导出 /json/list";
        btnExportJsonList.UseVisualStyleBackColor = true;
        btnExportJsonList.Click += btnExportJsonList_Click;

        // btnExportJsonProtocol
        btnExportJsonProtocol.Location = new System.Drawing.Point(180, 30);
        btnExportJsonProtocol.Name = "btnExportJsonProtocol";
        btnExportJsonProtocol.Size = new System.Drawing.Size(150, 30);
        btnExportJsonProtocol.TabIndex = 1;
        btnExportJsonProtocol.Text = "导出 /json/protocol";
        btnExportJsonProtocol.UseVisualStyleBackColor = true;
        btnExportJsonProtocol.Click += btnExportJsonProtocol_Click;

        // btnRefreshPages
        btnRefreshPages.Location = new System.Drawing.Point(18, 75);
        btnRefreshPages.Name = "btnRefreshPages";
        btnRefreshPages.Size = new System.Drawing.Size(150, 30);
        btnRefreshPages.TabIndex = 2;
        btnRefreshPages.Text = "刷新页面列表";
        btnRefreshPages.UseVisualStyleBackColor = true;
        btnRefreshPages.Click += btnRefreshPages_Click;

        // txtNewTabUrl
        txtNewTabUrl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtNewTabUrl.Location = new System.Drawing.Point(180, 80);
        txtNewTabUrl.Name = "txtNewTabUrl";
        txtNewTabUrl.Size = new System.Drawing.Size(160, 23);
        txtNewTabUrl.TabIndex = 3;
        txtNewTabUrl.Text = "https://example.com";

        // btnOpenNewTab
        btnOpenNewTab.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnOpenNewTab.Location = new System.Drawing.Point(350, 78);
        btnOpenNewTab.Name = "btnOpenNewTab";
        btnOpenNewTab.Size = new System.Drawing.Size(70, 27);
        btnOpenNewTab.TabIndex = 4;
        btnOpenNewTab.Text = "新开 Tab";
        btnOpenNewTab.UseVisualStyleBackColor = true;
        btnOpenNewTab.Click += btnOpenNewTab_Click;

        // grpPagesLog
        grpPagesLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        grpPagesLog.Controls.Add(txtLog);
        grpPagesLog.Controls.Add(lblLog);
        grpPagesLog.Controls.Add(lstPages);
        grpPagesLog.Controls.Add(lblPages);
        grpPagesLog.Location = new System.Drawing.Point(300, 555);
        grpPagesLog.Name = "grpPagesLog";
        grpPagesLog.Size = new System.Drawing.Size(590, 205);
        grpPagesLog.TabIndex = 7;
        grpPagesLog.TabStop = false;
        grpPagesLog.Text = "页面与日志";

        // lblPages
        lblPages.AutoSize = true;
        lblPages.Location = new System.Drawing.Point(15, 30);
        lblPages.Name = "lblPages";
        lblPages.Size = new System.Drawing.Size(80, 15);
        lblPages.TabIndex = 0;
        lblPages.Text = "页面 (Pages)：";

        // lstPages
        lstPages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lstPages.FormattingEnabled = true;
        lstPages.ItemHeight = 15;
        lstPages.Location = new System.Drawing.Point(15, 50);
        lstPages.Name = "lstPages";
        lstPages.Size = new System.Drawing.Size(560, 79);
        lstPages.TabIndex = 1;
        lstPages.SelectedIndexChanged += lstPages_SelectedIndexChanged;

        // lblLog
        lblLog.AutoSize = true;
        lblLog.Location = new System.Drawing.Point(15, 135);
        lblLog.Name = "lblLog";
        lblLog.Size = new System.Drawing.Size(68, 15);
        lblLog.TabIndex = 2;
        lblLog.Text = "日志输出：";

        // txtLog
        txtLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtLog.Location = new System.Drawing.Point(15, 155);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        txtLog.Size = new System.Drawing.Size(560, 45);
        txtLog.TabIndex = 3;
        txtLog.WordWrap = false;

        // MainForm
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(900, 760);
        Controls.Add(grpPagesLog);
        Controls.Add(grpUtilities);
        Controls.Add(grpAdvanced);
        Controls.Add(grpScripts);
        Controls.Add(grpNetwork);
        Controls.Add(grpAutomation);
        Controls.Add(grpWorkflow);
        Controls.Add(grpConnection);
        Name = "MainForm";
        Text = "External Browser (CDP) Launcher - WinForms";
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).EndInit();
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        grpWorkflow.ResumeLayout(false);
        grpAutomation.ResumeLayout(false);
        grpAutomation.PerformLayout();
        grpNetwork.ResumeLayout(false);
        grpNetwork.PerformLayout();
        grpScripts.ResumeLayout(false);
        grpScripts.PerformLayout();
        grpAdvanced.ResumeLayout(false);
        grpAdvanced.PerformLayout();
        grpUtilities.ResumeLayout(false);
        grpUtilities.PerformLayout();
        grpPagesLog.ResumeLayout(false);
        grpPagesLog.PerformLayout();
        ResumeLayout(false);
    }
}
