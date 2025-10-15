using System.Drawing;
using System.Windows.Forms;

namespace ExternalBrowserWinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var splitContainer = new SplitContainer();
        var leftPanel = new FlowLayoutPanel();
        var grpLaunch = new GroupBox();
        var launchLayout = new TableLayoutPanel();
        var lblExe = new Label();
        txtExePath = new TextBox();
        btnBrowseExe = new Button();
        var lblUserData = new Label();
        txtUserDataDir = new TextBox();
        btnPickUserData = new Button();
        var lblPort = new Label();
        numPort = new NumericUpDown();
        var lblProxy = new Label();
        txtProxy = new TextBox();
        var lblStartUrl = new Label();
        txtStartUrl = new TextBox();
        btnLaunch = new Button();
        btnWaitDevTools = new Button();
        btnConnect = new Button();
        btnNewPage = new Button();
        btnGoto = new Button();
        btnRunAll = new Button();
        btnResetRunAll = new Button();
        btnCloseAll = new Button();
        var grpOptions = new GroupBox();
        var optionsLayout = new TableLayoutPanel();
        chkIgnoreTls = new CheckBox();
        chkInitScript = new CheckBox();
        txtInitScript = new TextBox();
        chkExposeDotnet = new CheckBox();
        txtExposeName = new TextBox();
        chkPostNavScript = new CheckBox();
        txtPostNavScript = new TextBox();
        chkAutoScreenshot = new CheckBox();
        var lblScreenshot = new Label();
        txtScreenshotPath = new TextBox();
        btnPickScreenshot = new Button();
        var lblDownload = new Label();
        txtDownloadDir = new TextBox();
        btnPickDownloadDir = new Button();
        var lblRetryCount = new Label();
        numRetryCount = new NumericUpDown();
        var lblRetryDelay = new Label();
        numRetryDelayMs = new NumericUpDown();
        var grpPages = new GroupBox();
        var pagesLayout = new TableLayoutPanel();
        lstPages = new ListBox();
        btnRefreshPages = new Button();
        btnOpenNewTab = new Button();
        var lblNewTabUrl = new Label();
        txtNewTabUrl = new TextBox();
        var grpLogging = new GroupBox();
        var loggingLayout = new FlowLayoutPanel();
        btnStartLogging = new Button();
        btnStopLogging = new Button();
        btnSaveSnapshot = new Button();
        btnExportJsonList = new Button();
        btnExportJsonProtocol = new Button();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        leftPanel.SuspendLayout();
        grpLaunch.SuspendLayout();
        launchLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        grpOptions.SuspendLayout();
        optionsLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).BeginInit();
        grpPages.SuspendLayout();
        pagesLayout.SuspendLayout();
        grpLogging.SuspendLayout();
        loggingLayout.SuspendLayout();
        SuspendLayout();
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Name = "splitContainer";
        splitContainer.SplitterDistance = 420;
        splitContainer.TabIndex = 0;
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(leftPanel);
        // 
        // splitContainer.Panel2
        // 
        txtLog = new TextBox();
        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.ReadOnly = true;
        splitContainer.Panel2.Controls.Add(txtLog);
        // 
        // leftPanel
        // 
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.FlowDirection = FlowDirection.TopDown;
        leftPanel.WrapContents = false;
        leftPanel.AutoScroll = true;
        leftPanel.Controls.Add(grpLaunch);
        leftPanel.Controls.Add(grpOptions);
        leftPanel.Controls.Add(grpPages);
        leftPanel.Controls.Add(grpLogging);
        // 
        // grpLaunch
        // 
        grpLaunch.Text = "启动";
        grpLaunch.AutoSize = true;
        grpLaunch.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpLaunch.Controls.Add(launchLayout);
        // 
        // launchLayout
        // 
        launchLayout.ColumnCount = 3;
        launchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        launchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        launchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        launchLayout.RowCount = 10;
        for (var i = 0; i < launchLayout.RowCount; i++)
        {
            launchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        launchLayout.Dock = DockStyle.Fill;
        launchLayout.Controls.Add(lblExe, 0, 0);
        launchLayout.Controls.Add(txtExePath, 1, 0);
        launchLayout.Controls.Add(btnBrowseExe, 2, 0);
        launchLayout.Controls.Add(lblUserData, 0, 1);
        launchLayout.Controls.Add(txtUserDataDir, 1, 1);
        launchLayout.Controls.Add(btnPickUserData, 2, 1);
        launchLayout.Controls.Add(lblPort, 0, 2);
        launchLayout.Controls.Add(numPort, 1, 2);
        launchLayout.Controls.Add(lblProxy, 0, 3);
        launchLayout.Controls.Add(txtProxy, 1, 3);
        launchLayout.Controls.Add(lblStartUrl, 0, 4);
        launchLayout.Controls.Add(txtStartUrl, 1, 4);
        launchLayout.SetColumnSpan(txtStartUrl, 2);
        launchLayout.Controls.Add(btnLaunch, 0, 5);
        launchLayout.SetColumnSpan(btnLaunch, 3);
        launchLayout.Controls.Add(btnWaitDevTools, 0, 6);
        launchLayout.SetColumnSpan(btnWaitDevTools, 3);
        launchLayout.Controls.Add(btnConnect, 0, 7);
        launchLayout.SetColumnSpan(btnConnect, 3);
        var flowLaunchActions = new FlowLayoutPanel();
        flowLaunchActions.AutoSize = true;
        flowLaunchActions.Controls.Add(btnNewPage);
        flowLaunchActions.Controls.Add(btnGoto);
        flowLaunchActions.Controls.Add(btnCloseAll);
        launchLayout.Controls.Add(flowLaunchActions, 0, 8);
        launchLayout.SetColumnSpan(flowLaunchActions, 3);
        var flowRunAll = new FlowLayoutPanel();
        flowRunAll.AutoSize = true;
        flowRunAll.Controls.Add(btnRunAll);
        flowRunAll.Controls.Add(btnResetRunAll);
        launchLayout.Controls.Add(flowRunAll, 0, 9);
        launchLayout.SetColumnSpan(flowRunAll, 3);
        // 
        // lblExe
        // 
        lblExe.AutoSize = true;
        lblExe.Text = "EXE";
        // 
        // txtExePath
        // 
        txtExePath.Dock = DockStyle.Fill;
        // 
        // btnBrowseExe
        // 
        btnBrowseExe.Text = "浏览";
        btnBrowseExe.Click += btnBrowseExe_Click;
        // 
        // lblUserData
        // 
        lblUserData.AutoSize = true;
        lblUserData.Text = "UserData";
        // 
        // txtUserDataDir
        // 
        txtUserDataDir.Dock = DockStyle.Fill;
        // 
        // btnPickUserData
        // 
        btnPickUserData.Text = "选择";
        btnPickUserData.Click += btnPickUserData_Click;
        // 
        // lblPort
        // 
        lblPort.AutoSize = true;
        lblPort.Text = "端口";
        // 
        // numPort
        // 
        numPort.Dock = DockStyle.Fill;
        numPort.Maximum = 65535;
        numPort.Minimum = 1;
        numPort.Value = 9222;
        // 
        // lblProxy
        // 
        lblProxy.AutoSize = true;
        lblProxy.Text = "代理";
        // 
        // txtProxy
        // 
        txtProxy.Dock = DockStyle.Fill;
        launchLayout.SetColumnSpan(txtProxy, 2);
        // 
        // lblStartUrl
        // 
        lblStartUrl.AutoSize = true;
        lblStartUrl.Text = "起始 URL";
        // 
        // txtStartUrl
        // 
        txtStartUrl.Dock = DockStyle.Fill;
        // 
        // btnLaunch
        // 
        btnLaunch.Text = "启动进程";
        btnLaunch.Click += btnLaunch_Click;
        // 
        // btnWaitDevTools
        // 
        btnWaitDevTools.Text = "等待 DevTools";
        btnWaitDevTools.Enabled = false;
        btnWaitDevTools.Click += btnWaitDevTools_Click;
        // 
        // btnConnect
        // 
        btnConnect.Text = "连接";
        btnConnect.Enabled = false;
        btnConnect.Click += btnConnect_Click;
        // 
        // btnNewPage
        // 
        btnNewPage.Text = "新建页面";
        btnNewPage.Enabled = false;
        btnNewPage.Click += btnNewPage_Click;
        // 
        // btnGoto
        // 
        btnGoto.Text = "导航";
        btnGoto.Enabled = false;
        btnGoto.Click += btnGoto_Click;
        // 
        // btnCloseAll
        // 
        btnCloseAll.Text = "关闭所有";
        btnCloseAll.Enabled = false;
        btnCloseAll.Click += btnCloseAll_Click;
        // 
        // btnRunAll
        // 
        btnRunAll.Text = "一键运行";
        btnRunAll.Click += btnRunAll_Click;
        // 
        // btnResetRunAll
        // 
        btnResetRunAll.Text = "重置并运行";
        btnResetRunAll.Click += btnResetRunAll_Click;
        // 
        // grpOptions
        // 
        grpOptions.Text = "选项";
        grpOptions.AutoSize = true;
        grpOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpOptions.Controls.Add(optionsLayout);
        // 
        // optionsLayout
        // 
        optionsLayout.ColumnCount = 3;
        optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        optionsLayout.RowCount = 11;
        for (var i = 0; i < optionsLayout.RowCount; i++)
        {
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        optionsLayout.Dock = DockStyle.Fill;
        optionsLayout.Controls.Add(chkIgnoreTls, 0, 0);
        optionsLayout.SetColumnSpan(chkIgnoreTls, 3);
        optionsLayout.Controls.Add(chkInitScript, 0, 1);
        optionsLayout.SetColumnSpan(chkInitScript, 3);
        optionsLayout.Controls.Add(txtInitScript, 0, 2);
        optionsLayout.SetColumnSpan(txtInitScript, 3);
        optionsLayout.Controls.Add(chkExposeDotnet, 0, 3);
        optionsLayout.Controls.Add(txtExposeName, 1, 3);
        optionsLayout.Controls.Add(chkPostNavScript, 0, 4);
        optionsLayout.SetColumnSpan(chkPostNavScript, 3);
        optionsLayout.Controls.Add(txtPostNavScript, 0, 5);
        optionsLayout.SetColumnSpan(txtPostNavScript, 3);
        optionsLayout.Controls.Add(chkAutoScreenshot, 0, 6);
        optionsLayout.SetColumnSpan(chkAutoScreenshot, 3);
        optionsLayout.Controls.Add(lblScreenshot, 0, 7);
        optionsLayout.Controls.Add(txtScreenshotPath, 1, 7);
        optionsLayout.Controls.Add(btnPickScreenshot, 2, 7);
        optionsLayout.Controls.Add(lblDownload, 0, 8);
        optionsLayout.Controls.Add(txtDownloadDir, 1, 8);
        optionsLayout.Controls.Add(btnPickDownloadDir, 2, 8);
        optionsLayout.Controls.Add(lblRetryCount, 0, 9);
        optionsLayout.Controls.Add(numRetryCount, 1, 9);
        optionsLayout.Controls.Add(lblRetryDelay, 0, 10);
        optionsLayout.Controls.Add(numRetryDelayMs, 1, 10);
        // 
        // chkIgnoreTls
        // 
        chkIgnoreTls.AutoSize = true;
        chkIgnoreTls.Text = "忽略 TLS 错误";
        // 
        // chkInitScript
        // 
        chkInitScript.AutoSize = true;
        chkInitScript.Text = "注册 Init Script";
        // 
        // txtInitScript
        // 
        txtInitScript.Multiline = true;
        txtInitScript.Height = 60;
        txtInitScript.ScrollBars = ScrollBars.Vertical;
        // 
        // chkExposeDotnet
        // 
        chkExposeDotnet.AutoSize = true;
        chkExposeDotnet.Text = "暴露 .NET 方法";
        // 
        // txtExposeName
        // 
        txtExposeName.Dock = DockStyle.Fill;
        // 
        // chkPostNavScript
        // 
        chkPostNavScript.AutoSize = true;
        chkPostNavScript.Text = "导航后执行脚本";
        // 
        // txtPostNavScript
        // 
        txtPostNavScript.Multiline = true;
        txtPostNavScript.Height = 60;
        txtPostNavScript.ScrollBars = ScrollBars.Vertical;
        // 
        // chkAutoScreenshot
        // 
        chkAutoScreenshot.AutoSize = true;
        chkAutoScreenshot.Text = "自动截图";
        // 
        // lblScreenshot
        // 
        lblScreenshot.AutoSize = true;
        lblScreenshot.Text = "截图路径";
        // 
        // txtScreenshotPath
        // 
        txtScreenshotPath.Dock = DockStyle.Fill;
        // 
        // btnPickScreenshot
        // 
        btnPickScreenshot.Text = "选择";
        btnPickScreenshot.Click += btnPickScreenshot_Click;
        // 
        // lblDownload
        // 
        lblDownload.AutoSize = true;
        lblDownload.Text = "下载目录";
        // 
        // txtDownloadDir
        // 
        txtDownloadDir.Dock = DockStyle.Fill;
        // 
        // btnPickDownloadDir
        // 
        btnPickDownloadDir.Text = "选择";
        btnPickDownloadDir.Click += btnPickDownloadDir_Click;
        // 
        // lblRetryCount
        // 
        lblRetryCount.AutoSize = true;
        lblRetryCount.Text = "重试次数";
        // 
        // numRetryCount
        // 
        numRetryCount.Dock = DockStyle.Fill;
        numRetryCount.Minimum = 0;
        numRetryCount.Maximum = 10;
        // 
        // lblRetryDelay
        // 
        lblRetryDelay.AutoSize = true;
        lblRetryDelay.Text = "重试间隔(ms)";
        // 
        // numRetryDelayMs
        // 
        numRetryDelayMs.Dock = DockStyle.Fill;
        numRetryDelayMs.Maximum = 60000;
        numRetryDelayMs.Value = 1000;
        // 
        // grpPages
        // 
        grpPages.Text = "页面";
        grpPages.AutoSize = true;
        grpPages.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpPages.Controls.Add(pagesLayout);
        // 
        // pagesLayout
        // 
        pagesLayout.ColumnCount = 2;
        pagesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pagesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        pagesLayout.RowCount = 4;
        for (var i = 0; i < pagesLayout.RowCount; i++)
        {
            pagesLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        pagesLayout.Dock = DockStyle.Fill;
        pagesLayout.Controls.Add(lstPages, 0, 0);
        pagesLayout.SetColumnSpan(lstPages, 2);
        lstPages.Dock = DockStyle.Fill;
        lstPages.Height = 120;
        lstPages.SelectedIndexChanged += lstPages_SelectedIndexChanged;
        pagesLayout.Controls.Add(btnRefreshPages, 0, 1);
        btnRefreshPages.Text = "刷新";
        btnRefreshPages.Click += btnRefreshPages_Click;
        pagesLayout.Controls.Add(btnOpenNewTab, 1, 1);
        btnOpenNewTab.Text = "打开新标签";
        btnOpenNewTab.Click += btnOpenNewTab_Click;
        pagesLayout.Controls.Add(lblNewTabUrl, 0, 2);
        lblNewTabUrl.AutoSize = true;
        lblNewTabUrl.Text = "新标签 URL";
        pagesLayout.Controls.Add(txtNewTabUrl, 0, 3);
        pagesLayout.SetColumnSpan(txtNewTabUrl, 2);
        txtNewTabUrl.Dock = DockStyle.Fill;
        // 
        // grpLogging
        // 
        grpLogging.Text = "日志与快照";
        grpLogging.AutoSize = true;
        grpLogging.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpLogging.Controls.Add(loggingLayout);
        loggingLayout.AutoSize = true;
        loggingLayout.FlowDirection = FlowDirection.LeftToRight;
        loggingLayout.WrapContents = true;
        loggingLayout.Controls.Add(btnStartLogging);
        loggingLayout.Controls.Add(btnStopLogging);
        loggingLayout.Controls.Add(btnSaveSnapshot);
        loggingLayout.Controls.Add(btnExportJsonList);
        loggingLayout.Controls.Add(btnExportJsonProtocol);
        btnStartLogging.Text = "开始日志";
        btnStartLogging.Click += btnStartLogging_Click;
        btnStopLogging.Text = "停止日志";
        btnStopLogging.Enabled = false;
        btnStopLogging.Click += btnStopLogging_Click;
        btnSaveSnapshot.Text = "保存快照";
        btnSaveSnapshot.Click += btnSaveSnapshot_Click;
        btnExportJsonList.Text = "/json/list";
        btnExportJsonList.Click += btnExportJsonList_Click;
        btnExportJsonProtocol.Text = "/json/protocol";
        btnExportJsonProtocol.Click += btnExportJsonProtocol_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(splitContainer);
        Text = "External Browser Controller";
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        splitContainer.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        leftPanel.PerformLayout();
        grpLaunch.ResumeLayout(false);
        grpLaunch.PerformLayout();
        launchLayout.ResumeLayout(false);
        launchLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        optionsLayout.ResumeLayout(false);
        optionsLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numRetryCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRetryDelayMs).EndInit();
        grpPages.ResumeLayout(false);
        grpPages.PerformLayout();
        pagesLayout.ResumeLayout(false);
        pagesLayout.PerformLayout();
        grpLogging.ResumeLayout(false);
        grpLogging.PerformLayout();
        loggingLayout.ResumeLayout(false);
        loggingLayout.PerformLayout();
        ResumeLayout(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private TextBox txtExePath = null!;
    private Button btnBrowseExe = null!;
    private TextBox txtUserDataDir = null!;
    private Button btnPickUserData = null!;
    private NumericUpDown numPort = null!;
    private TextBox txtProxy = null!;
    private TextBox txtStartUrl = null!;
    private Button btnLaunch = null!;
    private Button btnWaitDevTools = null!;
    private Button btnConnect = null!;
    private Button btnNewPage = null!;
    private Button btnGoto = null!;
    private Button btnCloseAll = null!;
    private Button btnRunAll = null!;
    private Button btnResetRunAll = null!;
    private CheckBox chkIgnoreTls = null!;
    private CheckBox chkInitScript = null!;
    private TextBox txtInitScript = null!;
    private CheckBox chkExposeDotnet = null!;
    private TextBox txtExposeName = null!;
    private CheckBox chkPostNavScript = null!;
    private TextBox txtPostNavScript = null!;
    private CheckBox chkAutoScreenshot = null!;
    private TextBox txtScreenshotPath = null!;
    private Button btnPickScreenshot = null!;
    private TextBox txtDownloadDir = null!;
    private Button btnPickDownloadDir = null!;
    private NumericUpDown numRetryCount = null!;
    private NumericUpDown numRetryDelayMs = null!;
    private ListBox lstPages = null!;
    private Button btnRefreshPages = null!;
    private Button btnOpenNewTab = null!;
    private TextBox txtNewTabUrl = null!;
    private Button btnStartLogging = null!;
    private Button btnStopLogging = null!;
    private Button btnSaveSnapshot = null!;
    private Button btnExportJsonList = null!;
    private Button btnExportJsonProtocol = null!;
    private TextBox txtLog = null!;
}
