# DetectBrowserProcess

DetectBrowserProcess 是一个基于 Windows Forms 的辅助工具，用于分析当前 Windows 系统中正在运行的进程，并推断目标进程是否嵌入了常见的浏览器运行时（如 Electron、Chromium Embedded Framework、WebView2 等）。该工具可快速输出 JSON 报告，并提供简单的启发式分析结论，方便调试和问题排查。

## 功能特点

- **进程枚举**：列出当前操作系统中所有可见进程，支持选择任意目标进程。
- **模块探测**：使用 ToolHelp API 枚举进程已加载的模块，识别关键 DLL 并获取文件版本信息。
- **资源文件检查**：检测典型浏览器运行时附带的资源文件（例如 `icudtl.dat`、`resources.pak`、`app.asar` 等）。
- **启发式判定**：基于模块与资源文件命中情况，推断目标进程是否使用 Electron、CEF、WebView2 或其他 Chromium 内核。
- **报告导出**：支持以原始或格式化 JSON 形式复制或保存探测结果，并展示人类可读的分析摘要。

## 运行环境

- Windows 10 或更高版本操作系统。
- [.NET 8.0 SDK](https://dotnet.microsoft.com/zh-cn/download)（需支持 `net8.0-windows` 目标框架）。
- 运行时需要具有访问目标进程及其模块的权限，建议使用管理员权限启动。

## 构建与运行

1. 安装 .NET 8 SDK 后，在 Windows 终端中进入项目根目录：
   ```powershell
   cd DetectBrowserProcess
   ```
2. 通过 `dotnet build` 构建项目：
   ```powershell
   dotnet build
   ```
3. 构建成功后，可使用 `dotnet run` 直接运行，或在 `bin/Debug/net8.0-windows/` 中找到生成的可执行文件：
   ```powershell
   dotnet run
   ```

> **注意**：由于该项目依赖 Windows 特有的 API 和 Windows Forms，请在 Windows 环境下运行；在其他操作系统上编译或运行会失败。

## 使用说明

1. 启动应用程序后，默认显示系统内所有进程列表，可使用「Refresh」按钮刷新列表。
2. 选择需要分析的目标进程，点击「Detect」按钮开始探测。
3. 界面上方的文本框会展示探测得到的 JSON 数据，勾选「Pretty JSON」可切换格式化显示。
4. 如需复制或保存结果，可使用「Copy JSON」和「Save JSON」按钮。
5. 界面下方的「Analysis Result」区域会给出启发式分析结论、置信度、关键证据以及部分命令行与模块信息。

## JSON 输出结构

探测结果会序列化为如下结构的 JSON 对象：

```json
{
  "pid": 1234,
  "process": "MyApp",
  "path": "C:\\Path\\To\\MyApp.exe",
  "commandLine": "MyApp.exe --flag",
  "foundModules": [
    { "Name": "libcef.dll", "Path": "...", "Version": "FileVersion=..." }
  ],
  "resources": {
    "\\icudtl.dat": true,
    "\\resources.pak": false,
    "\\resources\\app.asar": true
  },
  "heuristic": {
    "isCEF": true,
    "isWebView2": false,
    "isElectron": true,
    "isChromium": true
  }
}
```

- `foundModules`：列出探测到的部分模块（含路径与版本信息），用于人工复核。
- `resources`：标记常见资源文件是否存在。
- `heuristic`：表示启发式判断的命中情况，可配合分析摘要理解结论来源。

## 常见问题

- **无法列出所有进程**：某些系统进程需要更高权限才能枚举，尝试以管理员身份运行本工具。
- **分析结果不准确**：启发式判断依赖模块与文件命名约定，针对定制修改较多的应用可能需要手动复核。
- **保存 JSON 失败**：确认目标目录是否具有写入权限，或更换保存路径。

## 许可协议

本项目遵循仓库根目录中的 [LICENSE](../LICENSE) 协议发布。
