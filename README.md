Playwright .NET MCP
A Model Context Protocol (MCP) Server for .NET integrating Microsoft Playwright

浏览器自动化智能体服务器 (.NET 版)
基于 Microsoft.Playwright 与 Model Context Protocol 架构

🧩 概述 | Overview

Playwright .NET MCP 是一个面向大语言模型（LLM）与智能体框架的 浏览器自动化服务器。
它基于 Microsoft 官方的 Playwright for .NET 与 Model Context Protocol (MCP) 实现，
允许 LLM 以结构化的方式访问和控制网页，而无需视觉模型或截图解析。

🔍 主要特性：

基于 可访问性树（Accessibility Tree） 的结构化页面理解

纯文本/结构化输入输出，无需视觉模型

确定性操作（非像素定位）

完全兼容 WinForms / Console / ASP.NET / MCP Host

⚙️ 系统要求 | Requirements
组件	要求
操作系统	Windows 10/11（或兼容的 Linux/macOS 环境）
.NET SDK	.NET 8.0 或更新版本
Playwright	Microsoft.Playwright 1.55 或更新版本
环境依赖	Edge WebView2 Runtime（若使用 WinForms/WebView 模式）
IDE	Visual Studio 2022 / Rider / VS Code
MCP 客户端	Cursor, Windsurf, Claude Desktop, Codex, or MyMcpHost
🚀 快速上手 | Getting Started
1️⃣ 安装依赖
dotnet add package Microsoft.Playwright
dotnet add package ModelContextProtocol.Server


初始化 Playwright 驱动：

pwsh bin/Debug/net8.0/playwright.ps1 install

2️⃣ 启动 MCP 服务器
dotnet run --project PlaywrightMcpServer


或在 VS / Rider 中直接启动 PlaywrightMcpServer 项目。

默认将通过 标准输入输出（STDIO） 启动 MCP 服务端。
如需 HTTP 方式，可指定端口参数：

dotnet run --project PlaywrightMcpServer -- --port 8931


在 MCP 客户端配置文件中（如 VS Code settings.json）：

{
  "mcpServers": {
    "playwright-dotnet": {
      "url": "http://localhost:8931/mcp"
    }
  }
}

⚡ 工具结构 | Tools & Capabilities

Playwright .NET MCP 提供一组基于 [McpServerTool] 注解的可调用工具类，
每个工具均以 异步 C# 方法 实现，支持 LLM 的自动参数绑定与结构化返回。

工具文件	功能	示例方法
PlaywrightTools.Actions.Navigate.cs	打开网页	BrowserNavigateAsync(url)
PlaywrightTools.Actions.Form.cs	填写表单	BrowserFillFormAsync(fields[])
PlaywrightTools.Actions.Snapshot.cs	获取页面快照	BrowserSnapshotAsync()
PlaywrightTools.Actions.Relaunch.cs	启动 / 重启浏览器	BrowserRelaunchAsync()
PlaywrightTools.Actions.Close.cs	关闭浏览器	BrowserCloseAsync()

✅ 每个工具方法返回结构化 JSON（或 YAML 块），例如：

{
  "status": 200,
  "url": "https://example.com/",
  "title": "Example Domain",
  "snapshot": "- heading \"Example Domain\" [level=1]\n- link \"Learn more\" [href=https://example.org/]"
}

🧠 快照机制 | Snapshot System

在 TypeScript 版中，Playwright MCP 内部通过 page._snapshotForAI() 生成 带 ref 标签的 YAML 快照。
.NET 版中该方法未公开，因此在 BrowserSnapshotAsync() 中实现了 可替代机制：

✅ 实现逻辑

通过 page.Accessibility.SnapshotAsync() 获取可访问性树

以 YAML 序列化输出（含 [ref=eX] 唯一标识符）

支持表单元素、按钮、链接、层级结构

保留页面 URL、标题、控制台消息、下载信息等上下文

