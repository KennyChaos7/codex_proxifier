using NUnit.Framework;
using ProxiFyreUI.Infrastructure;
using System.IO;

namespace ProxiFyre.Tests
{
    [TestFixture]
    public sealed class CodexPathDisplayTests
    {
        [Test]
        public void HidesProfileAndPrioritizesLocalAppDataWithoutChangingActualValue()
        {
            var actual = @"C:\Users\example\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe";
            var display = CodexTargetDiscovery.FormatDisplayPath(actual,
                @"C:\Users\example\AppData\Local", @"C:\Users\example");
            Assert.That(display, Is.EqualTo(@"%LOCALAPPDATA%\Programs\OpenAI\Codex\bin\codex.exe"));
            Assert.That(display, Does.Not.Contain("example"));
            Assert.That(actual, Is.EqualTo(@"C:\Users\example\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe"));
        }

        [Test]
        public void HidesOtherProfileAndLeavesNonpersonalPath()
        {
            Assert.That(CodexTargetDiscovery.FormatDisplayPath(@"C:\Users\other\tools\codex.exe",
                @"C:\Users\example\AppData\Local", @"C:\Users\example"),
                Is.EqualTo(@"C:\Users\<用户>\tools\codex.exe"));
            Assert.That(CodexTargetDiscovery.FormatDisplayPath(@"D:\Tools\codex.exe"),
                Is.EqualTo(@"D:\Tools\codex.exe"));
        }

        [Test]
        public void MasksPathsInErrorText()
        {
            Assert.That(CodexTargetDiscovery.FormatDisplayPath(
                @"Could not open C:\Users\example\AppData\Local\OpenAI\Codex\bin\codex.exe",
                @"C:\Users\example\AppData\Local", @"C:\Users\example"),
                Does.Not.Contain("example"));
        }
    }
}

