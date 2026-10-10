# 步骤 03：Windows 构建环境与依赖入口

状态：构建配置已写入；自动化测试未运行；实际编译由04记录。
时间：2026-10-10；执行者：Sol。

## 前一步

02 已同步 GitHub：33f0181dd7ed7efb354e2be080626b44c880becb。

## 实际操作

- 添加根 .github/workflows/build-codexproxy.yml，使用与上游支持的 windows-2022/VS2022、PS7、NuGet、vcpkg ms-gsl/boost-pool、.NET SDK。
- 添加 scripts/Build-CodexProxy.ps1，固定 x64、Debug/Release 参数，按仓库目录解析源码；恢复四个 packages.config 和两个原生安装辅助项目，编译 socksify.sln。
- Release 可显式 -BuildInstaller，使用锁定 WiX 6.0.2 图及上游安装脚本。
- 工作流仅由工作流文件改动或手动 dispatch 触发，不在每个功能提交重复构建；当前只编译 Debug 基线、上传构建产物，不运行测试、不发布 release/tag。
- 没有在本机安装完整 VS、修改管理员配置或启动新服务。

## 结果与后续

构建配置交付完成。编译运行是否通过以04的 GitHub Actions 实际结果为准，不以文件存在判定通过。
用户要求测试后置，最后更新工作流加入正式测试/Release 后统一执行。

## Git

build: 配置 Codex Proxy Windows 构建入口。
配置/记录/plan 同次提交并同步 origin/custorm。

