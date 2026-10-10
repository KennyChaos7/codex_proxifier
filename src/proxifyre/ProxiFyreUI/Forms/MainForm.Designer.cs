using System.Drawing;
using System.Windows.Forms;
using ProxiFyreUI.Infrastructure;

namespace ProxiFyreUI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components;
        private TableLayoutPanel settingsPanel, targetsPanel;
        private TextBox endpointTextBox, usernameTextBox, passwordTextBox, logTextBox;
        private CheckBox authenticationCheckBox, bypassLanCheckBox, minimizeToTrayCheckBox;
        private ComboBox logLevelComboBox;
        private ListBox targetsListBox;
        private Label titleLabel, serviceStateValueLabel, changesStateLabel, stateDetailLabel, cliPathLabel;
        private Button startServiceButton, stopServiceButton, applyButton, selectCliButton, refreshTargetsButton;
        private Timer serviceRefreshTimer;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            serviceRefreshTimer = new Timer(components) { Interval = 2000 };
            SuspendLayout();
            Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 10F);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(940, 680);
            MinimumSize = new Size(820, 640);
            Text = "Codex Proxy";
            BackColor = Color.FromArgb(245, 247, 250);
            StartPosition = FormStartPosition.CenterScreen;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(24) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 2, Margin = new Padding(0, 0, 0, 20) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            titleLabel = Label("titleLabel", "Codex 专用代理");
            titleLabel.Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 20F, FontStyle.Bold);
            header.Controls.Add(titleLabel, 0, 0);
            serviceStateValueLabel = Label("serviceStateValueLabel", "等待服务状态");
            serviceStateValueLabel.Anchor = AnchorStyles.Right;
            header.Controls.Add(serviceStateValueLabel, 1, 0);
            header.Controls.Add(Label("subtitleLabel", "桌面应用 · 专用后台进程 · 原生 CLI"), 0, 1);
            changesStateLabel = Label("changesStateLabel", "");
            changesStateLabel.Anchor = AnchorStyles.Right;
            header.Controls.Add(changesStateLabel, 1, 1);
            root.Controls.Add(header, 0, 0);
            var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            settingsPanel = new TableLayoutPanel { Name = "settingsPanel", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, BackColor = Color.White, Padding = new Padding(16), Margin = new Padding(0, 0, 10, 0) };
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 8; i++) settingsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            settingsPanel.Controls.Add(Label("proxyTitle", "SOCKS5 代理"), 0, 0);
            endpointTextBox = Input("endpointTextBox", "127.0.0.1:7890");
            settingsPanel.Controls.Add(Label("endpointLabel", "地址"), 0, 1); settingsPanel.Controls.Add(endpointTextBox, 1, 1);
            authenticationCheckBox = Check("authenticationCheckBox", "使用用户名和密码", false);
            settingsPanel.Controls.Add(authenticationCheckBox, 0, 2); settingsPanel.SetColumnSpan(authenticationCheckBox, 2);
            usernameTextBox = Input("usernameTextBox", ""); passwordTextBox = Input("passwordTextBox", ""); passwordTextBox.UseSystemPasswordChar = true;
            settingsPanel.Controls.Add(Label("usernameLabel", "用户名"), 0, 3); settingsPanel.Controls.Add(usernameTextBox, 1, 3);
            settingsPanel.Controls.Add(Label("passwordLabel", "密码"), 0, 4); settingsPanel.Controls.Add(passwordTextBox, 1, 4);
            bypassLanCheckBox = Check("bypassLanCheckBox", "绕过局域网", true);
            settingsPanel.Controls.Add(bypassLanCheckBox, 0, 5); settingsPanel.SetColumnSpan(bypassLanCheckBox, 2);
            logLevelComboBox = new ComboBox { Name = "logLevelComboBox", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            logLevelComboBox.Items.AddRange(new object[] { "Error", "Warning", "Info", "Debug", "All" }); logLevelComboBox.SelectedItem = "Info";
            settingsPanel.Controls.Add(Label("logLevelLabel", "日志级别"), 0, 6); settingsPanel.Controls.Add(logLevelComboBox, 1, 6);
            cards.Controls.Add(settingsPanel, 0, 0);
            targetsPanel = new TableLayoutPanel { Name = "targetsPanel", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.White, Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
            targetsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); targetsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); targetsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); targetsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            targetsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            targetsPanel.Controls.Add(Label("targetsTitle", "Codex 路径"), 0, 0);
            targetsListBox = new ListBox { Name = "targetsListBox", Dock = DockStyle.Fill, HorizontalScrollbar = true, IntegralHeight = false, BorderStyle = BorderStyle.None };
            targetsPanel.Controls.Add(targetsListBox, 0, 1);
            cliPathLabel = Label("cliPathLabel", "自动识别；可选择其他 codex.exe");
            targetsPanel.Controls.Add(cliPathLabel, 0, 2);
            var pathButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            selectCliButton = Button("selectCliButton", "选择 CLI 路径"); refreshTargetsButton = Button("refreshTargetsButton", "重新识别");
            pathButtons.Controls.Add(selectCliButton); pathButtons.Controls.Add(refreshTargetsButton); targetsPanel.Controls.Add(pathButtons, 0, 3);
            cards.Controls.Add(targetsPanel, 1, 0); root.Controls.Add(cards, 0, 1);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 18, 0, 10) };
            startServiceButton = Button("startServiceButton", "启动代理"); startServiceButton.BackColor = Color.FromArgb(52, 112, 230); startServiceButton.ForeColor = Color.White;
            stopServiceButton = Button("stopServiceButton", "停止代理"); applyButton = Button("applyButton", "保存并应用");
            actions.Controls.Add(startServiceButton); actions.Controls.Add(stopServiceButton); actions.Controls.Add(applyButton);
            stateDetailLabel = Label("stateDetailLabel", ""); actions.Controls.Add(stateDetailLabel); root.Controls.Add(actions, 0, 2);
            root.Controls.Add(Label("logsLabel", "运行日志"), 0, 3);
            logTextBox = new TextBox { Name = "logTextBox", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            root.Controls.Add(logTextBox, 0, 4);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            minimizeToTrayCheckBox = Check("minimizeToTrayCheckBox", "关闭窗口时最小化到托盘", true); footer.Controls.Add(minimizeToTrayCheckBox);
            footer.Controls.Add(Label("scopeLabel", "其他应用保持原有网络连接")); root.Controls.Add(footer, 0, 5);
            ResumeLayout(true);
        }
        private static Label Label(string name, string text) { return new Label { Name = name, Text = text, AutoSize = true, Margin = new Padding(3, 6, 3, 6) }; }
        private static TextBox Input(string name, string text) { return new TextBox { Name = name, Text = text, Dock = DockStyle.Fill, Margin = new Padding(3, 5, 3, 5) }; }
        private static CheckBox Check(string name, string text, bool value) { return new CheckBox { Name = name, Text = text, Checked = value, AutoSize = true, Margin = new Padding(3, 8, 3, 8) }; }
        private static Button Button(string name, string text) { return new Button { Name = name, Text = text, AutoSize = true, Padding = new Padding(12, 5, 12, 5), Margin = new Padding(3, 3, 8, 3) }; }
    }
}

