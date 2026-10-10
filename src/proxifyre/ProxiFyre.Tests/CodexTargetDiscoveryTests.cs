using NUnit.Framework;
using ProxiFyreUI.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace ProxiFyre.Tests
{
    [TestFixture]
    public sealed class CodexTargetDiscoveryTests
    {
        private string _root;
        [SetUp] public void SetUp() { _root = Path.Combine(Path.GetTempPath(), "CodexDiscovery-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_root); }
        [TearDown] public void TearDown() { Directory.Delete(_root, true); }

        [Test]
        public void KeepsSourceOrderMergesDuplicatesAndOmitsMissingFiles()
        {
            var path = Path.Combine(_root, "codex.exe");
            File.WriteAllText(path, "fixture");
            var candidates = CodexTargetDiscovery.NormalizeCandidates(new[] {
                new CodexPathCandidate { ExecutablePath = path, SourceLabel = "正在运行" },
                new CodexPathCandidate { ExecutablePath = path, SourceLabel = "PATH 命令" },
                new CodexPathCandidate { ExecutablePath = Path.Combine(_root, "missing", "codex.exe"), SourceLabel = "npm 安装" }
            });
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].ExecutablePath, Is.EqualTo(path));
            Assert.That(candidates[0].SourceLabel, Is.EqualTo("正在运行 · PATH 命令"));
        }

        [Test]
        public void FindsNativeNpmPayloadInsteadOfNodeOrShim()
        {
            var native = Path.Combine(_root, "node_modules", "@openai", "codex-win32-x64",
                "vendor", "x86_64-pc-windows-msvc", "codex", "codex.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(native));
            File.WriteAllText(native, "fixture");
            File.WriteAllText(Path.Combine(_root, "codex.cmd"), "node codex.js");
            File.WriteAllText(Path.Combine(_root, "node.exe"), "fixture");
            Assert.That(CodexTargetDiscovery.FindNpmExecutables(_root).ToArray(), Is.EqualTo(new[] { native }));
        }
    }
}

