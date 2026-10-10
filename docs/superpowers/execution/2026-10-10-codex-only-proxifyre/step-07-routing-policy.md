# 步骤 07：共享 Codex 路由策略

状态：实现和测试用例已写入；测试未运行。
时间：2026-10-10；执行者：Sol。
前一步远端：9c7b6709ec361e814ee7f34136706c44a84ae814。

## 实际改动

新增 CodexRoutingPolicy.ValidateTargets、CreateConfiguration、ValidateConfiguration，复用 ValidationResult/ValidationIssue。
只允许 WindowsApps/OpenAI.Codex_ 包范围、绝对 OpenAI/Codex 专用目录以及原生 codex.exe 的绝对路径。
单规则；空项、通用程序名、宽目录和 catch-all 被拒绝。生成时 appNames 只取传入 targets，不继承代理选项里的广泛目标。
专用目录尾分隔符、规范绝对路径和大小写去重；保留 SOCKS5/TLS/protocol 选项，不重写 native 匹配算法。
专用目录判断用固定 OpenAI/Codex 尾段，允许 LOCALAPPDATA 重定位；GUI只发现当前用户真实目录，后台不重新猜测用户环境。不是二进制真实性检测。

## 测试计划

CodexRoutingPolicyTests 已覆盖通用目标拒绝、桌面包/后台/CLI允许、单规则/去重、catch-all、多规则和绝对路径保持。
按用户明确要求，仅添加测试用例，不运行 RED/GREEN。
当前状态不能解释成测试通过；最终阶段集中执行。

## Git

feat: 添加 Codex 专用路由策略。
配置项目、测试项目、记录和plan同次提交同步。
