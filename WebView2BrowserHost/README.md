# WebView2BrowserHost

`WebView2BrowserHost` 是一个使用 WinForms 和 Microsoft Edge WebView2 控件构建的极简浏览器宿主程序，用于为 Playwright 远程连接提供一个可调试的 Chromium 环境。该程序也是 `PlaywrightRemoteBrowserLauncher` 测试场景中使用的默认示例，便于验证启动器在本地进程上的交互流程。

## 快速上手

1. 在解决方案根目录执行 `dotnet build WebView2BrowserHost/WebView2BrowserHost.csproj` 进行编译。
2. 运行生成的 `WebView2BrowserHost.exe`，可选地通过命令行参数控制调试端口、用户数据目录以及初始地址。
3. 在 Playwright 调试或 `PlaywrightRemoteBrowserLauncher` 中，将浏览器可执行文件路径指向该程序，即可通过 DevTools 协议连接。

## 命令行参数

程序支持以下命令行参数，方便在不同测试场景下调整行为：

| 参数 | 说明 | 默认值 |
| ---- | ---- | ------ |
| `--remote-debugging-port=<port>` | 指定对外暴露的 DevTools 调试端口 | `9222` |
| `--user-data-dir="<path>"` | 指定用户数据目录；未提供时会自动创建临时目录 | 随机临时目录 |
| `--url="<url>"` | 指定启动后加载的初始地址 | `https://example.com` |

## 运行时功能要点

* 在 `BrowserForm` 中初始化 WebView2 控件并开启 DevTools，确保 Playwright 可以通过指定的端口连接到嵌入式 Chromium。【F:WebView2BrowserHost/Program.cs†L45-L86】
* 支持通过 F5 刷新和 `Ctrl+L` 修改地址等键盘快捷键，便于手动测试页面加载逻辑。【F:WebView2BrowserHost/Program.cs†L88-L106】
* 监听 WebMessage 并输出到控制台，方便在调试时观察页面向宿主发送的消息。【F:WebView2BrowserHost/Program.cs†L111-L114】
* 在窗口标题中实时展示页面标题与调试端口信息，帮助区分不同实例。【F:WebView2BrowserHost/Program.cs†L72-L86】

## 在 PlaywrightRemoteBrowserLauncher 测试中的角色

`PlaywrightRemoteBrowserLauncher` 的默认配置指向本项目生成的可执行文件（`WebView2BrowserHost.exe`），用于作为真实浏览器进程的替身进行端到端流程验证。【F:PlaywrightRemoteBrowserLauncher/MainForm.cs†L66-L67】借助该宿主，可以在不依赖 Edge 安装路径的情况下快速验证远程调试连接、用户数据目录切换以及启动参数注入等逻辑。

