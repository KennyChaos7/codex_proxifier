# 步骤 14：Release 与安装包（执行中）

- Windows CI 现构建 Debug、Release 及独立 MSI/setup，随后执行最终测试。
- 安装器验证合同同步新服务名、UI 文件名、独立 UpgradeCode 与产品名；保留文件表、驱动和依赖检查。
- 托管套件完整执行；原生执行此次变更涉及的 OwnerRoutingPolicyTest 路由套件。
- 未安装、停止状态及 UAC 运行链路由步骤 15 记录，不以构建成功代替网络运行验收。

当前等待远端真实构建结果，主计划本步骤暂不勾选。
## 最终构建修复 1

- Run 38034932570：Debug 编译失败 CS0012，MainForm 清除 Configuration.ExtensionData 时需要 JToken 的程序集引用。
- 根因：.NET Framework 非 SDK 工程没有将共享模型的 Newtonsoft.Json 编译引用传递给 UI。
- 修复：UI 显式引用已经由配置项目还原的同版本 Newtonsoft.Json，不增加新的包或运行依赖。
- 同批修正候选窗口排序后按 Row.Tag 返回真实路径，增加排序回归；凭据遮盖同时覆盖正在编辑和已保存值。
- 修复后触发相同 Windows 构建/测试链路；没有将失败结果标记通过。
## 最终源码复核修复 2

- 手选 CLI 不在 PATH/npm 自动候选时，重新打开 GUI 的首次刷新可能丢弃已保存路径。
- 现以 app-config.json 中仍存在的原生 codex.exe 路径与自动发现结果合并；不增加第二套持久化状态。
- GUI 回读使用最后一个已保存 CLI 文件作为选择提示。
## 最终构建修复 3

- Run 38035171258：Debug、Release、MSI/setup 及安装包合同检查成功。
- 托管测试 322 项，321 通过、1 失败：200% DPI 下 bypassLanCheckBox 被 settingsPanel 裁切；原生测试因前序失败未执行。
- 根因：顶部设置卡片使用剩余空间的固定 60%，小于按字体与控件所需的内容高度。
- 修复：设置卡片按内容 AutoSize 分配高度，日志使用剩余空间；测试合同不放宽。
- 保留失败记录，并以新版本运行相同链路。
- 位图复核发现隐身窗体的 DrawToBitmap 只输出背景。截图测试改为显示测试子类的真实窗口；仅在测试中跳过 OnShown 服务连接，不给产品增加 preview 开关。
- 最终独立安装器提示同步 Codex Proxy 名称；本地产物目录 artifacts/ 加入忽略规则，不把二进制和测试临时结果提交进源码。
- 最终界面合同复核补充认证开关语义：启用认证但两个凭据都未填写时阻止保存。通用模型将两个空值视为未认证，所以这个 UI 意图必须在读取控件时处理；部分填写仍由原有通用校验负责。增加对应 UI 回归，不增加配置状态或第二层通用校验。
## 最终安装包文案合同修复

- Run 38035800408 在 Bundle 包合同失败：架构条件正确，但测试仍精确期待旧的 This ProxiFyre 提示。
- 同时核对最低 Windows 条件的旧品牌提示，生产者与消费者统一改为 Codex Proxy；保留架构及最低系统条件的精确断言。
- 真实 GUI 位图已下载并目视确认：两栏设置、Codex 路径、服务操作、日志与托盘偏好均可见，没有裁切默认布局。


## 最终结果（2026-10-10）

- 最终源提交：2fedc550c9b62252d435e0509ad9bd0973abd96c。
- CI Run 38036182436 全部成功：https://github.com/KennyChaos7/codex_proxifier/actions/runs/38036182436。
- Debug 与 Release x64 编译通过；MSI、Burn、未签名及依赖合同通过。
- 托管测试 323/323 通过；原生 OwnerRoutingPolicyTest 11/11 通过；UAC 清单检查通过。
- 已下载独立程序到 artifacts/CodexProxy-Release-x64/，安装包到 artifacts/CodexProxy-0.1.0-x64/，测试结果到 artifacts/test-results-final/。
- setup：CodexProxy-0.1.0-win-x64-setup.exe；MSI：CodexProxy-0.1.0-win-x64.msi。
- 最终 GUI 渲染截图位于 artifacts/CodexProxy-GUI.png。截图来自测试窗口，没有连接真实代理服务。
- 本机编译产物非管理员启动返回 740；发现 5 个 Codex 路径且共享范围校验通过（仅只读 smoke）。
- 自动权限审核曾超时一次，文件写入与执行拆开后成功；没有遗留被拒绝的验证动作。
- 本步骤完成。管理员安装及真实网络请求仍在步骤 15 保持未完成。

