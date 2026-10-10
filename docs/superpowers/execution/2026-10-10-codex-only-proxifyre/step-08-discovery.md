# 步骤 08：Codex 桌面与原生 CLI 发现

状态：发现逻辑和用例写入；测试未执行。
时间：2026-10-10；执行者：Sol。
前一步远端：b02484218aacc219e5839d88a95826468118ce39。

## 过程与结果

新增 CodexTargetDiscovery 与 CodexPathCandidate；Discover/FindCliCandidates 共用真实原生文件发现。
候选顺序为运行进程、PATH、专用安装目录、npm安装；只有现存 codex.exe，按完整路径大小写去重并合并来源。
桌面通过运行进程的实际 WindowsApps 路径和系统 Get-AppxPackage 固定查询获得 OpenAI.Codex_ 包前缀。只查询包信息，不执行发现到的 Codex/npm 脚本。
专用目录只读遍历当前用户 OpenAI/Codex 和 Programs/OpenAI/Codex；npm只在 PATH 对应 node_modules/@openai/codex* 下寻找原生 EXE，不代理 node或shim。
没有全盘扫描、持久候选缓存和后台监控。当前 DisplayPath 暂为实际路径，09统一实现隐私格式，GUI接入在11。

## 测试状态

用例覆盖同路径来源合并、缺失项过滤、排序保持、npm找到native而不是node/cmd。
按用户要求尚未运行。
测试文件使用独立临时目录；只在测试最终执行时创建/移除该临时目录。

## Git

feat: 发现 Codex 桌面与原生 CLI 路径。
GUI项目、测试项目、过程记录和plan同次提交同步。
