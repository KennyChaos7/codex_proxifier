# 步骤 18：新版 GUI 构建与实际截图核对

状态：已完成并归档，计划步骤 18 已勾选。

首轮：源码 a50ad9ea20ab99ec94a854510b24a560a7ec2cd4。
CI：https://github.com/KennyChaos7/codex_proxifier/actions/runs/38040947894
结果：Debug、Release、MSI/Burn 均成功，326/326 托管测试、11/11 原生路由测试通过。

实际截图暴露的问题：
- 主窗卡片的默认最小布局高度使日志区域偏矮，空日志 TextBox 遮盖空状态文字。
- 路径弹窗候选区高度不足，第二张卡片下缘裁切。
- MainForm.Layout 辅助方法隐藏 WinForms 自带 Layout 事件，产生编译警告。

局部修正：
- 自动布局面板明确 GrowAndShrink，压缩桌面路径列表的初始高度；空日志时隐藏 TextBox，有内容时恢复。
- 放大候选弹窗文字与窗口，调整行高以完整容纳路径两行文本。
- 保留选择状态描边，移除整行虚线；SOCKS5 标签改为胶囊。
- 将布局辅助方法改名 GridLayout。
- 截图预览使用明确的内存配置和停止状态夹具；仍绘制真实 WinForms 控件，不读取或修改主机代理服务。

下一步：提交推送对应修正，重新执行受影响的 Windows 构建和测试，核对最终截图。

第二轮：源码 ce1e639f3aa4f1789ffe69914c27a93f04c43611。
CI：https://github.com/KennyChaos7/codex_proxifier/actions/runs/38043077505
结果：构建全部成功，326/326 托管测试、11/11 原生路由测试通过，上一轮 Layout 隐藏警告消失。
截图：主窗卡片、日志空状态和第二候选卡片裁切已修正。
新增发现：GDI 的 WordBreak 不会按长 Windows 路径中的反斜杠换行，常见带版本目录的路径仍被省略号截断。
局部修正：仅在绘制时按可用宽度和路径分隔符换行，按行数调整候选卡片高度；显示字符串仍已脱敏，底层真实路径绑定不变。窗口缩放/字体变化时重算行高，主窗路径标签禁用助记符解析以正常显示路径中的 &。
推送后的源码复核发现路径分隔符字符字面量被脚本转义为单个反斜杠；改用 Path.DirectorySeparatorChar，避免编译错误。此修正仅涉及绘制换行，未改变路径数据。

最终验收：源码 4bfe59def8c5c232f5cd01fe9cbd8471be75507f。
CI：https://github.com/KennyChaos7/codex_proxifier/actions/runs/38043712168
实际结果：
- Debug 应用、Release 应用、MSI 与 Burn 安装包全部构建成功。
- 326/326 托管测试通过：包含默认/认证展开的 200% DPI 控件布局、日志收起/恢复、候选排序后的真实路径选择和实际截图输出。
- 11/11 原生 OwnerRoutingPolicyTest 通过，原有 GUI requireAdministrator manifest 合同检查通过。
- 本轮 C# 编译没有新增警告或错误。
- Sol 目视核对真实 WinForms 主窗与路径弹窗：浅色双栏圆角卡片、SOCKS5 胶囊、灰色服务状态胶囊、深色启动/确认按钮、独立桌面和 CLI 分区、操作条、日志空状态均符合既定示意图。候选卡片完整显示，首项长路径换行，不含本机用户名。
- 窗口标题栏由 Windows 系统绘制，CI 的 Windows Server 标题栏外观与示意图中的桌面系统不同；客户区采用上述统一样式。
- 截图采用固定示例配置/停止状态，在真实 WinForms 控件上绘制；不作为管理员安装或真实代理请求的证据。

本地产物（仓库根目录）：
- artifacts/gui-verified/CodexProxy-GUI.png
- artifacts/gui-verified/CodexProxy-PathPicker.png
- artifacts/gui-verified/CodexProxy-Release-x64/CodexProxy.exe
- artifacts/gui-verified/CodexProxy-0.1.0-x64/CodexProxy-0.1.0-win-x64-setup.exe
- artifacts/gui-verified/CodexProxy-0.1.0-x64/CodexProxy-0.1.0-win-x64.msi

结论：ship（本次 GUI 修改范围）。由 Sol 实现、核对与验收，Luna 未使用。未改变代理规则、服务身份、官方 ProxiFyre 服务或默认管理员启动行为。
步骤 15 的管理员安装与桌面/独立 CLI 真实网络请求验收仍未完成，继续保持未勾选。本步骤结束后仅提交记录和计划，不合并、不打 tag、不创建 release。
