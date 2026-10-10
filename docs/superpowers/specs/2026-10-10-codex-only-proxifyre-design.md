# Codex Proxy 源码定制设计提案

日期：2026-10-10
状态：已获实施及逐步提交推送授权；按详细 plan 记录实际进度。
工作分支：custorm

## 目标与最低验收

将 ProxiFyre 源码定制为独立 Windows x64 产品 Codex Proxy。产品包含专用 GUI 和后台服务，只代理 Codex 桌面应用、专用后台及独立运行的 Windows 原生 Codex CLI。

最低验收：可重复构建独立安装包；桌面与 CLI 各完成一次真实请求并取得应用响应，日志和连接记录能对应到所配置的 SOCKS5 端点；未匹配的程序不由本产品代理；GUI 的启动、停止、应用配置和托盘恢复可用；实际运行状态与 Windows 服务状态一致。

“只针对 Codex”按受支持的安装路径和专用可执行文件定义，不是基于二进制签名的身份鉴定，也不承诺隔离故意伪装成 Codex 的程序。

## 已核实的事实

- 当前仓库主要是官方安装版的使用文档、配置和脚本，尚无 ProxiFyre 源码；当前分支已经是 custorm。
- 官方 ProxiFyreService 当前处于 Running。旧配置含通用 ChatGPT.exe 匹配，不符合本次只代理 Codex 的目标。
- 上游基线为 wiresock/proxifyre v2.6.2，对应 commit eef55ba96da9bcb21b4c39140e225f3f3a316755。
- 上游 GUI 是原生启动器加托管 WinForms DLL；后台是 .NET Framework 程序，通过 socksify.dll 的 C++/CLI 组件调用网络核心。GUI 不处理数据包。
- 上游 GUI 和服务控制代码检查文件名、版本资源、依赖文件和配置；Release 还要求受保护安装目录。只修改窗口标题或 EXE 文件名不足以形成独立产品。
- 上游网络匹配中，有路径分隔符的项按完整进程路径的片段匹配，空字符串是全应用规则。Codex 专用策略必须拒绝空项、通用进程名和过宽目录。
- 本机 Codex 桌面包内部程序名为 ChatGPT.exe，路径位于 WindowsApps/OpenAI.Codex_ 包目录。
- 本机存在 LocalAppData/OpenAI/Codex 专用后台目录，以及 LocalAppData/Programs/OpenAI/Codex/bin/codex.exe 的命令入口。
- 官方 Codex npm 启动器最终启动原生 codex.exe。代理其真实子进程无需代理所有 node.exe。
- 目前只找到 .NET Framework 自带的旧 MSBuild/csc；在已检查的 PATH 和标准安装目录中未找到 VS 2022 C++ 工具链、NuGet CLI 和 .NET SDK。尚不能断言源码可在当前环境构建。
- 上游采用 AGPL-3.0；源码导入与交付保留原许可证、版权及修改说明。

## 方案选择

采用源码派生、精简现有 WinForms GUI、复用现有后台服务和网络核心。

直接包装官方安装版虽然快，但依赖原产品的服务与安装，不满足独立程序目标。另起 WPF、Avalonia 或 Electron GUI 会增加新技术栈和服务控制重写；首版没有此需求。自研内核驱动或转发引擎不属于本次范围。

源码固定导入 src/proxifyre/，由本仓库追踪，不引入嵌套 .git 或 submodule。内部项目文件和命名空间尽量保留，改动集中在产品输出身份、Codex 策略、GUI 和安装器。UPSTREAM.md 记录来源及基线，后续按需求手动吸收上游变更。

## 产品身份与运行模型

| 项目 | 首版定义 |
| --- | --- |
| 产品名称 | Codex Proxy |
| 产品版本 | 0.1.0；与上游基线版本分开记录 |
| GUI 启动程序 | CodexProxy.exe |
| 托管 GUI | CodexProxy.Managed.dll |
| 后台程序 | CodexProxy.Service.exe |
| Windows 服务 | CodexProxyService |
| 默认安装目录 | Program Files/CodexProxy |
| 生效配置 | 安装目录内 app-config.json |
| 日志 | 安装目录内 logs/ |
| GUI 偏好 | LocalAppData/CodexProxy/ui-settings.json |
| 防火墙规则 | Codex Proxy (TCP-In)、Codex Proxy (UDP-In)，限定后台程序 |
| MSI、Bundle 身份 | 各使用新 UpgradeCode；不得复用上游产品升级身份 |

