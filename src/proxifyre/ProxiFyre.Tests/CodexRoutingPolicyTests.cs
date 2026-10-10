using System;
using NUnit.Framework;
using ProxiFyre.Configuration;
using System.Collections.Generic;

namespace ProxiFyre.Tests
{
    [TestFixture]
    public sealed class CodexRoutingPolicyTests
    {
        [TestCase("")]
        [TestCase("ChatGPT.exe")]
        [TestCase("codex.exe")]
        [TestCase("node.exe")]
        [TestCase(@"C:\Program Files\WindowsApps")]
        [TestCase(@"C:\Users\example")]
        [TestCase(@"C:\Users\example\AppData\Local\OpenAI")]
        [TestCase(@"C:\Tools\notepad.exe")]
        public void RejectsGeneralTargets(string target)
        {
            Assert.That(CodexRoutingPolicy.ValidateTargets(new[] { target }).HasErrors, Is.True);
        }

        [TestCase(@"C:\Program Files\WindowsApps\OpenAI.Codex_")]
        [TestCase(@"C:\Users\example\AppData\Local\OpenAI\Codex\")]
        [TestCase(@"C:\Users\example\AppData\Local\Programs\OpenAI\Codex\")]
        [TestCase(@"D:\Portable\codex.exe")]
        public void AcceptsScopedTargets(string target)
        {
            Assert.That(CodexRoutingPolicy.ValidateTargets(new[] { target }).HasErrors, Is.False);
        }

        [Test]
        public void CreatesOneRuleAndUsesOnlyProvidedTargets()
        {
            var options = new ProxyRule { Socks5ProxyEndpoint = "127.0.0.1:7890",
                AppNames = new List<string> { "" } };
            var configuration = CodexRoutingPolicy.CreateConfiguration(
                new[] { @"C:\Tools\codex.exe", @"c:\tools\codex.exe" }, options, true, "Info");
            Assert.That(configuration.Proxies.Count, Is.EqualTo(1));
            Assert.That(configuration.Proxies[0].AppNames, Is.EqualTo(new[] { @"C:\Tools\codex.exe" }));
            Assert.That(configuration.Proxies[0].Socks5ProxyEndpoint, Is.EqualTo("127.0.0.1:7890"));
            Assert.That(configuration.BypassLan, Is.True);
            Assert.That(configuration.LogLevel, Is.EqualTo("Info"));
        }

        [Test]
        public void RejectsSecondRuleAndCatchAll()
        {
            var configuration = new ProxiFyreConfiguration();
            configuration.Proxies.Add(new ProxyRule { AppNames = new List<string> { "" } });
            Assert.That(CodexRoutingPolicy.ValidateConfiguration(configuration).HasErrors, Is.True);
            configuration.Proxies[0].AppNames = new List<string> { @"C:\Tools\codex.exe" };
            configuration.Proxies.Add(new ProxyRule { AppNames = new List<string> { @"C:\Tools\codex.exe" } });
            Assert.That(CodexRoutingPolicy.ValidateConfiguration(configuration).HasErrors, Is.True);
        }

        [Test]
        public void PreservesAbsoluteTargetsWithoutResolvingServiceEnvironment()
        {
            var configuration = CodexRoutingPolicy.CreateConfiguration(
                new[] { @"C:\Users\example\AppData\Local\OpenAI\Codex" },
                new ProxyRule { Socks5ProxyEndpoint = "127.0.0.1:7890" }, true, "Info");
            Assert.That(configuration.Proxies[0].AppNames[0],
                Is.EqualTo(@"C:\Users\example\AppData\Local\OpenAI\Codex\"));
            Assert.That(CodexRoutingPolicy.ValidateConfiguration(configuration).HasErrors, Is.False);
        }
    }
}

