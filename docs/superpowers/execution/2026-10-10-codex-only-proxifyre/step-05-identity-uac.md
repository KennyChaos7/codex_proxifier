# 步骤 05：程序身份与默认管理员启动

状态：身份改造已写入；构建/测试统一在功能结束后核验。
时间：2026-10-10；执行者：Sol。
前一步远端：98bf547e5f94bf6de51800cc362e4a083fdf4048。

## 实际改动

- 版本源改为0.1.0，仓库元数据指向本仓库；内部项目、命名空间和 ProxiFyreUI.ManagedEntryPoint 入口保留。
- native TargetName=CodexProxy；managed AssemblyName=CodexProxy.Managed；service AssemblyName=CodexProxy.Service。
- 服务常量 CodexProxyService，GUI偏好目录 CodexProxy，互斥体使用独立前缀，Startup日志独立。
- 同步 native 白名单、managed payload、engine定位和安装源码中的产物文件名及服务版本资源 ProductName=Codex Proxy。
- 将 GUI CLR 配置源文件改名 CodexProxy.exe.config，保持内容不变，同时更新 csproj、ignore 例外和 LF 属性。已有固定配置摘要无需变化；保留上游检查。
- 保留 RequireAdministrator linker设置及 requireAdministrator/uiAccess=false 清单。清单 identity 改名，默认提权不被改成自制启动循环。
- 同步源码内依赖这些名称的原有测试/构建说明，暂不运行它们。

## 过程与结果

最初 Python 3.8 不支持 write_text(newline=...)，在首次写入前失败；改用 write_bytes 后完成，不丢弃已存在文件。
实际 grep 显示 AssemblyName、TargetName、service常量和 UAC 规则已落盘。
没有在当前阶段执行编译或 UAC smoke，不宣称新程序已通过运行验证。
后续06独立安装身份，07—12实现Codex策略及UI，然后集中编译与测试。

## Git

feat: 独立 Codex Proxy 程序身份与管理员启动。
代码/消费者/步骤记录/plan 同次提交，使用 GitHub 接口同步到 custorm。
