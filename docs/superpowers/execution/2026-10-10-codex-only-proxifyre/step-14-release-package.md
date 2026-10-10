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
