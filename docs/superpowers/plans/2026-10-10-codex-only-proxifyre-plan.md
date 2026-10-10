# Codex Proxy Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans in this session. Steps use checkbox (`- [ ]`) syntax. 本计划的编号步骤是提交单位；步骤内的操作是该次提交的组成部分。

**Goal:** 在 custorm 分支将 ProxiFyre 定制为独立 Windows GUI 与服务，支持 Codex 桌面应用、专用后台及独立 Windows 原生 Codex CLI。

**Architecture:** 导入固定 ProxiFyre 2.6.2，保留 WinForms、SCM、配置保存和 socksify/NDISRD 转发核心。app-config.json 是唯一有效配置，GUI 与后台共用 CodexRoutingPolicy。内部工程名尽量保留，独立产品身份及安装身份。

**Tech Stack:** Windows 10/11 x64，C# 7.3，.NET Framework 4.7.2，VS 2022 v143/C++ CLI，NuGet，vcpkg，WiX 6.0.2，PowerShell 7。

**Spec:** [源码定制设计](../specs/2026-10-10-codex-only-proxifyre-design.md)

## 执行规则与授权

- 当前工作目录继续使用 custorm；用户已经指定此分支，不创建另一工作树、不改动 master。
- 用户于 2026-10-10 授权执行计划，并要求每个编号步骤完成后提交、推送。提交只包含该步文件，推送只到 origin/custorm；不 force push、不合并、不发布。
- 完成标准：该步骤交付物落盘、必要验证有真实结果、独立步骤记录写完。满足后将该步改为 [x]，与交付物一起提交，再推送；推送成功才进入下一步。
- 未完成步骤保持 [ ]。开始时创建该步记录，受阻时记录具体原因和恢复条件；不得把未运行测试写成通过。
- 每个记录保存目标、开始/结束时间、输入和文件、实际操作与命令、实际输出摘要、验证结果、决定或偏离、遗留阻碍、提交说明与推送目标。命令输出很长时只摘录决定结果的部分。
- 每步记录是长期事实来源，不另建重复的临时进度状态机。当前步骤的提交可用 git log -- 对应记录文件定位；不把提交自身的 SHA 写入它自身。上一提交的推送回执可写入下一步记录。
- 提交信息简要描述更新的模块和功能。已有 config/、downloads/、scripts/、verification/ 和旧设计稿不自动加入提交、不删除。
- 用户最新授权：按计划自主执行必要工具配置与实施，不再重复征求同意；实际 OS 管理员能力、登录凭据和系统约束不能由文字授权替代。不扩大到合并、发布或无关系统变更。
- 每步只运行能消除该步具体不确定性的验证。源码未改动前，导入只核对来源和核心文件；行为变更用可运行的测试和必要真实链路检查。
- Sol 保留耦合实施与验收。独立任务需要委派时才另写路由回执，不预先声称已使用 Luna。

## 不可变事实与最小验收

- 上游固定 commit：eef55ba96da9bcb21b4c39140e225f3f3a316755（v2.6.2）；保留 AGPL-3.0、版权及修改说明。
- 产品 0.1.0；GUI CodexProxy.exe；托管 GUI CodexProxy.Managed.dll；后台 CodexProxy.Service.exe；Windows 服务 CodexProxyService。
- 安装目录 Program Files/CodexProxy；业务配置为该目录 app-config.json；GUI 偏好为 LocalAppData/CodexProxy/ui-settings.json；共享 DLL 及系统驱动不另造一套。
- SOCKS5 默认 127.0.0.1:7890，LAN 直连开启，日志 Info；配置只有一个代理规则。
- 只允许 Codex 专用目录/包前缀/原生 CLI 绝对路径；不允许通用 ChatGPT.exe、裸 codex.exe、通用 node/python/git、空规则、catch-all、宽根目录。
- 真实绝对路径用于保存和后台匹配；界面用 %LOCALAPPDATA% / %USERPROFILE% 隐藏本电脑实际用户名。更具体的 LocalAppData 前缀优先。
- CLI 路径为空时选择按钮先显示现存候选；顺序为运行进程、PATH、专用安装目录、npm 原生布局；按完整路径去重并合并来源；确认才回填、取消不改变。
- 原生 GUI 启动器默认 RequireAdministrator，清单 requireAdministrator、uiAccess=false；UAC 取消不进入 GUI，不自动启动代理服务。
- 桌面及独立 CLI 各完成一次真实请求，并有进程匹配、到 SOCKS5 的连接和应用响应证据；一个未匹配应用不进入产品路由。
- GUI 服务状态与 SCM 一致；保存并应用、停止、托盘恢复和退出语义可用。关闭到托盘、退出 GUI 不停止服务。
- 原官方 ProxiFyreService 不被覆盖或删除；两引擎同用驱动尚未经证实可靠，真实验收一次只运行一个引擎。
- WSL/Linux CLI、自动代理 Codex 调用的外部工具、全局 DNS 不属于首版验收。