示例输出：

- document [ref=e1]:
  - heading "智慧校园后台" [level=1] [ref=e2]
  - textbox "请输入账号" [ref=e3]
  - textbox "请输入密码" [ref=e4]
  - button "登录" [ref=e5]

🧩 MCP 交互协议 | Protocol Structure

Playwright .NET MCP 遵循标准 Model Context Protocol (MCP) 定义：

输入：结构化 JSON 参数

输出：结构化 JSON / Markdown / YAML

支持：sse、stdio、http 多种传输模式

工具调用：通过 [McpServerTool] 自动注册到协议上下文

🧱 典型架构 | Architecture
┌────────────────────────────┐
│        LLM / MCP Client    │
│ (Claude, Cursor, MyMcpHost)│
└──────────────┬─────────────┘
               │
        JSON-RPC / SSE / STDIO
               │
┌──────────────┴─────────────┐
│     Playwright .NET MCP    │
│   (ModelContextProtocol)   │
│                            │
│  ├── PlaywrightTools.cs    │
│  ├── Actions/              │
│  │   ├─ Navigate.cs        │
│  │   ├─ Form.cs            │
│  │   ├─ Snapshot.cs        │
│  │   └─ Relaunch.cs        │
│  ├── Common/Response.cs    │
│  └── Helpers/Serializer.cs │
└────────────────────────────┘
               │
       Microsoft.Playwright

🧠 运行模式 | Runtime Modes
模式	描述
Persistent Profile	浏览器用户数据保存在 %LOCALAPPDATA%\ms-playwright\mcp-dotnet-profile
Isolated Mode	启动临时上下文，退出即销毁
Headless Mode	通过 --headless 参数运行无界面模式
Extension Mode (计划中)	支持连接到外部已运行浏览器
📦 典型配置 | Example appsettings.json
{
  "McpServer": {
    "Port": 8931,
    "Headless": true,
    "Isolated": true,
    "ViewportSize": "1280x720",
    "Timeouts": {
      "Action": 5000,
      "Navigation": 60000
    }
  }
}

🔍 调试与追踪 | Debugging & Tracing

可在 --output-dir 中启用调试信息：

保存浏览器 Trace (trace.zip)

保存视频录制 (session.mp4)

保存 YAML 快照 (page.yaml)

dotnet run -- --output-dir "C:\McpOutput" --save-trace

📄 扩展能力 | Extended Capabilities
功能	说明	启用方式
PDF 导出	生成页面 PDF	--caps=pdf
可视化定位	允许坐标点击	--caps=vision
跟踪分析	保存操作轨迹	--caps=tracing
校验模式	页面状态一致性检查	--caps=verify
🧰 示例：C# 代码调用 MCP 工具
var server = new McpServer();
await server.RegisterToolsAsync(typeof(PlaywrightTools));
await server.RunAsync(); // 启动 STDIO 模式

// 工具注册示例
[McpServerTool]
public static async Task<string> BrowserNavigateAsync(string url, CancellationToken token)
{
    await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
    return Serialize(new { url, title = await _page.TitleAsync() });
}

📚 项目结构建议 | Project Structure
PlaywrightMcpServer/
├── PlaywrightTools.cs
├── Actions/
│   ├── Navigate.cs
│   ├── Form.cs
│   ├── Snapshot.cs
│   ├── Relaunch.cs
│   └── Close.cs
├── Common/
│   ├── Response.cs
│   └── Serializer.cs
├── Properties/
│   └── launchSettings.json
└── Program.cs

✅ 总结 | Summary

Playwright .NET MCP 是 TypeScript 版 Playwright MCP 的完整 .NET 化移植：

全面兼容 MCP 标准

提供确定性可访问性快照（非像素级）

支持 WinForms、控制台、HTTP、SSE 多种运行环境

可作为企业级 AI 智能体平台的浏览器操作中枢
