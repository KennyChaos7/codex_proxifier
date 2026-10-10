# Codex 与 ChatGPT 桌面代理

在 Windows 上使用 [ProxiFyre](https://github.com/wiresock/proxifyre)，将 Codex 和 ChatGPT 桌面程序的匹配流量转发至本地 SOCKS5 入口 `127.0.0.1:7890`。

使用 ProxiFyre 自带图形界面控制服务、最小化到任务栏和收起到托盘。无需安装商业软件 Proxifier，无需开启 TUN 虚拟网卡或 Windows 系统代理；需要安装 ProxiFyre 的 Windows Packet Filter 网络过滤驱动。

## 环境要求

| 项目 | 要求 |
| --- | --- |
| 操作系统 | Windows x64；本机验证环境为 Windows x64、.NET Framework 4.8.1 |
| 代理引擎 | ProxiFyre 2.6.2 |
| 上游代理 | FlClash 或其他支持 SOCKS5 的本地代理，监听 `127.0.0.1:7890` |
| 权限 | 安装驱动、写入 Program Files 和控制代理服务需要管理员权限 |

7890 可以是同时支持 HTTP 和 SOCKS5 的 mixed-port。此配置使用 SOCKS5，因此只支持 HTTP 的监听端口不能直接替代。

## 1. 安装 ProxiFyre

1. 打开 [ProxiFyre 2.6.2 官方发布页](https://github.com/wiresock/proxifyre/releases/tag/v2.6.2)，下载 `ProxiFyre-2.6.2-win-x64-setup.exe`。
2. 运行安装器，查看许可条款并完成 Windows 管理员授权。安装器会按需要安装运行库和 Windows Packet Filter 驱动。
3. 从开始菜单打开 ProxiFyre。

本机下载的安装包已与官方发布的 SHA-256 核对一致。官方说明首方程序未签名；遇到“未知发布者”提示时，核对下载来源和发布页的校验信息。文件校验不等于代码签名。

## 2. 准备本地代理

启动 FlClash 或其他 SOCKS5 服务，确认 `127.0.0.1:7890` 可用。保持上游代理程序运行。

如果需要让其他应用恢复直连，应在 FlClash 中关闭 TUN，并关闭 Windows 系统代理。ProxiFyre 规则不会替你关闭这些已有的全局代理设置。

## 3. 配置应用规则

配置文件默认位于：

```text
C:\Program Files\ProxiFyre\app-config.json
```

在 ProxiFyre 中编辑规则，或用管理员权限将以下 JSON 保存到该文件。使用 UTF-8 编码。

**先将 `YOUR_USER` 替换为你的 Windows 用户目录名，并核对 Codex 的实际安装位置。** ProxiFyre 不会自动把示例中的用户名或环境变量展开成实际路径。

可在 PowerShell 中查看当前用户的 Codex 专用目录：

```powershell
Join-Path $env:LOCALAPPDATA 'OpenAI\Codex\'
```

```json
{
  "logLevel": "Info",
  "bypassLan": true,
  "proxies": [
    {
      "appNames": [
        "C:\\Program Files\\WindowsApps\\OpenAI.Codex_",
        "C:\\Users\\YOUR_USER\\AppData\\Local\\OpenAI\\Codex\\",
        "ChatGPT.exe"
      ],
      "socks5ProxyEndpoint": "127.0.0.1:7890",
      "socks5Transport": "TCP",
      "supportedProtocols": ["TCP", "UDP"],
      "supportedAddressFamilies": ["IPv4", "IPv6"]
    }
  ],
  "excludes": []
}
```

### 三个匹配项的含义

| 匹配项 | 覆盖范围 |
| --- | --- |
| `C:\Program Files\WindowsApps\OpenAI.Codex_` | Codex 桌面应用包内的程序，保留前缀以适应版本目录变化 |
| `C:\Users\YOUR_USER\AppData\Local\OpenAI\Codex\` | 当前用户的 Codex 后台和专用运行时 |
| `ChatGPT.exe` | 使用这个进程名的 ChatGPT 桌面程序及子进程 |

Codex 桌面应用包内部的主程序也可能叫 `ChatGPT.exe`。它与独立 ChatGPT 桌面应用应按安装路径区分，不能仅凭进程名判断是哪一个应用。

ProxiFyre 对包含路径分隔符的匹配项按完整程序路径的片段匹配；对 `ChatGPT.exe` 这样的项按程序名匹配，均忽略大小写。

JSON 中只有一个代理对象，因此界面可能只显示一行，但这一行包含三个应用匹配项。双击或选择 **Edit** 可查看完整列表。

`socks5Transport: TCP` 指 SOCKS5 连接使用的传输方式；需要转发的协议由 `supportedProtocols` 单独指定。`bypassLan: true` 会让局域网流量直连。

## 4. 应用与日常操作

1. 如果从外部修改了配置文件，点击 **Reload** 重新读取。
2. 点击 **Validate**，确认显示 **Configuration valid**。
3. 点击 **Apply & Restart**，完成必要的管理员授权。
4. 确认服务显示 **Running**，并在 **Logs** 中查看应用规则加载记录。

| 操作 | 效果 |
| --- | --- |
| Start | 启动代理服务 |
| Stop | 停止代理服务 |
| Restart / Apply & Restart | 重启服务；重启会影响现有连接 |
| 右上角最小化按钮 | 最小化到任务栏 |
| 关闭窗口 | 收起到托盘 |
| 托盘菜单或重新打开 ProxiFyre | 恢复控制窗口 |

退出图形界面不会自动停止代理服务。需要关闭代理时，使用 **Stop**。

此配置没有额外设置 Windows 开机自启或定时任务。修改规则后，应使用新建连接验证，现有长连接不一定立即切换。

## 5. 验证独立转发

1. 确认 ProxiFyre 服务运行，上游 7890 端口保持监听。
2. 关闭 FlClash 的 TUN 和 Windows 系统代理，排除它们对测试结果的影响。
3. 在 Codex 或 ChatGPT 桌面应用中发起新请求。
4. 结合 ProxiFyre 的转发日志和连接记录，确认请求经 ProxiFyre 连接至 `127.0.0.1:7890`，且应用收到响应。

只看到 **Running**、配置有效或端口握手成功，不足以证明应用的真实请求已经转发。

需要详细排查时，可在 **Settings / Diagnostics** 中临时把引擎日志级别改为 **Debug**，应用后发起新请求。排查完成后恢复 **Info**，避免持续产生大量日志。

### 本机验证结果（2026-10-10）

- TUN 和 Windows 系统代理关闭后，Codex 桌面应用及 `codex.exe` 后台的 HTTPS 连接仍能对应到 ProxiFyre 转发套接字，并经本地 7890 端口建立连接。
- 新增的 `ChatGPT.exe` 规则已写入配置，服务日志确认已成功关联至 `127.0.0.1:7890`。
- 独立 ChatGPT 桌面应用的实际请求尚未实测；TCP 验证结果不能代替 UDP、所有 DNS 或所有外部工具的验证。

## 常见问题

### 界面显示 “Running; apply state unknown”

这表示界面尚未确认运行配置的应用状态。先点击 **Reload**，查看配置和服务日志；如果修改了规则，再使用 **Apply & Restart**。该提示本身不能证明转发成功或失败。

### 新规则没有生效

确认 Windows 管理员授权已完成，核对安装目录中的 `app-config.json` 和服务加载日志，再发起新连接。授权取消或未完成时，配置可能仍停留在旧版本。

### ChatGPT 网页为什么没有被代理

这里配置的是桌面程序 `ChatGPT.exe`。Chrome、Edge 等浏览器中的 ChatGPT 网页不在匹配范围内。

### 配置有效，但应用无法联网

检查本地 SOCKS5 服务、7890 端口、上游节点及应用匹配路径。服务启动成功不代表上游代理可用。不要把 Python、Git、Node、浏览器或全部应用加入规则来掩盖路径错误。

### 换用户或移动安装目录后失效

更新 Codex 专用目录的匹配项。若 ChatGPT 桌面程序的实际进程名不是 `ChatGPT.exe`，应根据真实安装路径或进程名调整规则。

## 覆盖范围与限制

- 规则匹配 Codex 专用目录和 `ChatGPT.exe`；未匹配的进程不由此规则代理。
- 系统 Git、Python、通用 Node、外部浏览器等共享程序不包含在内，即使由 Codex 启动。
- Windows DNS 请求可能由共享系统服务发出，不能承诺所有 DNS 均走代理。
- UDP 转发需要本地代理及上游节点支持。ProxiFyre 官方说明部分碎片化 IPv6 UDP 会直连。
- 局域网流量按当前 `bypassLan` 设置直连；云端执行产生的流量不经过本机。
- 如果 TUN 或系统代理仍开启，其他应用仍可能被它们代理，不能据此判断只有 Codex 和 ChatGPT 走代理。

## 官方资料

- [ProxiFyre 源码](https://github.com/wiresock/proxifyre)
- [2.6.2 发布与安装包](https://github.com/wiresock/proxifyre/releases/tag/v2.6.2)
- [配置规则](https://github.com/wiresock/proxifyre/blob/v2.6.2/docs/configuration.md)
- [安装器说明](https://github.com/wiresock/proxifyre/blob/v2.6.2/docs/installer.md)
- [图形界面与托盘](https://github.com/wiresock/proxifyre/blob/v2.6.2/docs/gui.md)