## 当前进度

执行状态：00—01 已同步 GitHub；02 工具盘点完成。本机缺少 VS/SDK 且非管理员，03 改用受支持的 GitHub Windows 环境。功能测试统一在实现结束后执行，实施勾选不代表测试通过。

| 编号 | 步骤 | 过程和结果记录 |
| --- | --- | --- |
| 00 | 计划与逐步归档 | [step-00-plan.md](../execution/2026-10-10-codex-only-proxifyre/step-00-plan.md) |
| 01 | 固定上游源码导入 | step-01-source.md |
| 02 | 构建工具实测盘点 | step-02-toolchain.md |
| 03 | 配齐构建依赖 | step-03-dependencies.md |
| 04 | 未修改上游的 Debug 基线 | step-04-baseline.md |
| 05 | 程序身份与默认管理员启动 | step-05-identity-uac.md |
| 06 | 独立安装与服务身份 | step-06-installer-identity.md |
| 07 | Codex 路由策略 | step-07-routing-policy.md |
| 08 | Codex 桌面及 CLI 路径发现 | step-08-discovery.md |
| 09 | 隐私路径显示与候选选择 | step-09-path-picker.md |
| 10 | 后台执行同一范围约束 | step-10-service-policy.md |
| 11 | Codex 专用主界面与应用配置 | step-11-gui.md |
| 12 | 托盘、日志及服务操作 | step-12-tray-service.md |
| 13 | Debug GUI 集成核对 | step-13-debug-integration.md |
| 14 | Release 与独立安装包 | step-14-release.md |
| 15 | 受控安装和真实请求验收 | step-15-smoke.md |
| 16 | 最终文档及整支审查 | step-16-review.md |

以上记录均位于 docs/superpowers/execution/2026-10-10-codex-only-proxifyre/，对应步骤开始时创建。勾选状态以各编号步骤内的完成复选框为准。

## 用户指定的测试执行顺序

2026-10-10：实施阶段写代码及测试用例，但不逐步运行对应测试。所有 RED/GREEN 与逐步测试运行条目改为在 13—16 的最终验证阶段执行。必要依赖定位、源码来源核对和编译属于执行过程；结果必须如实记录，不能声明测试已通过。

## Review Focus

| 可达失败 | 所属步骤及证据 |
| --- | --- |
| 桌面内部程序也叫 ChatGPT.exe，被通用名称误匹配 | 07/08 的专用包路径断言，15 的真实桌面请求 |
| npm codex 是脚本，误代理所有 node.exe | 08 找原生 codex.exe 的断言，15 的独立 CLI 请求 |
| LocalSystem 重新解析到错误用户目录 | 10 使用已保存绝对路径，15 后台实际匹配 |
| 改名后清单/完整性检查拒绝新 GUI 或服务 | 05/14 构建与产物核对，15 最终安装启动 |
| 用户名在候选、已选路径、提示中重新出现 | 09 显示/保存断言，13 窗体检查 |
| 取消 UAC 后仍进入 GUI | 05 最终清单检查，15 普通权限启动接受/取消 |

## 编号步骤

### 00：详细计划与逐步记录

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** 本 plan、对应 spec、step-00-plan.md。
**接口/不变量：** 后续实现接口不变；记录与代码同次归档，不混入无关未跟踪文件。

