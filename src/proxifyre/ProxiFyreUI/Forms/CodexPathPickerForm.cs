using ProxiFyre.Configuration;
using ProxiFyreUI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProxiFyreUI.Forms
{
    internal sealed partial class CodexPathPickerForm : Form
    {
        private IReadOnlyList<CodexPathCandidate> _candidates;
        public string SelectedPath { get; private set; }

        public CodexPathPickerForm(IReadOnlyList<CodexPathCandidate> candidates = null)
        {
            InitializeComponent();
            SetCandidates(candidates ?? new CodexTargetDiscovery().FindCliCandidates());
        }

        private void SetCandidates(IReadOnlyList<CodexPathCandidate> candidates)
        {
            _candidates = candidates;
            candidatesGrid.Rows.Clear();
            foreach (var candidate in candidates)
            {
                var row = candidatesGrid.Rows.Add(CodexTargetDiscovery.FormatDisplayPath(candidate.ExecutablePath), candidate.SourceLabel);
                candidatesGrid.Rows[row].Tag = candidate;
            }
            candidatesMessage.Text = candidates.Count == 0
                ? "未找到 Codex CLI，请选择 codex.exe。"
                : "找到以下可用路径，请选择一个。";
            usePathButton.Enabled = candidates.Count > 0;
            if (candidates.Count > 0) candidatesGrid.CurrentCell = candidatesGrid.Rows[0].Cells[0];
        }

        private void UsePath(object sender, EventArgs args)
        {
            if (candidatesGrid.CurrentRow == null) return;
            SelectedPath = ((CodexPathCandidate)candidatesGrid.CurrentRow.Tag).ExecutablePath;
            DialogResult = DialogResult.OK;
        }

        private void BrowseOther(object sender, EventArgs args)
        {
            var first = _candidates.FirstOrDefault();
            using (var dialog = new OpenFileDialog {
                Title = "选择原生 Codex CLI",
                Filter = "Codex CLI (codex.exe)|codex.exe",
                CheckFileExists = true,
                InitialDirectory = first == null
                    ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    : Path.GetDirectoryName(first.ExecutablePath) })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (!string.Equals(Path.GetFileName(dialog.FileName), "codex.exe", StringComparison.OrdinalIgnoreCase) ||
                    CodexRoutingPolicy.ValidateTargets(new[] { dialog.FileName }).HasErrors)
                {
                    MessageBox.Show(this, "请选择原生 codex.exe。", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedPath = Path.GetFullPath(dialog.FileName);
                DialogResult = DialogResult.OK;
            }
        }

        private void RefreshCandidates(object sender, EventArgs args)
        {
            SetCandidates(new CodexTargetDiscovery().FindCliCandidates());
        }
    }
}


