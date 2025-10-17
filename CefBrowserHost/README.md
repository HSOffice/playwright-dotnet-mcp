# CefBrowserHost

`CefBrowserHost` 是一个基于 WinForms 和 [CefSharp](https://github.com/cefsharp/CefSharp) 的极简浏览器宿主程序，与 `WebView2BrowserHost` 类似，用于为 Playwright 远程调试场景提供一个独立的 Chromium 宿主进程。该示例展示了如何初始化 CEF、管理用户数据目录以及在界面中实现基础的地址栏与控制逻辑。

## 快速上手

1. 在解决方案根目录执行 `dotnet restore CefBrowserHost/CefBrowserHost.csproj` 下载依赖；随后运行 `dotnet build CefBrowserHost/CefBrowserHost.csproj -p:EnableWindowsTargeting=true` 进行编译。
2. 运行生成的 `CefBrowserHost.exe`，可以使用命令行参数自定义用户数据目录与初始页面地址。
3. 在需要基于 CEF 的调试流程中，将浏览器可执行文件路径指向该程序，即可通过 DevTools 或 Playwright 等工具连接。

> **提示**：CEF 运行时需要配套的依赖文件。`dotnet build` 会自动从 NuGet 拉取并放置到输出目录，请确保在分发时将整个输出文件夹一起携带。

## 命令行参数

| 参数 | 说明 | 默认值 |
| ---- | ---- | ------ |
| `--user-data-dir="<path>"` | 指定用户数据目录；未提供时会自动创建临时目录 | 随机临时目录 |
| `--url="<url>"` | 指定启动后加载的初始地址 | `https://example.com` |

## 运行时功能要点

* `Program.cs` 中在启动时初始化 `CefSettings`，开启持久化会话、缓存目录以及基本的日志记录。【F:CefBrowserHost/Program.cs†L25-L61】
* `BrowserForm` 提供了地址栏、页面标题同步、控制台消息输出以及 F5 刷新、Ctrl+L 聚焦地址栏等常用浏览器功能，方便在手动测试过程中使用。【F:CefBrowserHost/Program.cs†L63-L189】
* 默认对地址栏输入进行智能识别，既支持完整 URL，也支持关键字搜索，行为与 `WebView2BrowserHost` 保持一致。【F:CefBrowserHost/Program.cs†L120-L160】