操作：
1. 核对 custorm、远端 origin、未跟踪文件和既有五大任务。
2. 将任务分解为本计划的编号交付步骤，逐步列出文件、操作、结果和验证方式。
3. 在独立步骤记录里记下当前事实、用户授权、归档约定和验证结果。
4. 聚焦核对：桌面/CLI、候选提示、隐私路径、UAC 均有实施步骤；未执行步骤不打勾。
5. 定向 git add 本步骤三个文件；commit；首次 git push -u origin custorm。

**Expected:** 计划/设计/记录已写入，只有 00 完成；远端 custorm 跟踪建立。
**Commit:** docs: 细化 Codex Proxy 执行计划与逐步记录

### 01：导入固定上游源码

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** 新建 src/proxifyre/ 上游源码及 UPSTREAM.md；根 .gitignore 仅增加该源码实际会产生的构建目录规则。
**接口/不变量：** 不导入嵌套 .git；不激活根目录 GitHub 发布 workflow；不运行旧 Apply-CodexConfig.ps1。

操作：
1. 从 GitHub 获取固定 commit 的源码归档，保存到 downloads/ 的独立文件名。
2. 在工作区内解压，导入 src/proxifyre/；核对 LICENSE、socksify.sln、GUI、服务、配置和 netlib 实际存在。源代码中的 .github 内容仅作为上游材料保留在 src 内。
3. 写 UPSTREAM.md：仓库、tag、commit、导入日期、AGPL、原始/后续修改边界。
4. 添加必要构建输出 ignore；不改产品代码。
5. 记录导入文件数、来源、核心文件检查和实际命令；勾选 01；定向 add；commit；push origin custorm。

**Verify:** 固定来源下载成功，核心项目文件存在，git status 不包含下载归档/构建产物；git diff --cached --stat 用于确定本次只导入源码和记录。
**Expected:** src/proxifyre/ 可供后续构建，但不宣称可编译。
**Commit:** chore: 导入 ProxiFyre 2.6.2 固定源码

### 02：实测构建工具和依赖缺口

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** step-02-toolchain.md；不修改源码。
**Consumes:** 01 的实际工程/构建脚本。

操作：
1. 按上游工程确认 v143/C++ CLI、.NET 4.7.2、NuGet、vcpkg、WiX 6.0.2、PS7 和 .NET SDK 要求。
2. 用 PATH、vswhere 和标准安装目录定位；记录命令、实际版本/存在结果和缺失项，不用 Framework 老 MSBuild 冒充 VS MSBuild。
3. 对缺项提出具体安装范围；需要系统安装时停在 03，等待专项授权；已具备时直接继续。
4. 已完成的是盘点，不是构建；勾选 02；提交记录和 plan；push。

**Verify:** 每项结论由实际路径/命令或上游项目要求支撑。
**Expected:** 明确当前环境能否进入基线构建，以及缺什么。
**Commit:** docs: 记录 Codex Proxy 构建工具缺口

### 03：配置 Windows 构建环境与依赖入口

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** scripts/Build-CodexProxy.ps1 的依赖准备部分；step-03-dependencies.md。系统安装需授权，不将工具本体入库。

操作：
1. 使用 GitHub windows-2022 runner 的 VS 2022/v143/C++ CLI 和 .NET targeting pack；添加根 .github/workflows/build-codexproxy.yml，仅构建、不发布。工作流仅在其文件更新时触发，避免每次代码提交重复编译。
2. 项目本地准备 NuGet/vcpkg/.NET SDK 下的 WiX；遵守上游锁定依赖，不擅自升级。
3. 写脚本入口参数 Configuration=Debug/Release、固定 x64；按仓库位置解析源码，不硬编码本电脑用户名和 E 盘目录。
4. 明确定位 VS MSBuild、NuGet、vcpkg、PS7 与 dotnet；缺工具给出具体错误，不生成“成功”产物。
5. 一次实际定位命令确认依赖可用，写记录、勾选、提交、推送。

**Expected:** 04 的恢复与构建命令可以启动；没有安装授权或工具仍缺时不勾选。
**Commit:** build: 配置 Codex Proxy 构建依赖入口

### 04：建立未修改上游 Debug 基线

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** scripts/Build-CodexProxy.ps1；step-04-baseline.md。
**Consumes:** 未定制 src/proxifyre/ 和 03 的工具。

