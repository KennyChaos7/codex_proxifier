using ProxiFyre.Configuration;
using ProxiFyreUI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
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
            SizeCandidateRows();
            candidatesMessage.Text = candidates.Count == 0
                ? "未找到 Codex CLI，请选择 codex.exe。"
                : "找到以下可用路径，请选择一个。";
            usePathButton.Enabled = candidates.Count > 0;
            if (candidates.Count > 0) candidatesGrid.CurrentCell = candidatesGrid.Rows[0].Cells[0];
        }

        private void SizeCandidateRows()
        {
            foreach (DataGridViewRow row in candidatesGrid.Rows) row.Height = candidatesGrid.Font.Height * 4 + 22;
        }

        private void PaintCandidate(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.Handled = true;
            var row = candidatesGrid.Rows[e.RowIndex];
            var selected = row.Selected;
            var scale = Font.Height / 18F;
            var gap = (int)(8 * scale);
            var card = new Rectangle(e.RowBounds.X + 1, e.RowBounds.Y + 1, e.RowBounds.Width - 3, e.RowBounds.Height - gap);
            CodexVisual.Surface(e.Graphics, card, 8 * scale,
                selected ? Color.FromArgb(238, 245, 255) : Color.White,
                selected ? Color.FromArgb(179, 205, 255) : CodexVisual.Line);
            var radioSize = (int)(18 * scale);
            var radio = new Rectangle(card.X + (int)(18 * scale), card.Y + (card.Height - radioSize) / 2, radioSize, radioSize);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(selected ? CodexVisual.Blue : Color.FromArgb(143, 161, 195), 1.5F * scale))
            using (var brush = new SolidBrush(selected ? CodexVisual.Blue : Color.White))
            {
                e.Graphics.FillEllipse(brush, radio); e.Graphics.DrawEllipse(pen, radio);
                if (selected) using (var dot = new SolidBrush(Color.White))
                    e.Graphics.FillEllipse(dot, radio.X + radioSize * .32F, radio.Y + radioSize * .32F, radioSize * .36F, radioSize * .36F);
            }
            var tileSize = (int)(42 * scale);
            var tile = new Rectangle(radio.Right + (int)(16 * scale), card.Y + (card.Height - tileSize) / 2, tileSize, tileSize);
            CodexVisual.Surface(e.Graphics, tile, 7 * scale, Color.FromArgb(245, 248, 253), Color.FromArgb(245, 248, 253));
            var terminal = Rectangle.Inflate(tile, -(int)(6 * scale), -(int)(6 * scale));
            CodexVisual.Surface(e.Graphics, terminal, 4 * scale, CodexVisual.Ink, CodexVisual.Ink);
            CodexVisual.Icon(e.Graphics, "terminal", Rectangle.Inflate(terminal, -(int)(4 * scale), -(int)(4 * scale)), Color.White);
            var x = tile.Right + (int)(14 * scale);
            var source = row.Cells[1].Value.ToString();
            var running = source.Contains("正在运行");
            var badgeSize = TextRenderer.MeasureText(source, Font);
            var badge = new Rectangle(x, card.Y + (int)(12 * scale), badgeSize.Width + (int)(10 * scale), Font.Height + (int)(6 * scale));
            CodexVisual.Surface(e.Graphics, badge, 5 * scale,
                running ? Color.FromArgb(223, 248, 229) : Color.FromArgb(243, 247, 255),
                running ? Color.FromArgb(183, 230, 195) : CodexVisual.Line);
            TextRenderer.DrawText(e.Graphics, source, Font, badge,
                running ? Color.FromArgb(26, 119, 64) : Color.FromArgb(62, 87, 138),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            var text = new Rectangle(x, badge.Bottom + (int)(3 * scale),
                card.Right - x - (int)(14 * scale), card.Bottom - badge.Bottom - (int)(8 * scale));
            TextRenderer.DrawText(e.Graphics, row.Cells[0].Value.ToString(), Font, text, Color.FromArgb(62, 87, 138),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
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
