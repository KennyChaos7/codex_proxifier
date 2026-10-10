using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProxiFyreUI.Forms
{
    internal static class CodexVisual
    {
        internal static readonly Color Ink = Color.FromArgb(20, 31, 48);
        internal static readonly Color Muted = Color.FromArgb(96, 111, 133);
        internal static readonly Color Line = Color.FromArgb(220, 227, 238);
        internal static readonly Color Blue = Color.FromArgb(30, 100, 230);
        internal static readonly Color Background = Color.FromArgb(246, 248, 252);

        internal static GraphicsPath Round(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            var d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        internal static void Surface(Graphics g, Rectangle r, float radius, Color fill, Color line)
        {
            if (r.Width <= 1 || r.Height <= 1) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Round(new RectangleF(r.X + .5F, r.Y + .5F, r.Width - 1, r.Height - 1), radius))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(line))
            { g.FillPath(brush, path); g.DrawPath(pen, path); }
        }

        internal static void Icon(Graphics g, string icon, Rectangle r, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, Math.Max(1.5F, r.Width / 11F)))
            using (var brush = new SolidBrush(color))
            {
                if (icon == "play")
                    g.FillPolygon(brush, new[] { new PointF(r.X + r.Width * .25F, r.Y + r.Height * .1F),
                        new PointF(r.Right - r.Width * .15F, r.Y + r.Height * .5F),
                        new PointF(r.X + r.Width * .25F, r.Bottom - r.Height * .1F) });
                else if (icon == "stop")
                    g.FillRectangle(brush, r.X + r.Width * .22F, r.Y + r.Height * .22F, r.Width * .56F, r.Height * .56F);
                else if (icon == "refresh")
                {
                    g.DrawArc(pen, r.X + r.Width * .12F, r.Y + r.Height * .12F, r.Width * .76F, r.Height * .76F, 35, 285);
                    g.DrawLines(pen, new[] { new PointF(r.Right - r.Width * .08F, r.Y + r.Height * .02F),
                        new PointF(r.Right - r.Width * .08F, r.Y + r.Height * .31F),
                        new PointF(r.Right - r.Width * .36F, r.Y + r.Height * .31F) });
                }
                else if (icon == "terminal")
                {
                    g.DrawLines(pen, new[] { new PointF(r.X + r.Width * .18F, r.Y + r.Height * .25F),
                        new PointF(r.X + r.Width * .43F, r.Y + r.Height * .5F),
                        new PointF(r.X + r.Width * .18F, r.Y + r.Height * .75F) });
                    g.DrawLine(pen, r.X + r.Width * .55F, r.Y + r.Height * .75F, r.Right - r.Width * .12F, r.Y + r.Height * .75F);
                }
                else if (icon == "up" || icon == "down")
                {
                    var a = icon == "up" ? .65F : .35F;
                    var b = 1F - a;
                    g.DrawLines(pen, new[] { new PointF(r.X + r.Width * .15F, r.Y + r.Height * a),
                        new PointF(r.X + r.Width * .5F, r.Y + r.Height * b),
                        new PointF(r.X + r.Width * .85F, r.Y + r.Height * a) });
                }
                else
                {
                    g.DrawRectangle(pen, r.X + r.Width * .15F, r.Y + r.Height * .15F, r.Width * .7F, r.Height * .7F);
                    g.DrawLine(pen, r.X + r.Width * .3F, r.Y + r.Height * .35F, r.X + r.Width * .7F, r.Y + r.Height * .35F);
                }
            }
        }
    }

    internal sealed class CardLayout : TableLayoutPanel
    {
        internal Color SurfaceColor { get; set; } = Color.White;
        internal Color BorderColor { get; set; } = CodexVisual.Line;
        internal CardLayout()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            CodexVisual.Surface(e.Graphics, ClientRectangle, Font.Height * .65F, SurfaceColor, BorderColor);
            base.OnPaint(e);
        }
    }

    internal sealed class CodexButton : Button
    {
        internal bool Primary { get; set; }
        internal bool Quiet { get; set; }
        internal string Glyph { get; set; }
        private bool _hover;
        internal CodexButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.White;
            ForeColor = CodexVisual.Ink;
            AutoSize = true;
            Padding = new Padding(14, 7, 14, 7);
            Margin = new Padding(0, 0, 8, 0);
            Cursor = Cursors.Hand;
        }
        public override Size GetPreferredSize(Size proposedSize)
        {
            var text = TextRenderer.MeasureText(Text, Font);
            return new Size(text.Width + Padding.Horizontal + (string.IsNullOrEmpty(Glyph) ? 0 : Font.Height + 8),
                Math.Max(text.Height, Font.Height) + Padding.Vertical + 4);
        }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var fill = Primary ? (_hover && Enabled ? Color.FromArgb(42, 57, 76) : CodexVisual.Ink)
                : (_hover && Enabled ? Color.FromArgb(239, 244, 252) : Color.White);
            var color = Primary ? (Enabled ? Color.White : Color.FromArgb(187, 198, 211))
                : (Enabled ? ForeColor : Color.FromArgb(157, 169, 187));
            if (!Quiet) CodexVisual.Surface(e.Graphics, ClientRectangle, Font.Height * .4F, fill, Primary ? fill : CodexVisual.Line);
            var r = new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height);
            if (!string.IsNullOrEmpty(Glyph))
            {
                CodexVisual.Icon(e.Graphics, Glyph, new Rectangle(r.X, (Height - Font.Height) / 2, Font.Height, Font.Height), color);
                r.X += Font.Height + 8; r.Width -= Font.Height + 8;
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, r, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -4, -4), color, fill);
        }
    }

    internal sealed class CodexCheckBox : CheckBox
    {
        internal CodexCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            AutoSize = true;
            ForeColor = CodexVisual.Ink;
            Cursor = Cursors.Hand;
        }
        public override Size GetPreferredSize(Size proposedSize)
        {
            var text = TextRenderer.MeasureText(Text, Font);
            return new Size(text.Width + Font.Height + 12, Math.Max(text.Height, Font.Height) + Padding.Vertical + 4);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var size = Font.Height;
            var box = new Rectangle(1, (Height - size) / 2, size, size);
            var blue = Enabled ? CodexVisual.Blue : CodexVisual.Muted;
            CodexVisual.Surface(e.Graphics, box, size * .18F, Checked ? blue : Color.White, Checked ? blue : Color.FromArgb(168, 182, 203));
            if (Checked)
                using (var pen = new Pen(Color.White, Math.Max(1.5F, size / 9F)))
                    e.Graphics.DrawLines(pen, new[] { new PointF(box.X + size * .22F, box.Y + size * .5F),
                        new PointF(box.X + size * .43F, box.Y + size * .7F), new PointF(box.X + size * .8F, box.Y + size * .28F) });
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(size + 11, 0, Width - size - 11, Height),
                Enabled ? ForeColor : CodexVisual.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -1, -1));
        }
    }

    internal sealed class BadgeLabel : Label
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            CodexVisual.Surface(e.Graphics, ClientRectangle, Height / 2F, BackColor, BackColor);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Color.White)) e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }
    internal sealed class StatusPill : Label
    {
        internal StatusPill() { AutoSize = true; Padding = new Padding(30, 8, 14, 8); }
        protected override void OnPaint(PaintEventArgs e)
        {
            CodexVisual.Surface(e.Graphics, ClientRectangle, Height / 2F, Color.FromArgb(235, 239, 245), Color.FromArgb(235, 239, 245));
            var dot = Math.Max(8, Font.Height / 2);
            using (var brush = new SolidBrush(ForeColor)) e.Graphics.FillEllipse(brush, Padding.Left / 3F, (Height - dot) / 2F, dot, dot);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height),
                ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }
}
