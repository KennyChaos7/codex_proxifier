# 步骤 02：构建工具实测盘点

状态：盘点完成；功能测试尚未运行。
时间：2026-10-10；执行者：Sol。

## 前一步结果

步骤01通过 GitHub Git tree/commit/ref 接口同步，远端提交 85612e686fa85d46a05811ffa658f6bcec995cf1；git fetch 成功，HEAD 与 FETCH_HEAD 文件无差异。上游 207 文件完整保留。大 tree 单请求遇到 HTTP 500，按 21 批累积 tree 后成功，最终 Git tree 与本地一致。

## 实测过程与结果

- PATH Get-Command：只定位到已捆绑的 PowerShell 7；未找到 dotnet、nuget、msbuild、gh。
- 标准位置：vswhere.exe 不存在；VS 2022 BuildTools MSBuild 不存在；C:/Program Files/dotnet/dotnet.exe 不存在；NuGet 标准 CLI 不存在。
- .NET Framework 运行时 Release=533325；运行时存在不等于 4.7.2 targeting pack 已安装。Reference Assemblies/.NETFramework 无 targeting pack 结果。
- Windows IsUserAnAdmin=false；shell 权限提升机制并没有使当前 OS 进程成为管理员。
- 官方 ProxiFyreService 仍为 Running，未停止或修改。
- 受限 PowerShell 对对象和 WindowsPrincipal 查询报 Constrained Language 错误；使用正常文件查询及 Python ctypes 只读查询获得上述结论，不修改系统。

## 上游实际要求

src/proxifyre/docs/gui.md、socksify.sln、各 csproj/vcxproj 和 .github/workflows/main.yml 要求 VS 2022/v143/C++ CLI、.NET Framework 4.7.2、NuGet、vcpkg 的 ms-gsl/boost-pool、.NET SDK 下的 WiX 6.0.2 和 PS7。

## 决定与下一步

用户已要求无需重复同意。当前 OS 无管理员能力，安装完整 VS 仍会碰到系统提权；选择 GitHub windows-2022 构建环境复用上游支持的工具链。
03 配置本仓库 build-only 工作流和统一构建脚本；04 在未定制源码上编译基线，不运行对应测试。工作流不发布 release/tag，不更改本机驱动/服务。
正式测试仍留到实现后一次集中执行；真实桌面/CLI 代理和 UAC 需要最终产物及本机可执行的管理员环境，不能用 CI 编译代替。

## Git 归档

docs: 记录 Codex Proxy 构建工具缺口。
记录与 plan 同次提交并同步 origin/custorm。测试未运行。

