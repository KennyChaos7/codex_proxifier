# 步骤 18：新版 GUI 构建与实际截图核对

状态：执行中，未勾选。

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
