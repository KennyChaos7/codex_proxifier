using System.Drawing;
using System.Windows.Forms;
using ProxiFyreUI.Infrastructure;

namespace ProxiFyreUI.Forms
{
    internal sealed partial class CodexPathPickerForm
    {
        private DataGridView candidatesGrid;
        private Label candidatesMessage;
        private Button usePathButton;

        private void InitializeComponent()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 12F);
            ForeColor = CodexVisual.Ink;
            Text = "选择 Codex CLI 路径";
            ClientSize = new Size(660, 450);
            MinimumSize = new Size(660, 450);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; MinimizeBox = false; MaximizeBox = false;
            BackColor = Color.White;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24, 18, 24, 16),
                ColumnCount = 1, RowCount = 5 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var title = new Label { Text = "选择 Codex CLI 路径", AutoSize = true,
                Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 17F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8) };
            candidatesMessage = new Label { AutoSize = true, ForeColor = CodexVisual.Muted, Margin = new Padding(0, 0, 0, 14) };
            candidatesGrid = new DataGridView {
                Name = "candidatesGrid", Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false, MultiSelect = false, RowHeadersVisible = false, ColumnHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.None,
                ScrollBars = ScrollBars.Vertical, Margin = Padding.Empty };
            candidatesGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            candidatesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "候选路径", FillWeight = 78 });
            candidatesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "来源", FillWeight = 22 });
            candidatesGrid.RowPrePaint += PaintCandidate;
            candidatesGrid.SelectionChanged += (sender, args) => candidatesGrid.Invalidate();
            candidatesGrid.FontChanged += (sender, args) => SizeCandidateRows();
            candidatesGrid.SizeChanged += (sender, args) => SizeCandidateRows();
            var refresh = new CodexButton { Name = "refreshCandidatesButton", Text = "刷新候选路径",
                Glyph = "refresh", Quiet = true, ForeColor = Color.FromArgb(62, 87, 138),
                Padding = new Padding(3, 7, 3, 7), Margin = new Padding(0, 6, 0, 10), Anchor = AnchorStyles.Left };
            refresh.Click += RefreshCandidates;
            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 12, 0, 0), Margin = Padding.Empty };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            actions.Paint += (sender, args) => {
                using (var pen = new Pen(CodexVisual.Line)) args.Graphics.DrawLine(pen, 0, 0, actions.Width, 0);
            };
            var browse = new CodexButton { Name = "browseOtherButton", Text = "浏览其他位置...", Anchor = AnchorStyles.Left };
            browse.Click += BrowseOther;
            var cancel = new CodexButton { Name = "cancelButton", Text = "取消", DialogResult = DialogResult.Cancel, Padding = new Padding(22, 7, 22, 7) };
            usePathButton = new CodexButton { Name = "usePathButton", Text = "使用此路径", Primary = true, Margin = Padding.Empty };
            usePathButton.Click += UsePath;
            actions.Controls.Add(browse, 0, 0); actions.Controls.Add(cancel, 1, 0); actions.Controls.Add(usePathButton, 2, 0);
            AcceptButton = usePathButton; CancelButton = cancel;
            root.Controls.Add(title, 0, 0); root.Controls.Add(candidatesMessage, 0, 1);
            root.Controls.Add(candidatesGrid, 0, 2); root.Controls.Add(refresh, 0, 3); root.Controls.Add(actions, 0, 4);
            Controls.Add(root);
        }
    }
}