操作：
1. 在 VS Developer Shell 恢复依赖，命令在 src/proxifyre 执行：

```powershell
nuget restore ProxiFyre.Configuration\packages.config -PackagesDirectory packages -NonInteractive
nuget restore ProxiFyre\packages.config -PackagesDirectory packages -NonInteractive
nuget restore ProxiFyre.Tests\packages.config -PackagesDirectory packages -NonInteractive
nuget restore netlib-tests\packages.config -PackagesDirectory packages -NonInteractive
msbuild ProxiFyreSetupBootstrapper\ProxiFyreSetupBootstrapper.vcxproj -t:Restore -v:minimal -p:RestoreLockedMode=true -p:RestoreConfigFile=NuGet.Installer.Config
msbuild ProxiFyreSetupEngineExtension\ProxiFyreSetupEngineExtension.vcxproj -t:Restore -v:minimal -p:RestoreLockedMode=true -p:RestoreConfigFile=NuGet.Installer.Config
msbuild socksify.sln -t:Build -v:minimal -p:Configuration=Debug -p:Platform=x64 -p:Version=2.6.2
```

2. 将这些实际有效的命令组织到统一构建脚本；不为了掩盖工具错误改业务源码。
3. 定位上游 NUnit/VSTest 执行入口，跑原始托管测试；记录命令、退出码、用例数和失败名称。原生构建按实际依赖执行。
4. 核对 bin/exe/x64/Debug 的 GUI、服务和 socksify；不安装/启动官方或新服务。
5. 成功才勾选，记录并提交推送；失败先记录和定位，不进入产品改造。

**Expected:** 基线构建退出 0，所运行原始测试通过。
**Commit:** build: 建立 ProxiFyre Debug 构建基线

### 05：程序身份与默认管理员启动

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** Directory.Build.props；ProxiFyre/ProxiFyre.csproj、Properties/AssemblyInfo.cs、Program.cs；ProxiFyreUI/ProxiFyreUI.csproj、Properties/AssemblyInfo.cs、app.manifest、Program.cs；ProxiFyreUILauncher/Program.cpp、Resources.rc、ProxiFyreUILauncher.vcxproj；ProxiFyre.Configuration/PathUtilities.cs；GUI Infrastructure/EngineLocator.cs、PayloadIntegrity.cs、SingleInstanceCoordinator.cs、UiSettings.cs；实际受影响测试。
**Produces:** CodexProxy.exe、CodexProxy.Managed.dll、CodexProxy.Service.exe；默认 UAC 启动。

操作：
1. 先修改对应产品路径/定位断言，运行观察因旧产品身份失败。
2. 设置 AssemblyName/TargetName、版本 0.1.0、ProductName/OriginalFilename、CLR .config 名称、互斥体前缀和 GUI 偏好目录；内部命名空间及共享 DLL 保持原状。
3. 同步 GUI/服务位置与完整性校验预期；只更新受到改名或真实文件内容变更影响的上游摘要。
4. 保留原生启动器 UACExecutionLevel=RequireAdministrator 与 app.manifest 的 requireAdministrator、uiAccess=false；要求最终 native EXE 嵌入清单生效。
5. 构建 Debug，跑受影响托管测试，核对产物 FileVersionInfo 和嵌入清单。真实 UAC 接受/取消留到 15。
6. 记录、勾选、提交、推送。

**Expected:** 改名产物可加载、身份一致；不再默认定位旧服务。
**Commit:** feat: 独立 Codex Proxy 程序身份与管理员启动

### 06：独立安装与服务身份

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** ProxiFyre.Configuration/ServiceInstallLocationPolicy.cs、PathUtilities.cs；ProxiFyre.Installer/Package.wxs；ProxiFyre.Bundle/Bundle.wxs；对应 wixproj；scripts/Build-Installer.ps1；受影响安装测试。