共享库 ProxiFyre.Configuration.dll、socksify.dll 和第三方依赖可保留原名称，在产品私有安装目录内使用，不依赖官方 ProxiFyre 的程序目录或服务。

复用 Windows Packet Filter/NDISRD 驱动及 VC++ 运行库，不复制或改名驱动。卸载本产品不移除共享驱动。

服务按上游方式以 LocalSystem 运行，默认手动启动。GUI 关闭窗口时隐藏到托盘；从托盘退出 GUI 时服务继续运行。需要停止代理时明确使用“停止代理”。

## Codex 识别与配置的唯一来源

app-config.json 是后台生效配置的唯一来源，延续上游 schema。不增加一份持久化“业务设置”再双向同步。GUI 偏好文件只存窗口和日志视图等偏好。

GUI 发现并保存绝对路径；服务不依据 LocalSystem 的 LOCALAPPDATA、PATH 或登录用户猜测目标。生成的配置只有一个 SOCKS5 规则，appNames 由共享 Codex 策略生成和验证。

受支持目标：

1. 桌面 MSIX：从真实安装位置取得 WindowsApps 的根，生成以 OpenAI.Codex_ 为包名前缀的匹配项；不使用通用 ChatGPT.exe。
2. 专用运行时：实际存在的 LocalAppData/OpenAI/Codex/ 和 LocalAppData/Programs/OpenAI/Codex/，使用带结尾分隔符的完整专用目录。
3. 原生 CLI：PATH、正在运行的 Codex 进程或常见 npm 安装布局中的真实 codex.exe 完整路径。
4. 自动发现未覆盖的包管理器或自定义安装：允许用户选择真实 codex.exe；不接受 codex.cmd、codex.ps1、codex.js 作为网络进程规则。

共享策略拒绝：空列表或空字符串、ChatGPT.exe/codex.exe 等裸名称、node.exe/python.exe/git.exe、WindowsApps 根、用户目录或整个 OpenAI 根目录、第二条代理规则和全应用规则。

GUI 自动发现负责便利性；共享策略负责目标范围。后台加载配置后、创建 Socksifier 和关联进程规则前，调用同一策略检查；外部 JSON 不符合范围时启动失败并记录具体原因。

设置项：SOCKS5 地址、可选认证、LAN 直连、日志级别。默认 127.0.0.1:7890、bypassLan=true、Info；保留上游 TCP/UDP、IPv4/IPv6 能力及其实际限制，不以复选框承诺未经验证的 DNS/UDP 行为。

CLI 更换安装位置后刷新并重新应用路径。桌面包版本目录变化由包名前缀适配。GUI 中展示已发现目标及覆盖状态，不以“服务运行中”代替转发结果。

## GUI 最小范围

主窗口显示代理状态、SOCKS5 地址、Codex 桌面和 CLI 的识别结果，以及“启动代理”“停止代理”“保存并应用”“刷新 Codex 路径”。

日志与简要诊断保留。诊断只显示运行所需的路径、版本、驱动及服务状态；认证密码不出现在日志或诊断。托盘提供打开窗口、启动、停止、退出。

移除通用规则新增/删除/排序、任意进程选择、全应用代理和多代理规则入口。复用服务控制、配置原子保存、日志跟随、单实例和托盘行为，不重写其状态机。

界面采用中文产品文案，不写 Agent 的计划、实现过程和开发汇报。


### 未设置路径时的候选选择

Codex CLI 路径未填写时，点击“选择路径”先打开候选列表，不直接打开空文件浏览框。候选来自正在运行的 codex.exe、PATH 命令入口、OpenAI 专用安装目录和常见 npm 原生文件布局，按上述来源顺序展示；只列出当前存在且可作为原生 Codex 目标的文件，按完整路径去重，同一路径合并来源。后台进程路径标明其来源，不能把发现某个后台路径等同于识别了用户平时的 CLI 命令入口。

每项显示隐藏用户信息的路径和来源，例如“正在运行”“PATH 命令”“常见安装目录”。可突出首项供选择，但不自动写入配置。用户点击“使用此路径”后回填，点击“保存并应用”后生效；取消保持原值。

“浏览其他位置…”始终可用。没有候选时显示“未找到 Codex CLI，请选择 codex.exe”，直接提供文件选择入口。浏览窗口有已确认存在的候选目录时从该目录打开；无候选时从当前用户目录打开，不猜造不存在的默认目录。不扫描整块磁盘、不为候选引入后台监控或持久化缓存。

### 路径显示与启动权限

