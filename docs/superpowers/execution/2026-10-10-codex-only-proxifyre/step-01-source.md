# 步骤 01：固定上游源码导入

状态：源码交付完成；按用户最新要求，测试统一在实现结束后执行。
时间：2026-10-10；执行者：Sol。

## 输入与过程

- 前一步同步：本机 git push 再次因缺少凭据失败；GitHub 连接账号 KennyChaos7 可用。
- 使用 GitHub create_tree/create_commit/create_branch 发布步骤 00 的相同文件树到 custorm；远端提交 2957a98cfa33d2d45a085950919be790cfbea789。
- git fetch origin custorm 成功；git diff HEAD FETCH_HEAD 无差异。随后仅使用 reset --soft 将本地分支定位到相同内容的接口提交，建立 origin/custorm 跟踪；未改动未跟踪文件。
- Invoke-WebRequest 从固定 commit 的 codeload 下载 downloads/proxifyre-eef55ba-source.zip，归档 1,255,517 bytes。
- 在 downloads/source-import/ 解压；确认源和目标绝对路径均位于工作区后，移动到 src/proxifyre/。
- 上游 207 个文件，LICENSE、socksify.sln、ProxiFyreUI、ProxiFyre.Configuration、ProxiFyre 和 netlib 均存在；没有 .git。
- 写 UPSTREAM.md 并保留上游原始材料；根 ignore 仅忽略本次下载/解压暂存。

## 决定

- 用户最新指令覆盖原计划的逐步 RED/GREEN：测试在实现结束后统一执行；实施勾选只表示交付物/记录完成，测试状态另记。
- 来源仓库已跟踪的部分配置和原生源码文件命中上游 broad ignore。仅在本次未改动基线导入时定向 force add src/proxifyre/，保证归档中的源码文件完整；不对后续构建输出 force add。
- 不新增永久 Git 同步框架；直接用已连接 GitHub 的 Git tree/commit/ref 工具发布每步，并 fetch 相同内容的提交作为本地基线。

## 结果与测试

结果：固定源码在库，未修改产品行为。
测试：未运行，按要求推迟。
构建：尚未运行；02 盘点已显示本机缺工具，后续选 Windows 构建环境。

## Git 归档

chore: 导入 ProxiFyre 2.6.2 固定源码。
本步骤使用 GitHub Git 接口同步到 origin/custorm；回执记录在下一步骤。归档不包含下载文件、旧 config/、旧脚本和旧验证文件。

