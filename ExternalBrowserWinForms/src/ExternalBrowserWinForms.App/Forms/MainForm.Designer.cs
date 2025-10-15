using System.Drawing;
using System.Windows.Forms;

namespace ExternalBrowserWinForms.App.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtUrl = null!;
    private Label lblUrl = null!;
    private CheckBox chkUseDefaultBrowser = null!;
    private TextBox txtBrowserPath = null!;
    private Button btnBrowse = null!;
    private Label lblBrowserPath = null!;
    private TextBox txtArguments = null!;
    private Label lblArguments = null!;
    private Button btnLaunch = null!;
    private Label lblStatus = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtUrl = new TextBox();
        lblUrl = new Label();
        chkUseDefaultBrowser = new CheckBox();
        txtBrowserPath = new TextBox();
        btnBrowse = new Button();
        lblBrowserPath = new Label();
        txtArguments = new TextBox();
        lblArguments = new Label();
        btnLaunch = new Button();
        lblStatus = new Label();
        SuspendLayout();
        // 
        // lblUrl
        // 
        lblUrl.AutoSize = true;
        lblUrl.Location = new Point(12, 15);
        lblUrl.Name = "lblUrl";
        lblUrl.Size = new Size(59, 15);
        lblUrl.TabIndex = 0;
        lblUrl.Text = "目标地址";
        // 
        // txtUrl
        // 
        txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtUrl.Location = new Point(130, 12);
        txtUrl.Name = "txtUrl";
        txtUrl.PlaceholderText = "https://example.com";
        txtUrl.Size = new Size(428, 23);
        txtUrl.TabIndex = 1;
        // 
        // chkUseDefaultBrowser
        // 
        chkUseDefaultBrowser.AutoSize = true;
        chkUseDefaultBrowser.Location = new Point(130, 50);
        chkUseDefaultBrowser.Name = "chkUseDefaultBrowser";
        chkUseDefaultBrowser.Size = new Size(111, 19);
        chkUseDefaultBrowser.TabIndex = 2;
        chkUseDefaultBrowser.Text = "使用默认浏览器";
        chkUseDefaultBrowser.UseVisualStyleBackColor = true;
        // 
        // lblBrowserPath
        // 
        lblBrowserPath.AutoSize = true;
        lblBrowserPath.Location = new Point(12, 87);
        lblBrowserPath.Name = "lblBrowserPath";
        lblBrowserPath.Size = new Size(92, 15);
        lblBrowserPath.TabIndex = 3;
        lblBrowserPath.Text = "浏览器可执行文件";
        // 
        // txtBrowserPath
        // 
        txtBrowserPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtBrowserPath.Location = new Point(130, 84);
        txtBrowserPath.Name = "txtBrowserPath";
        txtBrowserPath.Size = new Size(347, 23);
        txtBrowserPath.TabIndex = 4;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(483, 83);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(75, 25);
        btnBrowse.TabIndex = 5;
        btnBrowse.Text = "浏览...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += BtnBrowseClick;
        // 
        // lblArguments
        // 
        lblArguments.AutoSize = true;
        lblArguments.Location = new Point(12, 125);
        lblArguments.Name = "lblArguments";
        lblArguments.Size = new Size(80, 15);
        lblArguments.TabIndex = 6;
        lblArguments.Text = "额外命令参数";
        // 
        // txtArguments
        // 
        txtArguments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtArguments.Location = new Point(130, 122);
        txtArguments.Name = "txtArguments";
        txtArguments.Size = new Size(428, 23);
        txtArguments.TabIndex = 7;
        // 
        // btnLaunch
        // 
        btnLaunch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLaunch.Location = new Point(483, 162);
        btnLaunch.Name = "btnLaunch";
        btnLaunch.Size = new Size(75, 30);
        btnLaunch.TabIndex = 8;
        btnLaunch.Text = "启动";
        btnLaunch.UseVisualStyleBackColor = true;
        btnLaunch.Click += BtnLaunchClick;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Location = new Point(12, 169);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(465, 23);
        lblStatus.TabIndex = 9;
        lblStatus.Text = "准备就绪";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(570, 210);
        Controls.Add(lblStatus);
        Controls.Add(btnLaunch);
        Controls.Add(txtArguments);
        Controls.Add(lblArguments);
        Controls.Add(btnBrowse);
        Controls.Add(txtBrowserPath);
        Controls.Add(lblBrowserPath);
        Controls.Add(chkUseDefaultBrowser);
        Controls.Add(txtUrl);
        Controls.Add(lblUrl);
        MinimumSize = new Size(540, 200);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "外部浏览器启动器";
        ResumeLayout(false);
        PerformLayout();
    }
}
