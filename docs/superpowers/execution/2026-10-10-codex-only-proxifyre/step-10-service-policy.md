# 步骤 10：服务端范围校验

## 过程
- 在读取、通用校验及规范化 app-config.json 后调用共享 CodexRoutingPolicy。
- 范围错误在获取 Socksifier 原生实例前抛出并写入错误日志。
- 示例配置仅匹配 Codex WindowsApps 包前缀；默认 SOCKS5 127.0.0.1:7890、绕过局域网、Info。

## 结果
- 手工编辑 JSON 的 catch-all、普通应用名、非 Codex 路径不能通过服务启动入口。
- GUI 与服务共用策略，原有协议与凭据通用校验保留。
- 本步骤未运行测试；策略测试和完整构建统一最后执行。

