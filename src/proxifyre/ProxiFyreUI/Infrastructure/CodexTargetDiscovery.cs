using ProxiFyre.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProxiFyreUI.Infrastructure
{
    public sealed class CodexPathCandidate
    {
        public string ExecutablePath { get; set; }
        public string DisplayPath { get; set; }
        public string SourceLabel { get; set; }
    }

    public sealed class CodexTargetDiscovery
    {
        private static readonly string[] DedicatedRoots =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "OpenAI", "Codex")
        };

        public IReadOnlyList<string> Discover()
        {
            var targets = new List<string>();
            foreach (var path in RunningExecutables())
            {
                var marker = path.IndexOf(@"\WindowsApps\OpenAI.Codex_", StringComparison.OrdinalIgnoreCase);
                if (marker >= 0) targets.Add(path.Substring(0, marker) + @"\WindowsApps\OpenAI.Codex_");
            }
            foreach (var packagePath in InstalledDesktopPackages())
            {
                if (Path.GetFileName(packagePath).StartsWith("OpenAI.Codex_", StringComparison.OrdinalIgnoreCase))
                    targets.Add(Path.Combine(Path.GetDirectoryName(packagePath), "OpenAI.Codex_"));
            }
            targets.AddRange(DedicatedRoots.Where(Directory.Exists).Select(path => path + "\\"));
            targets.AddRange(FindCliCandidates().Select(candidate => candidate.ExecutablePath));
            return targets.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public IReadOnlyList<CodexPathCandidate> FindCliCandidates()
        {
            var candidates = new List<CodexPathCandidate>();
            foreach (var path in RunningExecutables())
                candidates.Add(Candidate(path, "正在运行"));
            var directories = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator).Select(value => value.Trim().Trim('"'))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var directory in directories)
                candidates.Add(Candidate(Path.Combine(directory, "codex.exe"), "PATH 命令"));
            foreach (var root in DedicatedRoots)
                candidates.AddRange(FindExecutables(root).Select(path => Candidate(path, "专用安装目录")));
            foreach (var directory in directories)
                candidates.AddRange(FindNpmExecutables(directory).Select(path => Candidate(path, "npm 安装")));
            return NormalizeCandidates(candidates);
        }

        internal static IReadOnlyList<CodexPathCandidate> NormalizeCandidates(IEnumerable<CodexPathCandidate> candidates)
        {
            var result = new List<CodexPathCandidate>();
            foreach (var candidate in candidates)
            {
                if (candidate == null || !File.Exists(candidate.ExecutablePath) ||
                    !string.Equals(Path.GetFileName(candidate.ExecutablePath), "codex.exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                var path = Path.GetFullPath(candidate.ExecutablePath);
                if (CodexRoutingPolicy.ValidateTargets(new[] { path }).HasErrors) continue;
                var existing = result.FirstOrDefault(value =>
                    string.Equals(value.ExecutablePath, path, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                    result.Add(new CodexPathCandidate { ExecutablePath = path,
                        DisplayPath = path, SourceLabel = candidate.SourceLabel });
                else if (!existing.SourceLabel.Split(new[] { " · " }, StringSplitOptions.None).Contains(candidate.SourceLabel))
                    existing.SourceLabel += " · " + candidate.SourceLabel;
            }
            return result;
        }

        internal static IEnumerable<string> FindNpmExecutables(string commandDirectory)
        {
            var openAi = Path.Combine(commandDirectory, "node_modules", "@openai");
            if (!Directory.Exists(openAi)) return new string[0];
            try
            {
                return Directory.GetDirectories(openAi, "codex*")
                    .SelectMany(FindExecutables).ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return new string[0];
            }
        }

        private static IEnumerable<string> FindExecutables(string directory)
        {
            if (!Directory.Exists(directory)) return new string[0];
            try { return Directory.GetFiles(directory, "codex.exe", SearchOption.AllDirectories); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            { return new string[0]; }
        }

        private static IEnumerable<string> RunningExecutables()
        {
            var result = new List<string>();
            foreach (var name in new[] { "codex", "ChatGPT", "Codex" })
                foreach (var process in Process.GetProcessesByName(name))
                    using (process)
                    {
                        try
                        {
                            var path = process.MainModule?.FileName;
                            if (!string.IsNullOrWhiteSpace(path)) result.Add(path);
                        }
                        catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException ||
                                                   ex is NotSupportedException)
                        { }
                    }
            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static IEnumerable<string> InstalledDesktopPackages()
        {
            var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell", "v1.0", "powershell.exe");
            var info = new ProcessStartInfo(powershell,
                "-NoProfile -NonInteractive -Command \"Get-AppxPackage -Name OpenAI.Codex | Select-Object -ExpandProperty InstallLocation\"")
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            try
            {
                using (var process = Process.Start(info))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode == 0
                        ? output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(path => path.Trim()).Where(Directory.Exists).ToList()
                        : new List<string>();
                }
            }
            catch (Win32Exception) { return new string[0]; }
        }

        private static CodexPathCandidate Candidate(string path, string source)
        {
            return new CodexPathCandidate { ExecutablePath = path, SourceLabel = source };
        }
    }
}