操作：
1. 用上游安装契约测试约束新服务名/后台路径、文件清单和身份，不安装系统。
2. 将 service 固定为 CodexProxyService，安装目录 CodexProxy，短名称/注册表/快捷方式独立。
3. MSI/Bundle 各生成一次新的 UpgradeCode，写入对应项目，后续本产品升级沿用。
4. 防火墙项 Codex Proxy (TCP-In)/(UDP-In) 精确绑定新后台文件；共享 NDISRD 驱动不复制品牌、不随本产品卸载。
5. 调整上游安装清单与构建预期，运行受影响契约测试。
6. 写记录、勾选、提交、推送。

**Expected:** 与官方 ProxiFyre 安装身份不同；不覆盖旧服务。最终安装在 15 验证。
**Commit:** feat: 配置 Codex Proxy 独立安装与服务身份

### 07：共享 Codex 路由策略

- [x] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** 新增 ProxiFyre.Configuration/CodexRoutingPolicy.cs；ProxiFyre.Tests/CodexRoutingPolicyTests.cs；对应 csproj。
**Produces:**
- ValidateTargets(IEnumerable<string> targets) -> ValidationResult
- CreateConfiguration(IEnumerable<string> targets, ProxyRule proxyOptions, bool bypassLan, string logLevel) -> ProxiFyreConfiguration
- ValidateConfiguration(ProxiFyreConfiguration configuration) -> ValidationResult

操作：
1. 测试允许 WindowsApps/OpenAI.Codex_ 包前缀、专用 Codex 目录和原生 codex.exe 绝对路径；拒绝空项、通用名称、宽根目录、第二个代理规则和 catch-all。
2. CreateConfiguration 断言只保留代理选项，appNames 只能来自 targets；默认值固定为 127.0.0.1:7890、LAN=true、Info。
3. 运行新增测试观察旧行为/缺少功能失败，再实现策略；复用 ValidationResult/ValidationIssue。
4. 专用目录保留尾分隔符，CLI 转为规范绝对路径并去重；不改 native 的既有匹配算法。
5. 跑策略测试及受影响原配置消费者测试；记录、勾选、提交、推送。

**Expected:** GUI 和后台能调用同一范围契约；只定义支持路径，不承诺识别恶意伪装程序。
**Commit:** feat: 添加 Codex 专用路由策略

### 08：桌面与原生 CLI 路径发现

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** 新增 ProxiFyreUI/Infrastructure/CodexTargetDiscovery.cs；ProxiFyre.Tests/CodexTargetDiscoveryTests.cs；对应 csproj。
**Produces:** Discover() -> IReadOnlyList<string>；FindCliCandidates() -> IReadOnlyList<CodexPathCandidate>；同文件定义 ExecutablePath、DisplayPath、SourceLabel。

操作：
1. 固定测试真实桌面包路径、专用运行目录、PATH 直接 EXE，以及 npm cmd/ps1/js 指向的 vendor/native codex.exe；不把 node.exe 或脚本本身作为路由目标。
2. 测试运行进程、PATH、专用目录、npm 的候选顺序，只返回存在的 EXE，重复完整路径合并来源。
3. 先运行失败测试；实现读取包安装信息、当前进程和已支持布局，不执行发现到的脚本、不扫描全盘。
4. Discover 与候选选择共用同一发现逻辑；GUI 显式刷新，不缓存或后台监控。
5. 在本机只读发现一次，与实际存在路径核对；自动发现不足时返回未识别并允许选原生 EXE。
6. 记录、勾选、提交、推送。

**Expected:** 桌面更新目录用包前缀保持有效；独立 CLI 更新位置可刷新。
**Commit:** feat: 发现 Codex 桌面与原生 CLI 路径

### 09：隐藏用户信息的候选选择

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** CodexTargetDiscovery.cs；新增 Forms/CodexPathPickerForm.cs、CodexPathPickerForm.Designer.cs；相应测试与 csproj。
**Consumes:** 08 的 ExecutablePath/SourceLabel。
**Produces:** DisplayPath 仅供显示；确认返回真实 ExecutablePath，取消不改变当前配置。