候选列表、已选路径及相关路径错误提示不展示本电脑的实际用户名。当前用户的 LocalAppData 前缀显示为 %LOCALAPPDATA%，其余用户目录前缀显示为 %USERPROFILE%；先替换更具体的 LocalAppData。不在悬浮提示中重新显示实际用户名。内部仍保存真实绝对路径，确认选择、保存配置及后台路由均使用真实路径，占位符只用于显示。

GUI 默认通过 Windows UAC 申请管理员权限。沿用上游原生启动器的 RequireAdministrator 设置和嵌入清单 requestedExecutionLevel=requireAdministrator、uiAccess=false；最终生效对象是 CodexProxy.exe。用户取消 UAC 时不进入 GUI，不增加自定义提权循环或普通权限回退。已经提升权限时按 Windows 规则启动。申请 GUI 管理员权限不等于自动启动代理服务。

## 构建、安装与现有保护

首版交付 Windows 10/11 x64；保留上游 .NET Framework 4.7.2 目标和 C# 7.3、VS 2022 v143 C++/CLI、NuGet、vcpkg、WiX 6.0.2 的构建路径。不为首版重写全部平台配置或引入新的 GUI 运行时。

Debug 用于开发目录启动，Release 通过独立 MSI/Setup 安装到受保护目录。先验证上游基线能构建，再修改产品代码。

产品改名同步更新项目输出、版本资源、PathUtilities、GUI/服务位置校验、原生启动器允许的文件名、CLR 配置名、单实例对象前缀、安装器及打包脚本。现有安全控制保持有效；仅当已被固定摘要覆盖的文件内容变化时更新摘要，单纯改名不要求额外生成校验和。

## 验证范围与未解决事项

一次聚焦合同检查：Codex 目标生成/拒绝规则、GUI 与后台使用同一策略、共享身份消费者和新配置保存行为。共享常量和输出格式变化后运行相关现有托管测试；不对未修改的网络协议核心另起大规模审计。

一次真实链路验收包含桌面与 CLI 两个受支持入口，各建立新连接，关联路由日志、后台到 SOCKS5 的连接以及应用响应；再用一个未匹配应用确认其没有进入本产品路由。按实际测试情况分别报告 TCP、UDP 和 DNS 覆盖，不能互相替代。

现有官方服务正在运行。独立服务身份允许两个产品安装并存，但尚未验证两个转发引擎同时控制同一驱动的可靠性。源码中每个引擎会设置网卡的捕获事件和工作模式，停止时会清空这些状态，服务名不同不能保证转发引擎可并行。因此首版验收采用一次只运行一个引擎；测试前记录旧服务状态，在明确授权后停止旧服务，测试结束按原状态恢复。先不为此增加永久冲突守卫或自动迁移。

UAC 使用另一管理员账户时，自动发现的用户目录可能不同；可选择实际 Codex 路径，不静默保存错误用户目录。WSL/Linux CLI、自动代理 Codex 启动的所有外部工具、全系统 DNS、代理节点管理和更新器不在首版范围。

## 本轮与后续授权边界

用户已于 2026-10-10 要求将计划详细写入 plan，并逐步执行、打勾、在独立步骤文件记录过程与结果，每完成一步提交推送。实施范围包括本仓库定制源码和本地构建产物；现有已安装官方服务、配置和网络设置不因此获得修改授权。

本次明确授权每个完成步骤 commit 并 push 到 origin/custorm，不授权 force push、merge、tag、release 或部署。安装开发工具、安装/启动系统服务、停止正在使用的旧服务和改动防火墙仍属于需专项授权的系统操作。

## 来源

- [ProxiFyre v2.6.2 固定源码](https://github.com/wiresock/proxifyre/tree/eef55ba96da9bcb21b4c39140e225f3f3a316755)
- [GUI 架构与构建](https://github.com/wiresock/proxifyre/blob/v2.6.2/docs/gui.md)
- [产品路径和服务常量](https://github.com/wiresock/proxifyre/blob/v2.6.2/ProxiFyre.Configuration/PathUtilities.cs)
- [进程匹配策略](https://github.com/wiresock/proxifyre/blob/v2.6.2/netlib/src/proxy/process_routing_policy.h)
- [网卡捕获状态控制](https://github.com/wiresock/proxifyre/blob/v2.6.2/netlib/src/ndisapi/queued_multi_interface_packet_filter.h)
- [Codex 官方 CLI 启动器](https://github.com/openai/codex/blob/main/codex-cli/bin/codex.js)，读取日 2026-10-10



