# Codex Proxy

基于 ProxiFyre 2.6.2 定制的 Windows 独立代理工具。提供 `CodexProxy.exe` 图形界面及 `CodexProxyService` 后台服务，支持 Codex 桌面应用、专用后台进程和独立运行的 Windows 原生 Codex CLI。

## 安装与使用

支持 Windows 10/11 x64，需要 .NET Framework 4.7.2 或更高版本、Visual C++ 运行库和 Windows Packet Filter 驱动。完整 setup 安装器包含依赖处理；单独 MSI 需要预先满足依赖。

1. 从本仓库 Actions 的 `CodexProxy-0.1.0-x64` 产物取得 `CodexProxy-0.1.0-win-x64-setup.exe`。
2. 运行安装器。应用默认安装到 `C:\Program Files\CodexProxy`，与官方 ProxiFyre 使用不同服务及安装器身份。
3. 启动 Codex Proxy，完成 Windows 管理员权限提示。
4. 确保本地 SOCKS5 代理已经启动，填写监听地址，默认 `127.0.0.1:7890`。只有 HTTP 的端口不能使用；mixed-port 必须支持 SOCKS5。
5. 检查已识别的 Codex 路径。CLI 未识别时点击「选择 CLI 路径」，先选择候选，再按需浏览其他位置，选择实际的 `codex.exe`。
6. 点击「启动代理」；更改设置后点击「保存并应用」，运行中的服务会重启以加载新配置。

认证可选，开启时需填写用户名和密码；默认绕过局域网、日志级别 Info。配置保存在安装目录的 `app-config.json`，使用真实绝对路径。界面路径提示用 `%LOCALAPPDATA%`、`%USERPROFILE%` 或 `<用户>` 隐藏用户信息。日志及复制诊断也会遮盖路径和认证信息。

「停止代理」停止后台服务。关闭窗口默认进入托盘，双击托盘图标恢复；托盘的「退出」只退出 GUI，已经运行的后台服务继续工作。窗口偏好保存在 `%LOCALAPPDATA%\CodexProxy\ui-settings.json`。

## 代理范围

只接受一条 Codex 专用规则：Codex WindowsApps 应用包前缀、`OpenAI\Codex\` 专用目录或原生 `codex.exe` 的绝对路径。拒绝空规则、catch-all、普通应用名和通用 ChatGPT 匹配；服务在启动代理引擎前再次执行同一范围校验。

候选优先来自正在运行的 Codex、PATH、专用安装目录和 npm 安装目录。仅选择现存的原生 `codex.exe`，不执行 `codex.cmd`、Node 或 PowerShell 启动脚本。更新 Codex 后可点击「重新识别」并保存应用。WSL、远端 SSH 主机和容器中的 CLI 不在 Windows 本机进程代理范围内。

此工具按安装路径匹配进程，不验证 Codex 的数字签名。它不会调整系统代理或其他工具的 TUN 设置；其他应用是否直连也取决于这些既有网络设置。

## 从源码构建

源码位于 `src/proxifyre`，上游版本与提交见其中的 `UPSTREAM.md`。需要 VS 2022（v143、C++/CLI、.NET 4.7.2 targeting pack）、NuGet CLI、.NET 8 SDK、PowerShell 7，以及 vcpkg 的 `ms-gsl:x64-windows` 和 `boost-pool:x64-windows`。

```powershell
./scripts/Build-CodexProxy.ps1 -Configuration Debug
./scripts/Build-CodexProxy.ps1 -Configuration Release -BuildInstaller
./scripts/Test-CodexProxy.ps1 -Configuration Release
```

Windows 构建流水线生成 Debug/Release 程序、MSI/setup，并执行托管测试、原生路由测试与安装包合同检查。当前实际验证结果及尚未完成的运行验收请查看步骤记录。

- [实施计划](docs/superpowers/plans/2026-10-10-codex-only-proxifyre-plan.md)
- [实施记录](docs/superpowers/execution/2026-10-10-codex-only-proxifyre/)
- [Windows 构建结果](https://github.com/KennyChaos7/codex_proxifier/actions/workflows/build-codexproxy.yml)

## 许可证

保留 ProxiFyre 的 AGPL-3.0 许可证及版权声明，见 [LICENSE](src/proxifyre/LICENSE)。定制源码、构建脚本及改动记录随仓库提供。发行产物尚未进行代码签名。

仓库早期的 `scripts/Apply-CodexConfig.ps1` 和旧配置属于官方 ProxiFyre 配置方案；新版本安装和使用不需要执行该脚本。

