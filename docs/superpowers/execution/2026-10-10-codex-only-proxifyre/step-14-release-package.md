# 步骤 14：Release 与安装包（执行中）

- Windows CI 现构建 Debug、Release 及独立 MSI/setup，随后执行最终测试。
- 安装器验证合同同步新服务名、UI 文件名、独立 UpgradeCode 与产品名；保留文件表、驱动和依赖检查。
- 托管套件完整执行；原生执行此次变更涉及的 OwnerRoutingPolicyTest 路由套件。
- 未安装、停止状态及 UAC 运行链路由步骤 15 记录，不以构建成功代替网络运行验收。

当前等待远端真实构建结果，主计划本步骤暂不勾选。
