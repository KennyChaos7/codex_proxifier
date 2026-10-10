using NUnit.Framework;
using ProxiFyreUI.Forms;
using ProxiFyreUI.Infrastructure;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ProxiFyre.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public sealed class CodexPathPickerTests
    {
        [Test]
        public void SortedCandidateStillReturnsItsRealPath()
        {
            var candidates = new[] {
                new CodexPathCandidate { ExecutablePath = @"C:\Users\Hidden\z\codex.exe", SourceLabel = "PATH" },
                new CodexPathCandidate { ExecutablePath = @"C:\Users\Hidden\a\codex.exe", SourceLabel = "npm" }
            };
            using (var form = new CodexPathPickerForm(candidates))
            {
                var grid = (DataGridView)typeof(CodexPathPickerForm).GetField("candidatesGrid", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                grid.Sort(grid.Columns[0], ListSortDirection.Ascending);
                grid.CurrentCell = grid.Rows[0].Cells[0];
                Assert.That(grid.Rows[0].Cells[0].Value.ToString(), Does.Not.Contain("Hidden"));
                Assert.That(form.SelectedPath, Is.Null);
                typeof(CodexPathPickerForm).GetMethod("UsePath", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { null, System.EventArgs.Empty });
                Assert.That(form.SelectedPath, Is.EqualTo(candidates[1].ExecutablePath));
                Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            }
        }
    }
}