操作：
1. 写显示/存储断言：LocalAppData 优先变为 %LOCALAPPDATA%，其余用户目录变为 %USERPROFILE%，界面路径不含本电脑用户名，保存使用原始绝对路径。
2. 写候选交互测试：确认只回填未保存值、取消不改、零候选仍能浏览、同一路径只出现一次。
3. 运行失败测试，然后实现最小显示格式函数和 WinForms 选择窗体。
4. 提供“浏览其他位置…”“取消”“使用此路径”；路径为空点击选择先展示候选；来源“正在运行”不宣称必然是 CLI 入口。
5. 候选、已选路径、相关错误提示使用同一显示格式，不用悬浮提示暴露用户名；不将占位符传到后台配置。
6. 跑上述测试并检查窗体显示；记录、勾选、提交、推送。

**Expected:** 提示保护用户信息，真实路由路径不变。
**Commit:** feat: 添加隐私路径提示与 Codex 候选选择

### 10：后台使用同一范围约束

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** ProxiFyre/Program.cs、app-config.sample.json；后台加载/策略测试。
**Consumes:** 07 的 ValidateConfiguration。

操作：
1. 测试合法用户绝对路径在模拟 LocalSystem 下不变；外部 JSON 的空规则、notepad.exe、通用 ChatGPT.exe 和第二条代理规则在初始化 native 前被拒绝。
2. 运行失败测试，接入 LoadConfiguration：格式验证、归一化后执行共享范围校验，成功才创建 Socksifier/进程关联。
3. 失败记录具体配置原因，不静默丢弃规则、不扩大范围；同步新样例。
4. 跑后台加载与配置消费者测试；写记录、勾选、提交、推送。

**Expected:** 外部配置不能绕过 GUI 的 Codex 范围；后台不从 LocalSystem 的环境重新发现用户目标。
**Commit:** feat: 限制后台服务仅路由 Codex 目标

### 11：Codex 专用主界面与配置应用

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** Forms/MainForm.cs、MainForm.Designer.cs；ConfigurationWorkspace/ConfigurationApplyCoordinator 的必要接入；相应测试及 csproj。
**Consumes:** 07 策略、08 发现、09 选择窗体；保留 IProxiFyreServiceController、LogTailer、SingleInstanceCoordinator。

操作：
1. 写配置协调测试：修改 SOCKS5 保留 Codex targets；CLI 刷新改变真实路径；没有目标/无效地址不显示“已生效”。
2. 运行失败测试，再整理主界面为状态、SOCKS5、Codex 目标、折叠日志及启动/停止/保存并应用/刷新路径。
3. 连接候选选择与真实配置，所有生成都走 CreateConfiguration；app-config.json 为唯一有效配置。
4. 移除通用规则编辑、通用进程列表、多代理和 catch-all 入口；仅在没有引用后移出旧窗体编译项。
5. 保留可选认证、LAN、日志级别；密码不明文进入诊断。
6. 跑配置协调/窗体相关测试；记录、勾选、提交、推送。

**Expected:** 界面只有 Codex 工作流，配置可保存到本产品位置。
**Commit:** feat: 精简 Codex 专用代理主界面

### 12：托盘、日志和真实服务操作

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** MainForm、托盘/服务协调的必要接入；相关测试。不另建服务状态机。

操作：
1. 断言启动/停止/应用后以 SCM 实际状态更新 GUI，操作失败给出具体错误；重复打开恢复同一窗口。
2. 保留关闭到托盘、退出 GUI 不停止服务；托盘提供打开、启动、停止、退出，调用同一服务控制接口。
3. 日志可展开并跟随现有 LogTailer；诊断只显示完成任务所需信息，路径提示隐藏用户名，认证信息不明文输出。
4. 跑受影响协调测试；不为了测试自行安装/停止系统服务。
5. 记录、勾选、提交、推送。

**Expected:** GUI/托盘语义一致，真实系统动作留到有授权的 15。
**Commit:** feat: 完成 Codex Proxy 托盘与服务操作

### 13：Debug GUI 集成核对

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** 必要局部修复、step-13-debug-integration.md；不引入无关功能。

操作：
1. 构建 Debug x64 并运行受影响托管测试集合。
2. 打开最终 GUI，核对识别、候选、手动选择、隐藏用户名、保存、日志展开和托盘恢复。
3. 核对配置文件仍含准确绝对目标路径，而界面使用显示形式；不更改已安装官方 app-config.json。
4. 如发现具体可复现问题，记录并修复对应局部，再重跑受影响检查；没有新改动不重复扩张验证。
5. 记录实际启动环境及界面结果，勾选、提交、推送。

