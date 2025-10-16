# Playwright Remote Browser Launcher

本项目是一个面向 Windows 的 WinForms 工具，旨在帮助开发者启动并管理基于 Chromium 的远程浏览器实例，随后通过 [Microsoft Playwright](https://playwright.dev/dotnet/) 的 CDP 支持与之交互。工具聚焦于调试及自动化场景，提供了一键启动、等待 DevTools、连接 Playwright、创建页面、导航、采集日志与截图等常用功能。

## 目录结构

```
PlaywrightRemoteBrowserLauncher/
├── Extensions/                # WinForms 扩展方法
├── Models/                    # UI 所使用的模型类型
├── Services/                  # 业务服务与 Playwright 封装
├── MainForm.cs                # 应用程序主要的交互逻辑
├── MainForm.Designer.cs       # WinForms 设计器自动生成的 UI 代码
├── Program.cs                 # 应用程序入口
└── PlaywrightRemoteBrowserLauncher.csproj
```

### 主要模块

#### `Program`
应用程序入口，负责初始化 WinForms 高 DPI/默认字体配置并启动 `MainForm`。

#### `MainForm`
WinForms 主窗体，负责：

- 维护桌面下的持久化目录（日志、截图、下载、用户数据等）；
- 启动外部浏览器进程（默认指向示例项目 `WebView2BrowserHost`）；
- 监听 DevTools 端口，确认远程调试接口可用；
- 通过 `PlaywrightController` 连接浏览器、创建上下文/页面并执行导航；
- 控制 UI 各按钮的状态机，实现“一键运行”“重置”等操作；
- 将日志写入文本框以及每日滚动的 `run-YYYYMMDD.log`；
- 提供导出 JSON (`/json/list`, `/json/protocol`)、保存快照、开启/停止日志等辅助功能；
- 与 `LoggingManager`、`PlaywrightController` 订阅的事件协同更新页面列表。

#### `Extensions`
包含对 WinForms `Control` 的扩展方法 `InvokeSafe`，用于在跨线程场景下安全更新 UI。

#### `Models`
`PageItem` 封装了 Playwright `IPage` 与友好的名称字符串，便于在列表控件中展示与切换。

#### `Services`

- **`BrowserProcessLauncher`**：使用 `ProcessStartInfo` 启动/停止带有 `--remote-debugging-port` 参数的外部浏览器进程，并可选追加代理配置。
- **`DevToolsEndpointWatcher`**：轮询 `http://127.0.0.1:<port>/json/version`，提取 `webSocketDebuggerUrl`，用于等待 DevTools 端点上线。
- **`ContextConfiguration`**：封装创建 `BrowserNewContextOptions` 所需的布尔/字符串配置（忽略 TLS、HAR 录制、初始化脚本、暴露 .NET 函数等）。
- **`LoggingManager`**：根据时间戳生成日志文件，记录控制台输出（NDJSON）与网络请求响应（CSV），并在 UI 上回显日志状态。
- **`PlaywrightController`**：对 Playwright API 的高级封装。核心职责包括：
  - 通过 CDP 连接远程浏览器；
  - 按配置创建 `BrowserContext`，注册初始化脚本与 .NET 暴露函数；
  - 创建 `IPage`、管理当前页面、执行重试导航与执行后导航脚本；
  - 截图、保存页面快照、导出调试 JSON；
  - 监听控制台、网络、下载、关闭事件，并驱动 `LoggingManager` 记录。

## 运行流程概览

1. 在 UI 中指定浏览器可执行文件路径（默认指向仓库中的示例 **WebView2BrowserHost** 项目编译产物）。
2. 点击“启动”后，`BrowserProcessLauncher` 会以指定调试端口、用户数据目录启动浏览器。
3. 通过“等待 DevTools”按钮，`DevToolsEndpointWatcher` 会轮询直到远程调试端点准备就绪。
4. “连接 Playwright”调用 `PlaywrightController.ConnectAsync` 连接到 DevTools WebSocket。
5. “创建页面”使用 `EnsureContextAsync` 创建上下文并生成新页面，同时注册日志与下载处理。
6. “访问”与“一键运行”按钮触发导航逻辑，并可自动截图、执行后置脚本。
7. 可随时通过“保存快照”“导出 JSON”“开启日志”等按钮获得额外的调试信息。
8. “关闭全部”调用 `CleanupAsync` 与 `BrowserProcessLauncher.Stop`，确保进程与 Playwright 资源释放。

## 日志与数据存储

应用在桌面目录下创建 `PlaywrightRemoteBrowserLauncher` 文件夹，内含：

- `Logs/`：每日运行日志、控制台 NDJSON、网络 CSV、快照等。
- `Screenshots/`：自动或手动触发的页面截图。
- `Downloads/`：通过 Playwright 捕获的下载文件。
- `UserData/`：浏览器进程的用户数据目录，支持持久化登录等场景。

路径均由 `MainForm` 在启动时自动创建，无需手动准备。

## 默认测试浏览器

仓库同时包含 `WebView2BrowserHost` 示例项目，可作为默认测试目标。`MainForm` 初始化时会默认将 EXE 路径指向该项目的 Debug 构建输出 (`WebView2BrowserHost/bin/Debug/net8.0-windows/WebView2BrowserHost.exe`)。开发者可通过 Visual Studio 或 `dotnet build` 先行编译该项目，然后直接使用 Launcher 进行调试与自动化测试。

## 开发与扩展建议

- **自定义浏览器**：将“浏览器路径”切换到任何兼容 Chromium 的可执行文件（需支持 `--remote-debugging-port`）。
- **日志管线**：`LoggingManager` 默认写入本地文件，若需集中化可扩展为推送至外部服务。
- **自动化脚本**：利用 `InitScript`、`Post Navigation Script` 或 `ExposeDotnet` 功能与网页交互，实现定制自动化逻辑。
- **二次开发**：`PlaywrightController` 已封装基础能力，亦可扩展新的事件处理（如 WebSocket、Worker、视频录制等）。

欢迎根据项目需要进一步扩展 UI 与服务层逻辑。
