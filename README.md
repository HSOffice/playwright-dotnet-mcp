# Playwright .NET MCP

> 浏览器自动化智能体服务器 (.NET 版) ·
> A Model Context Protocol server toolkit powered by Microsoft Playwright for .NET

Playwright .NET MCP 为需要将浏览器能力暴露给大语言模型 (LLM) 或 Agent 宿主的应用，提供一套 .NET 实现的工具集合。项目遵循 [Model Context Protocol](https://github.com/modelcontextprotocol) 规范，通过 `ModelContextProtocol.Server` 包的 `McpServerTool` 特性描述工具参数与返回值，让宿主可以在不依赖视觉模型的情况下，结构化地驱动网页。

- **结构化快照**：利用可访问性树生成与 TypeScript 版兼容的 YAML/Markdown 快照格式。
- **确定性操作**：通过 Playwright 定位器完成导航、表单、鼠标、键盘等操作，避免坐标点击的不确定性。
- **Agent 友好输出**：统一的 `Response` 序列化逻辑输出结果、代码片段、标签摘要与快照文本，方便客户端解析。

---

## 🗂️ 仓库结构 | Repository Layout

| 路径 | 说明 |
| --- | --- |
| `dotnet/` | Playwright MCP 核心实现：`PlaywrightTools` 部分类、响应序列化、Tab 管理与快照构建。|
| `dotnet/PlaywrightMcpServer.Tests/` | xUnit 测试套件，覆盖响应模型、快照 Markdown 构建、Tab 状态等逻辑。|
| `docs/` | 设计记录与迁移规划，含快照能力说明、TypeScript 版本对照等。|

---

## 🔍 能力速览 | Feature Highlights

### 浏览器工具分组

| 分组 | 代表工具 | 功能摘要 |
| --- | --- | --- |
| 导航 Navigation | `browser_relaunch`, `browser_navigate`, `browser_navigate_back` | 启动/重启浏览器、访问目标 URL、处理历史导航。|
| 输入 Input | `browser_fill_form`, `browser_click`, `browser_drag`, `browser_hover`, `browser_select_option` | 使用可访问性定位器完成表单填写、点击、拖拽与悬停。|
| 键鼠 Keyboard & Mouse | `browser_press_key`, `browser_type`, `browser_mouse_move_xy`, `browser_mouse_click_xy` | 提供键盘按压、逐字输入与绝对坐标点击等低阶操作。|
| 媒体 Media & Files | `browser_take_screenshot`, `browser_pdf_save`, `browser_file_upload` | 支持截图、导出 PDF 以及文件上传。|
| 状态感知 State & Snapshot | `browser_snapshot`, `browser_tabs`, `browser_network_requests`, `browser_console_messages` | 导出页面快照、Tab 摘要、网络与控制台事件。|
| 调试 Debugging | `browser_start_tracing`, `browser_stop_tracing`, `browser_generate_locator` | 控制 Playwright Trace 录制并生成定位器辅助信息。|
| 校验 Verification | `browser_verify_element_visible`, `browser_verify_text_visible`, `browser_verify_list_visible`, `browser_verify_value` | 根据快照结果执行断言，便于构建回归检查。|

每个工具以异步静态方法实现，并通过 `ExecuteWithResponseAsync` 统一封装响应、错误与附加快照逻辑，保持输出格式一致。辅助组件 `TabManager` 与 `SnapshotManager` 负责多页面状态、下载记录、模态窗口等上下文管理。

### 快照与响应体系

- `SnapshotMarkdownBuilder` 将可访问性树、控制台、网络、下载等信息整合为 Markdown 文本，供 MCP 客户端直读。
- `Response`/`ResponseContent`/`ResponseJsonSerializer` 组成响应管线，可根据工具配置输出结果、代码片段、图片与 Tab 概览。
- `SecretRedactor` 支持在序列化前对敏感字段进行脱敏处理。

---

## ⚙️ 系统要求 | Requirements

| 组件 | 最低版本 |
| --- | --- |
| 操作系统 | Windows 10/11, macOS 12+, 或支持 .NET 8 的 Linux 发行版 |
| .NET SDK | 8.0 |
| Microsoft.Playwright | 1.41.2 (随测试项目引用，可根据需要更新) |
| IDE / 编辑器 | Visual Studio 2022、Rider、VS Code 等 |
| MCP 客户端 | 任意兼容 MCP 的宿主（例如 Claude Desktop、Cursor、Windsurf 等） |

> **提示**：首次运行前请使用 `playwright.ps1 install` 或 `playwright.sh install` 安装浏览器运行时，命令由 NuGet 包 `Microsoft.Playwright` 自动生成。

---

## 🚀 快速开始 | Getting Started

1. **克隆仓库 / Clone the repository**
   ```bash
   git clone https://github.com/<your-org>/playwright-dotnet-mcp.git
   cd playwright-dotnet-mcp
   ```
2. **恢复并运行测试 / Restore & run tests**
   ```bash
   dotnet test dotnet/PlaywrightMcpServer.Tests
   ```
   首次执行会构建测试工程并验证快照、响应与 Tab 管理等核心逻辑。
3. **集成至 MCP 宿主 / Integrate with your MCP host**
   - 在自定义宿主中引用 `ModelContextProtocol.Server` 与本项目生成的程序集。
   - 注册 `PlaywrightTools` 部分类为工具提供者，然后选择合适的传输方式（STDIO、SSE、HTTP 等）。
   - 详细集成方式可参考宿主框架的 `McpServer` / `McpServerBuilder` API 文档。

---

## 🔧 运行时配置 | Runtime Configuration

`PlaywrightTools` 支持通过环境变量调整行为：

| 环境变量 | 作用 |
| --- | --- |
| `MCP_PLAYWRIGHT_HEADLESS` | 设为 `true` 时使用无头模式启动浏览器。|
| `MCP_PLAYWRIGHT_DOWNLOADS_DIR` | 自定义下载文件保存目录，默认 `./downloads`。|
| `MCP_PLAYWRIGHT_VIDEOS_DIR` | 自定义视频录制保存目录，默认 `./videos`。|
| `MCP_PLAYWRIGHT_CHROMIUM_CHANNEL` | 指定 Chromium 启动通道（如 `msedge`、`chrome`）。未设置时，Windows/Linux 默认使用 `msedge`。|

此外，截图、PDF、Trace 分别写入 `./shots`、`./pdf`、`./traces`，在 `EnsureDirectories()` 中自动创建。

---

## 🧪 测试覆盖 | Test Coverage

测试项目通过 Moq 模拟 Playwright 行为，验证以下能力：

- 下载记录与快照文本是否正确写入响应。
- `SnapshotMarkdownBuilder` 生成的 Markdown 区块是否符合预期结构。
- `TabManager` 的激活、等待完成与状态序列化逻辑。

执行 `dotnet test` 可在 CI 或本地快速回归这些关键功能。

---

## 📚 相关文档 | Further Reading

- [docs/ARCHITECTURE_PLAN.md](docs/ARCHITECTURE_PLAN.md)：.NET 版 MCP 服务的分层规划与后续路线。 
- [docs/snapshot_capabilities.md](docs/snapshot_capabilities.md)：快照字段对照与设计考量。 
- [docs/tab-capture-snapshot.md](docs/tab-capture-snapshot.md)：多 Tab 捕获与快照流程说明。 

---

## 🛠️ 贡献指南 | Contributing

欢迎通过 Issue / PR 讨论：

1. Fork & 创建特性分支。
2. 完成变更后运行 `dotnet test` 确认通过。
3. 提交 PR 并说明变更动机、测试结果。

感谢你对 Playwright .NET MCP 的关注！