**Expected:** 新 GUI 自身工作流完整；未授权安装时不宣称真实转发通过。
**Commit:** test: 核对 Codex Proxy Debug GUI 集成

### 14：Release 和独立安装包

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** Build-CodexProxy.ps1、Build-Installer.ps1、受影响安装脚本/项目；README 构建说明。
**Produces:** bin/exe/x64/Release/CodexProxy.exe、CodexProxy.Service.exe、必要 DLL；bin/installer/x64/Release 的独立 MSI/Setup。

操作：
1. 用固定依赖构建 Release x64、WiX Bundle/MSI，不自动上传产物。
2. 跑受改名影响的安装契约/清单测试；核对产品身份、嵌入 UAC、文件资源和受保护安装目录预期。
3. 记录实际安装包文件名/大小、构建命令与退出码；README 写平台、AGPL 源码来源和构建步骤。
4. 产物不默认提交到 Git；只提交脚本、说明和记录。
5. 勾选、提交、推送。

**Expected:** 可供受控安装的独立安装包；能打包不代表能转发。
**Commit:** build: 生成 Codex Proxy 独立 Release 安装包

### 15：受控安装和真实请求验收

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** verification/codex-proxy-smoke.md；step-15-smoke.md；必要局部修复。
**授权前提：** 明确同意安装新产品/驱动或复用已有驱动、切换服务和必要防火墙操作；授权未到保持未完成。

操作：
1. 记录官方 ProxiFyreService、系统代理/TUN 状态和 SOCKS5 端点；按授权暂停旧引擎，避免两个引擎同时争用驱动。
2. 从独立 Setup 安装到受保护目录；核对新服务、安装路径、快捷方式及不覆盖官方产品。
3. 从普通权限会话启动 GUI，接受 UAC 后进入管理员进程；另一次取消 UAC 后无 GUI。已有管理员进程不要求重复提示。
4. 在 GUI 发现/选择 Codex，保存并启动新服务；进行一次桌面请求，保存匹配、SOCKS5 连接和应用响应证据。
5. 独立运行 Windows 原生 CLI 完成一次请求；npm 场景跟踪到真实 codex.exe，不修改登录凭证。
6. 一个未匹配应用建立连接，核对不进入本产品路由；TUN/系统代理影响存在时只在已授权的隔离环境判定直连。
7. 核对停止、保存并应用、托盘恢复、退出不停止；必要卸载测试只卸载自身并保留共享驱动。
8. 按授权恢复旧服务原状态；如授权保留新产品则记录最终状态。仅记录实测 TCP/UDP/DNS 覆盖，不扩展承诺。
9. 完成所有必要证据才勾选，提交记录及实际修复，push。

**Expected:** 桌面与 CLI 真正转发、未匹配应用不被本产品接管，旧产品保留完整。
**Commit:** test: 完成 Codex 桌面与 CLI 代理验收

### 16：最终使用文档与整支审查

- [ ] **完成本步骤的交付物与过程记录（对应测试在最后执行）。**

**Files:** README.md；源码许可/修改说明；step-16-review.md；必要局部修复。

操作：
1. 文档覆盖安装、默认 UAC、SOCKS5、候选/手动 CLI 选择、显示隐藏与实际保存、托盘、停止/退出、版本更新后刷新、支持边界。
2. 根据各步骤真实记录描述已验证功能和剩余限制；不把工程过程写进产品 UI。
3. 对整个定制 diff 做一次独立上下文审查；按技能路由实际可用 reviewer，明确仅审查、不擅自推送。
4. 只修实际可达的重要问题，重新验证受影响链路；审查最终结论 ship / fix-first / rethink，不能用通过构建替代真实请求结果。
5. 核对所有完成勾选都有记录与真实证据，最终 git status/远端分支同步。
6. 记录结论、勾选、提交、推送；不 merge、tag、release。

**Expected:** 使用文档与已验证行为一致，整个独立产品可交付；有未完成关键步骤时不宣称最终完成。
**Commit:** docs: 完成 Codex Proxy 使用文档与验收记录









