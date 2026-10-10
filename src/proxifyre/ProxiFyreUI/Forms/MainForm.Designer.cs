using System.Drawing;
using System.Windows.Forms;
using ProxiFyreUI.Infrastructure;

namespace ProxiFyreUI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components;
        private TableLayoutPanel settingsPanel, targetsPanel, authenticationFieldsPanel, rootPanel;
        private TextBox endpointTextBox, usernameTextBox, passwordTextBox, logTextBox;
        private CheckBox authenticationCheckBox, bypassLanCheckBox, minimizeToTrayCheckBox;
        private ComboBox logLevelComboBox;
        private ListBox targetsListBox;
        private Label titleLabel, serviceStateValueLabel, changesStateLabel, stateDetailLabel, cliPathLabel, logEmptyLabel, driverLabel;
        private Button startServiceButton, stopServiceButton, applyButton, selectCliButton, refreshTargetsButton, collapseLogsButton;
        private Panel logBodyPanel;
        private Timer serviceRefreshTimer;
        private NotifyIcon notifyIcon;
        private ToolStripMenuItem trayStartItem, trayStopItem;
        private Button copyDiagnosticsButton;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            serviceRefreshTimer = new Timer(components) { Interval = 2000 };
            SuspendLayout();
            Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 10.5F);
            ForeColor = CodexVisual.Ink;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(940, 690);
            MinimumSize = new Size(880, 680);
            Text = "Codex Proxy";
            BackColor = CodexVisual.Background;
            StartPosition = FormStartPosition.CenterScreen;
            rootPanel = Layout(1, 5);
            rootPanel.Padding = new Padding(24, 22, 24, 12);
            rootPanel.RowStyles[3].SizeType = SizeType.Percent; rootPanel.RowStyles[3].Height = 100;
            Controls.Add(rootPanel);

            var header = Layout(2, 2);
            header.Margin = new Padding(6, 0, 6, 20);
            header.ColumnStyles[0].SizeType = SizeType.Percent; header.ColumnStyles[0].Width = 100;
            titleLabel = Label("titleLabel", "Codex 专用代理");
            titleLabel.Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 21F, FontStyle.Bold);
            header.Controls.Add(titleLabel, 0, 0);
            var subtitle = Label("subtitleLabel", "桌面应用 · 后台进程 · CLI"); subtitle.ForeColor = CodexVisual.Muted;
            header.Controls.Add(subtitle, 0, 1);
            serviceStateValueLabel = new StatusPill { Name = "serviceStateValueLabel", Text = "等待服务状态",
                ForeColor = CodexVisual.Muted, Anchor = AnchorStyles.Right, Margin = new Padding(0, 6, 0, 0) };
            header.Controls.Add(serviceStateValueLabel, 1, 0); header.SetRowSpan(serviceStateValueLabel, 2);
            rootPanel.Controls.Add(header, 0, 0);

            var cards = Layout(2, 1);
            cards.MinimumSize = new Size(0, 280);
            cards.ColumnStyles.Clear();
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51));
            settingsPanel = Card(1, 7);
            settingsPanel.Name = "settingsPanel"; settingsPanel.Margin = new Padding(0, 0, 8, 0);
            var proxyHeading = Layout(2, 1);
            proxyHeading.ColumnStyles[0].SizeType = SizeType.Percent; proxyHeading.ColumnStyles[0].Width = 100;
            proxyHeading.Controls.Add(Heading("proxyTitle", "代理设置"), 0, 0);
            var badge = Label("transportBadge", "SOCKS5"); badge.ForeColor = CodexVisual.Blue;
            badge.BackColor = Color.FromArgb(232, 240, 255); badge.Padding = new Padding(10, 4, 10, 4); badge.Anchor = AnchorStyles.Right;
            proxyHeading.Controls.Add(badge, 1, 0);
            settingsPanel.Controls.Add(proxyHeading, 0, 0);
            var endpointLabel = Label("endpointLabel", "代理地址"); endpointLabel.Margin = new Padding(0, 14, 0, 6);
            settingsPanel.Controls.Add(endpointLabel, 0, 1);
            endpointTextBox = Input("endpointTextBox", "127.0.0.1:7890");
            settingsPanel.Controls.Add(InputFrame(endpointTextBox), 0, 2);
            var hint = Label("endpointHint", "使用 SOCKS5 或混合端口"); hint.ForeColor = CodexVisual.Muted;
            hint.Margin = new Padding(0, 7, 0, 15); settingsPanel.Controls.Add(hint, 0, 3);
            bypassLanCheckBox = Check("bypassLanCheckBox", "局域网直连", true);
            authenticationCheckBox = Check("authenticationCheckBox", "代理需要认证", false);
            settingsPanel.Controls.Add(bypassLanCheckBox, 0, 4); settingsPanel.Controls.Add(authenticationCheckBox, 0, 5);
            authenticationFieldsPanel = Layout(2, 2);
            authenticationFieldsPanel.Margin = new Padding(0, 7, 0, 0);
            authenticationFieldsPanel.ColumnStyles[1].SizeType = SizeType.Percent; authenticationFieldsPanel.ColumnStyles[1].Width = 100;
            usernameTextBox = Input("usernameTextBox", ""); passwordTextBox = Input("passwordTextBox", "");
            passwordTextBox.UseSystemPasswordChar = true;
            authenticationFieldsPanel.Controls.Add(Label("usernameLabel", "用户名"), 0, 0);
            authenticationFieldsPanel.Controls.Add(InputFrame(usernameTextBox), 1, 0);
            authenticationFieldsPanel.Controls.Add(Label("passwordLabel", "密码"), 0, 1);
            authenticationFieldsPanel.Controls.Add(InputFrame(passwordTextBox), 1, 1);
            settingsPanel.Controls.Add(authenticationFieldsPanel, 0, 6);
            cards.Controls.Add(settingsPanel, 0, 0);

            targetsPanel = Card(1, 3); targetsPanel.Name = "targetsPanel";
            targetsPanel.Margin = new Padding(8, 0, 0, 0);
            targetsPanel.RowStyles[1].SizeType = SizeType.Percent; targetsPanel.RowStyles[1].Height = 100;
            var targetsHeading = Layout(2, 1);
            targetsHeading.ColumnStyles[0].SizeType = SizeType.Percent; targetsHeading.ColumnStyles[0].Width = 100;
            targetsHeading.Controls.Add(Heading("targetsTitle", "Codex 路径"), 0, 0);
            refreshTargetsButton = Button("refreshTargetsButton", "刷新路径", "refresh");
            refreshTargetsButton.Padding = new Padding(10, 4, 10, 4); refreshTargetsButton.Margin = Padding.Empty;
            targetsHeading.Controls.Add(refreshTargetsButton, 1, 0); targetsPanel.Controls.Add(targetsHeading, 0, 0);
            var desktopCard = Card(1, 2); desktopCard.SurfaceColor = CodexVisual.Background;
            desktopCard.Padding = new Padding(12); desktopCard.Margin = new Padding(0, 12, 0, 10);
            desktopCard.RowStyles[1].SizeType = SizeType.Percent; desktopCard.RowStyles[1].Height = 100;
            desktopCard.Controls.Add(Heading("desktopTitle", "桌面应用与后台进程"), 0, 0);
            targetsListBox = new ListBox { Name = "targetsListBox", Dock = DockStyle.Fill, HorizontalScrollbar = true,
                IntegralHeight = false, BorderStyle = BorderStyle.None, BackColor = CodexVisual.Background,
                ForeColor = CodexVisual.Muted, Margin = new Padding(0, 8, 0, 0), Height = 66 };
            targetsListBox.Items.Add("自动识别 Codex 安装路径");
            desktopCard.Controls.Add(targetsListBox, 0, 1); targetsPanel.Controls.Add(desktopCard, 0, 1);
            var cliCard = Card(2, 2); cliCard.SurfaceColor = CodexVisual.Background;
            cliCard.Padding = new Padding(12); cliCard.Margin = Padding.Empty;
            cliCard.ColumnStyles[0].SizeType = SizeType.Percent; cliCard.ColumnStyles[0].Width = 100;
            cliCard.Controls.Add(Heading("cliTitle", "Codex CLI"), 0, 0);
            cliPathLabel = Label("cliPathLabel", "未选择路径"); cliPathLabel.ForeColor = CodexVisual.Muted;
            cliPathLabel.AutoSize = false; cliPathLabel.Dock = DockStyle.Fill; cliPathLabel.Height = 44;
            cliCard.Controls.Add(cliPathLabel, 0, 1);
            selectCliButton = Button("selectCliButton", "选择路径"); selectCliButton.Margin = new Padding(8, 0, 0, 0);
            selectCliButton.Anchor = AnchorStyles.Right;
            cliCard.Controls.Add(selectCliButton, 1, 0); cliCard.SetRowSpan(selectCliButton, 2);
            targetsPanel.Controls.Add(cliCard, 0, 2); cards.Controls.Add(targetsPanel, 1, 0);
            rootPanel.Controls.Add(cards, 0, 1);

            var actions = Card(3, 1); actions.Padding = new Padding(16, 12, 16, 12);
            actions.Margin = new Padding(0, 12, 0, 12);
            actions.ColumnStyles[1].SizeType = SizeType.Percent; actions.ColumnStyles[1].Width = 100;
            var actionButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                WrapContents = false, Margin = Padding.Empty };
            startServiceButton = Button("startServiceButton", "启动代理", "play"); ((CodexButton)startServiceButton).Primary = true;
            stopServiceButton = Button("stopServiceButton", "停止代理", "stop");
            applyButton = Button("applyButton", "保存并应用", "save");
            actionButtons.Controls.AddRange(new Control[] { startServiceButton, stopServiceButton, applyButton });
            actions.Controls.Add(actionButtons, 0, 0);
            stateDetailLabel = Label("stateDetailLabel", ""); stateDetailLabel.ForeColor = CodexVisual.Muted;
            stateDetailLabel.Anchor = AnchorStyles.Left; actions.Controls.Add(stateDetailLabel, 1, 0);
            changesStateLabel = Label("changesStateLabel", ""); changesStateLabel.Anchor = AnchorStyles.Right;
            changesStateLabel.ForeColor = CodexVisual.Muted; actions.Controls.Add(changesStateLabel, 2, 0);
            rootPanel.Controls.Add(actions, 0, 2);

            var logs = Card(1, 2); logs.AutoSize = false; logs.Padding = new Padding(16, 10, 16, 14);
            logs.RowStyles[1].SizeType = SizeType.Percent; logs.RowStyles[1].Height = 100;
            var logHeader = Layout(2, 1);
            logHeader.ColumnStyles[0].SizeType = SizeType.Percent; logHeader.ColumnStyles[0].Width = 100;
            logHeader.Controls.Add(Heading("logsLabel", "运行日志"), 0, 0);
            var logTools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                WrapContents = false, Margin = Padding.Empty };
            copyDiagnosticsButton = Button("copyDiagnosticsButton", "复制诊断"); ((CodexButton)copyDiagnosticsButton).Quiet = true;
            logLevelComboBox = new ComboBox { Name = "logLevelComboBox", DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat, Width = 84, Margin = new Padding(8, 7, 12, 0) };
            logLevelComboBox.Items.AddRange(new object[] { "Error", "Warning", "Info", "Debug", "All" }); logLevelComboBox.SelectedItem = "Info";
            collapseLogsButton = Button("collapseLogsButton", "收起", "up"); ((CodexButton)collapseLogsButton).Quiet = true;
            collapseLogsButton.Margin = Padding.Empty; collapseLogsButton.Click += ToggleLogs;
            logTools.Controls.AddRange(new Control[] { copyDiagnosticsButton, logLevelComboBox, collapseLogsButton });
            logHeader.Controls.Add(logTools, 1, 0); logs.Controls.Add(logHeader, 0, 0);
            logBodyPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 0), BackColor = Color.White };
            logTextBox = new TextBox { Name = "logTextBox", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Both, WordWrap = false, BackColor = Color.White, BorderStyle = BorderStyle.None };
            logEmptyLabel = new Label { Name = "logEmptyLabel", Text = "启动代理后显示日志", Dock = DockStyle.Fill,
                ForeColor = CodexVisual.Muted, TextAlign = ContentAlignment.MiddleCenter };
            logBodyPanel.Controls.Add(logTextBox); logBodyPanel.Controls.Add(logEmptyLabel); logEmptyLabel.BringToFront();
            logTextBox.TextChanged += (sender, args) => logEmptyLabel.Visible = logTextBox.TextLength == 0;
            logs.Controls.Add(logBodyPanel, 0, 1); rootPanel.Controls.Add(logs, 0, 3);

            var footer = Layout(2, 1); footer.Dock = DockStyle.Bottom; footer.Margin = new Padding(3, 10, 3, 0);
            footer.ColumnStyles[0].SizeType = SizeType.Percent; footer.ColumnStyles[0].Width = 100;
            driverLabel = Label("driverLabel", "Windows Packet Filter"); driverLabel.ForeColor = CodexVisual.Muted;
            footer.Controls.Add(driverLabel, 0, 0);
            minimizeToTrayCheckBox = Check("minimizeToTrayCheckBox", "关闭窗口后保留在托盘", true);
            minimizeToTrayCheckBox.ForeColor = CodexVisual.Muted; minimizeToTrayCheckBox.Margin = Padding.Empty;
            footer.Controls.Add(minimizeToTrayCheckBox, 1, 0); rootPanel.Controls.Add(footer, 0, 4);
            ResumeLayout(true);
        }

        private static TableLayoutPanel Layout(int columns, int rows)
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = columns,
                RowCount = rows, Margin = Padding.Empty, BackColor = Color.Transparent };
            for (var i = 0; i < columns; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            if (columns == 1) { panel.ColumnStyles[0].SizeType = SizeType.Percent; panel.ColumnStyles[0].Width = 100; }
            for (var i = 0; i < rows; i++) panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return panel;
        }
        private static CardLayout Card(int columns, int rows)
        {
            var panel = new CardLayout { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = columns,
                RowCount = rows, Padding = new Padding(16), Margin = Padding.Empty };
            for (var i = 0; i < columns; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            if (columns == 1) { panel.ColumnStyles[0].SizeType = SizeType.Percent; panel.ColumnStyles[0].Width = 100; }
            for (var i = 0; i < rows; i++) panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return panel;
        }
        private static Label Label(string name, string text)
        { return new Label { Name = name, Text = text, AutoSize = true, Margin = new Padding(0, 4, 0, 4), Anchor = AnchorStyles.Left }; }
        private static Label Heading(string name, string text)
        { var label = Label(name, text); label.Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 11.5F, FontStyle.Bold); return label; }
        private static TextBox Input(string name, string text)
        { return new TextBox { Name = name, Text = text, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Margin = Padding.Empty }; }
        private static CardLayout InputFrame(TextBox input)
        {
            var frame = Card(1, 1); frame.Padding = new Padding(11, 9, 11, 9);
            frame.Margin = new Padding(0, 0, 0, 5); frame.Controls.Add(input, 0, 0);
            input.GotFocus += (sender, args) => { frame.BorderColor = CodexVisual.Blue; frame.Invalidate(); };
            input.LostFocus += (sender, args) => { frame.BorderColor = CodexVisual.Line; frame.Invalidate(); };
            return frame;
        }
        private static CheckBox Check(string name, string text, bool value)
        { return new CodexCheckBox { Name = name, Text = text, Checked = value, Margin = new Padding(0, 5, 0, 5) }; }
        private static Button Button(string name, string text, string glyph = null)
        { return new CodexButton { Name = name, Text = text, Glyph = glyph }; }
    }
}


