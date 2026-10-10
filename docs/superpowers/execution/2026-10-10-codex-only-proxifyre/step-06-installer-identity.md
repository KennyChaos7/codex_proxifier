# 步骤 06：独立安装与服务身份

状态：安装源码已完成；安装和对应测试留到最终阶段。
时间：2026-10-10；执行者：Sol。
前一步远端：4bc459a249e46539bd9f316e05dca464bcde7f78。

## 过程与结果

- Package/Bundle 产品名 Codex Proxy，安装目录 CodexProxy，Manufacturer=KennyChaos7。
- 服务 CodexProxyService / Codex Proxy Service；只绑定 CodexProxy.Service.exe。
- 生成并固定新的 MSI/Bundle UpgradeCode（后续升级复用）：
MSI UpgradeCode: B1D29949-BA0D-425B-B098-AA2D62E18326
Bundle UpgradeCode: 1FE96B81-50B4-4D3C-8DEF-84CFC226AF15
- 注册表 Software/CodexProxy；独立快捷方式与安装产物 CodexProxy-<version>-win-x64。
- 防火墙名称 Codex Proxy (TCP-In)/(UDP-In)，保留精确 File=当前引擎绑定，不开全局端口。
- 确定性 MSI component GUID 的已有计算命名空间改为 CodexProxy.Installer，避免显式目录/快捷方式GUID与上游产品共用。
- 原内部 WiX Id/工程名、setup 辅助 DLL 和共享 NDISRD 依赖保留；不移除驱动，不新增兼容迁移层。
- 许可指向本分支保留的 AGPL 源码 LICENSE，版权来源仍保留。

## 测试

未运行，按用户要求最后集中编译、安装契约及真实安装核验。
当前未安装新产品、未覆盖旧服务。

## Git

feat: 配置 Codex Proxy 独立安装与服务身份。
安装源码、必要消费者、记录和plan同次提交同步。
