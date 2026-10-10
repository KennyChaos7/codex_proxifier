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
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = WinFormsFontBootstrap.CreateUiFont("Segoe UI", 10F);
            Text = "选择 Codex CLI 路径";
            ClientSize = new Size(800, 390);
            MinimumSize = new Size(640, 330);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            BackColor = Color.White;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20),
                ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            candidatesMessage = new Label { AutoSize = true, Padding = new Padding(0, 0, 0, 12) };
            candidatesGrid = new DataGridView {
                Name = "candidatesGrid", Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                MultiSelect = false, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            candidatesGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            candidatesGrid.DefaultCellStyle.Padding = new Padding(8);
            candidatesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "可用路径", FillWeight = 78 });
            candidatesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "来源", FillWeight = 22 });
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                Padding = new Padding(0, 14, 0, 0) };
            var refresh = new Button { Name = "refreshCandidatesButton", Text = "刷新候选路径", AutoSize = true };
            refresh.Click += RefreshCandidates;
            var browse = new Button { Name = "browseOtherButton", Text = "浏览其他位置…", AutoSize = true };
            browse.Click += BrowseOther;
            var cancel = new Button { Name = "cancelButton", Text = "取消", DialogResult = DialogResult.Cancel, AutoSize = true };
            usePathButton = new Button { Name = "usePathButton", Text = "使用此路径", AutoSize = true };
            usePathButton.Click += UsePath;
            actions.Controls.AddRange(new Control[] { browse, refresh, cancel, usePathButton });
            AcceptButton = usePathButton;
            CancelButton = cancel;
            root.Controls.Add(candidatesMessage, 0, 0);
            root.Controls.Add(candidatesGrid, 0, 1);
            root.Controls.Add(actions, 0, 2);
            Controls.Add(root);
        }
    }
}

