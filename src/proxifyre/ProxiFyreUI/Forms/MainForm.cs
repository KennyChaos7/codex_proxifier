using ProxiFyre.Configuration;
using ProxiFyreUI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProxiFyreUI.Forms
{
    internal partial class MainForm : Form
    {
        private readonly UiSettingsStore _settingsStore;
        private readonly IEngineLocator _engineLocator;
        private readonly IProxiFyreServiceController _serviceController;
        private readonly ConfigurationWorkspace _workspace;
        private readonly ILogTailer _logTailer;
        private readonly DiagnosticsBuilder _diagnosticsBuilder;
        private readonly UserVisibleErrorFormatter _errorFormatter;
        private readonly ConfigurationApplyCoordinator _applyCoordinator;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly CodexTargetDiscovery _discovery = new CodexTargetDiscovery();
        private readonly List<string> _targets = new List<string>();
        private readonly Queue<string> _logLines = new Queue<string>();
        private UiSettings _settings;
        private EngineLocation _engineLocation;
        private ServiceStatusInfo _serviceStatus = new ServiceStatusInfo(ProxiFyreServiceState.Unknown);
        private string _selectedCliPath;
        private bool _binding, _busy, _refreshing, _exiting;

        public MainForm() : this(new UiSettingsStore(), new EngineLocator(),
            new ProxiFyreServiceController(), new ConfigurationWorkspace(), new LogTailer(), new DiagnosticsBuilder()) { }

        internal MainForm(UiSettingsStore settingsStore, IEngineLocator engineLocator,
            IProxiFyreServiceController serviceController, ConfigurationWorkspace workspace,
            ILogTailer logTailer, DiagnosticsBuilder diagnosticsBuilder)
        {
            _settingsStore = settingsStore;
            _engineLocator = engineLocator;
            _serviceController = serviceController;
            _workspace = workspace;
            _logTailer = logTailer;
            _diagnosticsBuilder = diagnosticsBuilder;
            _errorFormatter = new UserVisibleErrorFormatter(diagnosticsBuilder);
            _applyCoordinator = new ConfigurationApplyCoordinator(workspace, serviceController,
                path => _engineLocator.IsRegisteredServiceExecutable(path));
            _settings = _settingsStore.Load();
            InitializeComponent();
            minimizeToTrayCheckBox.Checked = _settings.MinimizeToTray;
            _workspace.DirtyStateChanged += WorkspaceChanged;
            serviceRefreshTimer.Tick += async (sender, args) => await RefreshStatusAsync();
            startServiceButton.Click += async (sender, args) => await ApplyAsync();
            applyButton.Click += async (sender, args) => await ApplyAsync();
            stopServiceButton.Click += async (sender, args) => await StopAsync();
            selectCliButton.Click += SelectCli;
            refreshTargetsButton.Click += async (sender, args) => await RunAsync(RefreshTargetsAsync);
            endpointTextBox.TextChanged += SettingsChanged;
            usernameTextBox.TextChanged += SettingsChanged;
            passwordTextBox.TextChanged += SettingsChanged;
            authenticationCheckBox.CheckedChanged += SettingsChanged;
            bypassLanCheckBox.CheckedChanged += SettingsChanged;
            logLevelComboBox.SelectedIndexChanged += SettingsChanged;
            minimizeToTrayCheckBox.CheckedChanged += (sender, args) => {
                _settings.MinimizeToTray = minimizeToTrayCheckBox.Checked;
                try { _settingsStore.Save(_settings); } catch (Exception ex) { ShowError("无法保存窗口偏好。", ex); }
            };
            UpdateState();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await RunAsync(async () => {
                _engineLocation = _engineLocator.Resolve(_settings);
                if (!_engineLocation.IsResolved)
                    throw new InvalidOperationException("未找到 CodexProxy.Service.exe，请重新安装 Codex Proxy。 " + _engineLocation.Error);
                _workspace.Load(_engineLocation);
                BindConfiguration();
                await RefreshTargetsAsync();
            });
            await RefreshStatusAsync();
            serviceRefreshTimer.Start();
        }

        private void BindConfiguration()
        {
            _binding = true;
            try
            {
                var configuration = _workspace.Configuration;
                var rule = configuration.Proxies.FirstOrDefault();
                endpointTextBox.Text = rule?.Socks5ProxyEndpoint ?? "127.0.0.1:7890";
                usernameTextBox.Text = rule?.Username ?? "";
                passwordTextBox.Text = rule?.Password ?? "";
                authenticationCheckBox.Checked = !string.IsNullOrEmpty(rule?.Username) || !string.IsNullOrEmpty(rule?.Password);
                bypassLanCheckBox.Checked = configuration.BypassLan;
                logLevelComboBox.SelectedItem = new[] { "Error", "Warning", "Info", "Debug", "All" }
                    .FirstOrDefault(value => string.Equals(value, configuration.LogLevel, StringComparison.OrdinalIgnoreCase)) ?? "Info";
                _selectedCliPath = rule?.AppNames?.FirstOrDefault(path =>
                    string.Equals(Path.GetFileName(path), "codex.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(path));
            }
            finally { _binding = false; }
        }

        private async Task RefreshTargetsAsync()
        {
            var targets = await Task.Run(() => _discovery.Discover());
            if (_lifetime.IsCancellationRequested) return;
            if (!string.IsNullOrEmpty(_selectedCliPath) && File.Exists(_selectedCliPath)) targets =
                targets.Concat(new[] { _selectedCliPath }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            else _selectedCliPath = null;
            _targets.Clear();
            _targets.AddRange(targets);
            BindTargets();
            if (_workspace.Configuration != null) _workspace.MarkDirty();
        }

        private void BindTargets()
        {
            targetsListBox.Items.Clear();
            foreach (var target in _targets) targetsListBox.Items.Add(CodexTargetDiscovery.FormatDisplayPath(target));
            if (_targets.Count == 0) targetsListBox.Items.Add("未发现 Codex。请安装桌面应用或选择原生 CLI。");
            cliPathLabel.Text = string.IsNullOrEmpty(_selectedCliPath) ? "自动识别；可选择其他 codex.exe" :
                CodexTargetDiscovery.FormatDisplayPath(_selectedCliPath);
        }

        private void SelectCli(object sender, EventArgs e)
        {
            using (var picker = new CodexPathPickerForm())
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                _selectedCliPath = picker.SelectedPath;
                if (!_targets.Contains(_selectedCliPath, StringComparer.OrdinalIgnoreCase)) _targets.Add(_selectedCliPath);
                BindTargets();
                _workspace.MarkDirty();
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (!_binding && _workspace.Configuration != null) _workspace.MarkDirty();
            UpdateState();
        }

        private void ReadConfiguration()
        {
            var configuration = CodexRoutingPolicy.CreateConfiguration(_targets, new ProxyRule {
                Socks5ProxyEndpoint = endpointTextBox.Text.Trim(),
                Username = authenticationCheckBox.Checked ? usernameTextBox.Text : "",
                Password = authenticationCheckBox.Checked ? passwordTextBox.Text : "",
                Socks5Transport = "TCP"
            }, bypassLanCheckBox.Checked, logLevelComboBox.SelectedItem?.ToString() ?? "Info");
            var current = _workspace.Configuration;
            current.Proxies = configuration.Proxies;
            current.Excludes = configuration.Excludes;
            current.LogLevel = configuration.LogLevel;
            current.BypassLan = configuration.BypassLan;
            current.ExtensionData.Clear();
            var validation = _workspace.Validate();
            if (validation.HasErrors) throw new InvalidOperationException(ConfigurationWorkspace.FormatValidation(validation));
        }

        private async Task ApplyAsync()
        {
            await RunAsync(async () => {
                ReadConfiguration();
                var result = await _applyCoordinator.SaveAndApplyAsync(false, _engineLocation.Path, true, true,
                    ApplicationConstants.ServiceOperationTimeout, _lifetime.Token);
                if (!result.IsApplied)
                {
                    if (result.ServiceResult != null) throw new InvalidOperationException(
                        _errorFormatter.FormatServiceFailure(result.ServiceResult, _logLines, Secrets()));
                    if (result.SaveResult?.Error != null) throw result.SaveResult.Error;
                    throw new InvalidOperationException(result.Outcome == ConfigurationApplyOutcome.ExternalChangeDetected ?
                        "配置已被其他程序更改。请重新打开窗口后再保存。" : "配置未能应用：" + result.Outcome);
                }
                stateDetailLabel.Text = "配置已保存并应用";
            });
            await RefreshStatusAsync();
        }

        private async Task StopAsync()
        {
            await RunAsync(async () => {
                var result = await _serviceController.StopAsync(ApplicationConstants.ServiceOperationTimeout, _lifetime.Token);
                if (!result.Success) throw new InvalidOperationException(
                    _errorFormatter.FormatServiceFailure(result, _logLines, Secrets()));
            });
            await RefreshStatusAsync();
        }

        private async Task RunAsync(Func<Task> operation)
        {
            if (_busy) return;
            _busy = true;
            UpdateState();
            try { await operation(); }
            catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
            catch (Exception ex) { if (!_exiting) ShowError("操作未完成。", ex); }
            finally { _busy = false; if (!_exiting) UpdateState(); }
        }

        private async Task RefreshStatusAsync()
        {
            if (_refreshing || _exiting || _busy) return;
            _refreshing = true;
            try { _serviceStatus = await _serviceController.GetStatusAsync(_lifetime.Token); UpdateState(); }
            catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
            catch (Exception ex) { _serviceStatus = new ServiceStatusInfo(ProxiFyreServiceState.Error, ex.Message); UpdateState(); }
            finally { _refreshing = false; }
        }

        private void WorkspaceChanged(object sender, EventArgs e) { UpdateState(); }
        private void UpdateState()
        {
            usernameTextBox.Enabled = passwordTextBox.Enabled = authenticationCheckBox.Checked && !_busy;
            settingsPanel.Enabled = targetsPanel.Enabled = !_busy;
            var ready = !_busy && _workspace.Configuration != null;
            startServiceButton.Enabled = ready && !_serviceStatus.IsRunning;
            stopServiceButton.Enabled = !_busy && _serviceStatus.IsRunning;
            applyButton.Enabled = ready;
            serviceStateValueLabel.Text = _busy ? "正在处理…" : StateText(_serviceStatus.State);
            serviceStateValueLabel.ForeColor = _serviceStatus.IsRunning ? Color.FromArgb(30, 130, 82) : Color.FromArgb(70, 80, 96);
            changesStateLabel.Text = _workspace.IsDirty ? "更改尚未保存" : "";
        }

        private static string StateText(ProxiFyreServiceState state)
        {
            switch (state)
            {
                case ProxiFyreServiceState.Running: return "代理运行中";
                case ProxiFyreServiceState.Stopped: return "代理已停止";
                case ProxiFyreServiceState.NotInstalled: return "服务未安装";
                case ProxiFyreServiceState.StartPending: return "正在启动";
                case ProxiFyreServiceState.StopPending: return "正在停止";
                case ProxiFyreServiceState.Error: return "无法读取服务状态";
                case ProxiFyreServiceState.DeletionPending: return "服务正在移除";
                default: return "等待服务状态";
            }
        }

        private IEnumerable<string> Secrets() { return new[] { usernameTextBox.Text, passwordTextBox.Text }; }
        private void ShowError(string summary, Exception ex)
        {
            MessageBox.Show(this, _errorFormatter.FormatException(summary, ex, Secrets()), "Codex Proxy",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void RestoreFromSecondaryLaunch()
        {
            if (_exiting) return;
            Show(); WindowState = FormWindowState.Normal; Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_exiting && e.CloseReason == CloseReason.UserClosing && minimizeToTrayCheckBox.Checked)
            { e.Cancel = true; Hide(); return; }
            if (_busy && e.CloseReason == CloseReason.UserClosing)
            { e.Cancel = true; return; }
            _exiting = true;
            _lifetime.Cancel();
            serviceRefreshTimer.Stop();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _exiting = true;
                _lifetime.Cancel();
                _workspace.DirtyStateChanged -= WorkspaceChanged;
                _logTailer.Dispose();
                _serviceController.Dispose();
                _lifetime.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
