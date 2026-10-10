using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProxiFyre.Configuration
{
    public static class CodexRoutingPolicy
    {
        public static ValidationResult ValidateTargets(IEnumerable<string> targets)
        {
            var values = targets == null ? new List<string>() : targets.ToList();
            var issues = new List<ValidationIssue>();
            if (values.Count == 0)
                issues.Add(Error("Codex.TargetsMissing", "请先选择或识别 Codex 路径。", "proxies[0].appNames"));
            for (var i = 0; i < values.Count; i++)
            {
                if (!IsAllowedTarget(values[i]))
                    issues.Add(Error("Codex.TargetNotAllowed",
                        "仅支持 Codex 专用目录、桌面应用包和原生 codex.exe 的绝对路径。",
                        "proxies[0].appNames[" + i + "]"));
            }
            return new ValidationResult(issues);
        }

        public static ProxiFyreConfiguration CreateConfiguration(IEnumerable<string> targets,
            ProxyRule proxyOptions, bool bypassLan, string logLevel)
        {
            var values = targets == null ? new List<string>() : targets.ToList();
            var validation = ValidateTargets(values);
            if (validation.HasErrors)
                throw new ArgumentException(string.Join(Environment.NewLine,
                    validation.Errors.Select(issue => issue.Message)), nameof(targets));
            if (proxyOptions == null) throw new ArgumentNullException(nameof(proxyOptions));
            var rule = new ProxyRule
            {
                AppNames = values.Select(NormalizeTarget).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Socks5ProxyEndpoint = proxyOptions.Socks5ProxyEndpoint,
                Username = proxyOptions.Username,
                Password = proxyOptions.Password,
                Socks5Transport = proxyOptions.Socks5Transport ?? "tcp",
                TlsServerName = proxyOptions.TlsServerName,
                TlsPinnedSha256 = proxyOptions.TlsPinnedSha256,
                TlsAllowInvalidCertificate = proxyOptions.TlsAllowInvalidCertificate,
                SupportedProtocols = proxyOptions.SupportedProtocols == null
                    ? new List<string> { "TCP", "UDP" } : new List<string>(proxyOptions.SupportedProtocols),
                SupportedAddressFamilies = proxyOptions.SupportedAddressFamilies == null
                    ? new List<string> { "IPv4", "IPv6" } : new List<string>(proxyOptions.SupportedAddressFamilies)
            };
            return new ProxiFyreConfiguration
            {
                LogLevel = string.IsNullOrWhiteSpace(logLevel) ? "Info" : logLevel,
                BypassLan = bypassLan,
                Proxies = new List<ProxyRule> { rule }
            };
        }

        public static ValidationResult ValidateConfiguration(ProxiFyreConfiguration configuration)
        {
            if (configuration?.Proxies == null || configuration.Proxies.Count != 1 ||
                configuration.Proxies[0] == null)
                return new ValidationResult(new[] { Error("Codex.SingleProxyRequired",
                    "Codex Proxy 只支持一个 Codex 专用代理规则。", "proxies") });
            return ValidateTargets(configuration.Proxies[0].AppNames);
        }

        private static bool IsAllowedTarget(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var path = value.Trim().Replace('/', '\\');
            if (!(path.Length > 2 && char.IsLetter(path[0]) && path[1] == ':' && path[2] == '\\') &&
                !path.StartsWith(@"\\", StringComparison.Ordinal)) return false;
            try
            {
                path = Path.GetFullPath(path).TrimEnd('\\');
                if (path.IndexOf(@"\WindowsApps\OpenAI.Codex_", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (path.EndsWith(@"\OpenAI\Codex", StringComparison.OrdinalIgnoreCase))
                    return true;
                return string.Equals(Path.GetFileName(path), "codex.exe", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException ||
                                       ex is PathTooLongException)
            {
                return false;
            }
        }

        private static string NormalizeTarget(string value)
        {
            var path = Path.GetFullPath(value.Trim().Replace('/', '\\')).TrimEnd('\\');
            return path.EndsWith(@"\OpenAI\Codex", StringComparison.OrdinalIgnoreCase)
                ? path + "\\" : path;
        }

        private static ValidationIssue Error(string code, string message, string path)
        {
            return new ValidationIssue(ValidationSeverity.Error, code, message, path, 0);
        }
    }
}

