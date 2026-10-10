using NUnit.Framework;
using ProxiFyreUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ProxiFyre.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public sealed class HighDpiLayoutTests
    {
        [TestCase(false), TestCase(true)]
        public void CodexWindowActionsRemainUsableAtTwoHundredPercent(bool authentication)
        {
            using (var form = new MainForm())
            using (var scaled = ScaleToTwoHundredPercentWithAuthentication(form, authentication))
            {
                Assert.That(form.Text, Is.EqualTo("Codex Proxy"));
                AssertControlsAreContained(GetControlTree(scaled.Surface).Where(control =>
                    control is Button || control is CheckBox || control is ComboBox));
                Assert.That(FindControl<TextBox>(form, "passwordTextBox").UseSystemPasswordChar, Is.True);
                Assert.That(FindControl<TextBox>(form, "logTextBox").ReadOnly, Is.True);
            }
        }

        [Test]
        public void RenderCodexWindowForVisualReview()
        {
            Application.EnableVisualStyles();
            using (var form = new PreviewMainForm())
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.Show();
                Application.DoEvents();
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var path = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, "CodexProxy-GUI.png");
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                TestContext.AddTestAttachment(path);
            }
        }
        [Test]
        public void DialogActionsRemainInsideTheirWindowsAtTwoHundredPercent()
        {
            var forms = new Form[]
            {
                new CodexPathPickerForm(new ProxiFyreUI.Infrastructure.CodexPathCandidate[0])
            };
            foreach (var form in forms)
            {
                using (form)
                using (var scaledLayout = ScaleToTwoHundredPercent(form))
                {
                    AssertControlsAreContained(GetControlTree(scaledLayout.Surface)
                        .Where(control => control is Button));
                }
            }
        }

        [Test]
        public void EnabledAuthenticationWithoutCredentialsCannotBeSaved()
        {
            using (var form = new MainForm())
            {
                FindControl<CheckBox>(form, "authenticationCheckBox").Checked = true;
                var method = typeof(MainForm).GetMethod("ReadConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
                var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(form, null));
                Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(error.InnerException.Message, Does.Contain("用户名和密码"));
            }
        }

        [Test]
        public void RenderCandidateCardsForVisualReview()
        {
            Application.EnableVisualStyles();
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var candidates = new[] {
                new ProxiFyreUI.Infrastructure.CodexPathCandidate {
                    ExecutablePath = System.IO.Path.Combine(local, @"OpenAI\Codex\bin\2e5e00daee91c61d\codex.exe"),
                    SourceLabel = "正在运行" },
                new ProxiFyreUI.Infrastructure.CodexPathCandidate {
                    ExecutablePath = System.IO.Path.Combine(local, @"Programs\OpenAI\Codex\bin\codex.exe"),
                    SourceLabel = "PATH 命令" }
            };
            using (var form = new CodexPathPickerForm(candidates))
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.Show();
                Application.DoEvents();
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var path = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, "CodexProxy-PathPicker.png");
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                TestContext.AddTestAttachment(path);
            }
        }

        [Test]
        public void LogBodyCanBeCollapsedAndRestored()
        {
            using (var form = new PreviewMainForm())
            {
                form.Show();
                Application.DoEvents();
                var toggle = FindControl<Button>(form, "collapseLogsButton");
                var body = FindControl<Panel>(form, "logBodyPanel");
                Assert.That(body.Visible, Is.True);
                toggle.PerformClick();
                Assert.That(body.Visible, Is.False);
                toggle.PerformClick();
                Assert.That(body.Visible, Is.True);
                Assert.That(FindControl<TextBox>(form, "logTextBox").ReadOnly, Is.True);
            }
        }

        private static ScaledLayout ScaleToTwoHundredPercentWithAuthentication(MainForm form, bool authentication)
        {
            FindControl<CheckBox>(form, "authenticationCheckBox").Checked = authentication;
            return ScaleToTwoHundredPercent(form);
        }
        private sealed class PreviewMainForm : MainForm
        {
            // Render the actual controls without connecting to a host service or opening
            // engine resolution errors. Runtime acceptance remains a separate real test.
            protected override void OnShown(EventArgs e)
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var cli = System.IO.Path.Combine(local, @"OpenAI\Codex\bin\codex.exe");
                var targets = (List<string>)typeof(MainForm).GetField("_targets", flags).GetValue(this);
                targets.Add(@"C:\Program Files\WindowsApps\OpenAI.Codex_");
                targets.Add(cli);
                typeof(MainForm).GetField("_selectedCliPath", flags).SetValue(this, cli);
                var workspace = (ProxiFyreUI.Infrastructure.ConfigurationWorkspace)typeof(MainForm).GetField("_workspace", flags).GetValue(this);
                typeof(ProxiFyreUI.Infrastructure.ConfigurationWorkspace).GetProperty("Configuration").SetValue(workspace,
                    ProxiFyre.Configuration.CodexRoutingPolicy.CreateConfiguration(targets,
                        new ProxiFyre.Configuration.ProxyRule { Socks5ProxyEndpoint = "127.0.0.1:7890", Socks5Transport = "TCP" }, true, "Info"));
                typeof(MainForm).GetField("_serviceStatus", flags).SetValue(this,
                    new ProxiFyreUI.Infrastructure.ServiceStatusInfo(ProxiFyreUI.Infrastructure.ProxiFyreServiceState.Stopped));
                typeof(MainForm).GetMethod("BindTargets", flags).Invoke(this, null);
                typeof(MainForm).GetMethod("UpdateState", flags).Invoke(this, null);
                FindControl<TextBox>(this, "endpointTextBox").Select(0, 0);
            }
        }

        private static ScaledLayout ScaleToTwoHundredPercent(Form form)
        {
            Assert.That(form.AutoScaleMode, Is.EqualTo(AutoScaleMode.Dpi));
            var originalClientSize = form.ClientSize;
            var originalFontHeight = form.Font.Height;
            var layoutSurface = new Panel
            {
                Font = form.Font,
                Padding = form.Padding,
                RightToLeft = form.RightToLeft,
                Size = originalClientSize
            };
            var topLevelControls = form.Controls.Cast<Control>().ToArray();
            form.SuspendLayout();
            layoutSurface.SuspendLayout();
            try
            {
                layoutSurface.Controls.AddRange(topLevelControls);
                for (var index = 0; index < topLevelControls.Length; index++)
                    layoutSurface.Controls.SetChildIndex(topLevelControls[index], index);
            }
            finally
            {
                form.ResumeLayout(false);
                layoutSurface.ResumeLayout(false);
            }

            var fontSnapshots = GetControlTree(layoutSurface)
                .Select(control => new KeyValuePair<Control, Font>(control, control.Font))
                .ToArray();

            // CI normally runs at 96 DPI. At 192 DPI, point fonts retain their point size but
            // occupy roughly twice as many device pixels, so double both geometry and the
            // 96-DPI font metrics to exercise clipping without changing the runner display.
            // Use a non-window layout surface because Form caps its bounds at the desktop's
            // maximum tracking size, which is only 1024x768 on the hosted Windows runner.
            var scaledFonts = new List<Font>(fontSnapshots.Length);
            try
            {
                layoutSurface.Scale(new SizeF(2F, 2F));
                foreach (var snapshot in fontSnapshots.Reverse())
                {
                    var originalFont = snapshot.Value;
                    var scaledFont = new Font(originalFont.FontFamily,
                        originalFont.SizeInPoints * 2F, originalFont.Style, GraphicsUnit.Point,
                        originalFont.GdiCharSet, originalFont.GdiVerticalFont);
                    snapshot.Key.Font = scaledFont;
                    scaledFonts.Add(scaledFont);
                }
                layoutSurface.PerformLayout();

                Assert.Multiple(() =>
                {
                    Assert.That(layoutSurface.ClientSize.Width,
                        Is.GreaterThanOrEqualTo(originalClientSize.Width * 2 - 2));
                    Assert.That(layoutSurface.ClientSize.Height,
                        Is.GreaterThanOrEqualTo(originalClientSize.Height * 2 - 2));
                    Assert.That(layoutSurface.Font.Height,
                        Is.GreaterThanOrEqualTo(originalFontHeight * 2 - 1));
                });
                return new ScaledLayout(layoutSurface, scaledFonts.ToArray());
            }
            catch
            {
                layoutSurface.Dispose();
                DisposeFonts(scaledFonts);
                throw;
            }
        }

        private static IEnumerable<Control> GetControlTree(Control root)
        {
            yield return root;
            foreach (Control child in root.Controls)
            {
                foreach (var descendant in GetControlTree(child))
                    yield return descendant;
            }
        }

        private static void DisposeFonts(IEnumerable<Font> fonts)
        {
            foreach (var font in fonts)
                font.Dispose();
        }

        private static T FindControl<T>(Control root, string name) where T : Control
        {
            FieldInfo field = null;
            for (var type = root.GetType(); type != null && field == null; type = type.BaseType)
                field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected a field named '{0}'.", name);
            var control = field.GetValue(root) as T;
            Assert.That(control, Is.Not.Null, "Expected '{0}' to be a {1}.", name,
                typeof(T).Name);
            return control;
        }

        private static void AssertControlsAreContained(System.Collections.Generic.IEnumerable<Control> controls)
        {
            foreach (var control in controls)
            {
                Assert.That(control.Parent, Is.Not.Null);
                Assert.That(control.Parent.ClientRectangle.Contains(control.Bounds), Is.True,
                    "Control '{0}' is clipped by '{1}' at 200% scaling.",
                    control.Name, control.Parent.Name);
            }
        }

        private sealed class ScaledLayout : IDisposable
        {
            private readonly Control _layoutSurface;
            private readonly Font[] _fonts;

            internal ScaledLayout(Control layoutSurface, Font[] fonts)
            {
                _layoutSurface = layoutSurface;
                _fonts = fonts;
            }

            internal Control Surface => _layoutSurface;

            public void Dispose()
            {
                _layoutSurface.Dispose();
                DisposeFonts(_fonts);
            }
        }
    }
}
