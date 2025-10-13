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

## 🧭 迁移蓝图速览 | Migration Blueprint at a Glance

为帮助贡献者快速理解 TypeScript 参考实现与 .NET 版本之间的差异，以下内容摘自 [`docs/PlaywrightMcp_Analysis_Report.md`](docs/PlaywrightMcp_Analysis_Report.md)，并在此总结：

### TypeScript 版关键模块

| 模块路径 | 职责概述 |
| --- | --- |
| `ts/mcp/browser/browserContextFactory.ts` | 创建持久化、隔离、远程、CDP 或共享等多类型浏览器上下文，统一注入追踪、初始化脚本与退出清理逻辑。 |
| `ts/mcp/browser/context.ts` | 管理上下文生命周期、Tab 列表、模态状态、请求拦截、录制钩子及快照产物（视频、追踪、下载）。 |
| `ts/mcp/browser/response.ts` | 聚合工具结果，生成代码、快照与图像，格式化 Markdown 内容并执行敏感信息脱敏。 |
| `ts/mcp/browser/tab.ts` | 表征单页，监听控制台/请求/对话框/下载事件，实现 `_snapshotForAI` 并维护模态状态。 |
| `ts/mcp/browser/tools/*` | 定义工具元数据、能力过滤与交互逻辑（快照、Tab 管理、Evaluate、拖拽、定位器生成等）。 |

> **能力特征摘要**：TypeScript 实现拥有成熟的快照体系（`_snapshotForAI` + `response.ts`）、多上下文/多标签管理、统一工具注册与等待重试模型，并通过注入脚本生成可执行的 Playwright 代码段。

### .NET 版当前状况

- `dotnet/PlaywrightTools.cs` 仍以单页模型为主，事件追踪与状态管理耦合在静态字段中。
- `dotnet/PlaywrightTools.Actions/*.cs` 多数尚未实现快照、交互与响应封装，保留 `NotImplementedException` 占位。
- `dotnet/mcp` 虽已搭建 Context/Tab/Tool 框架，但与真实 Playwright 对象、模态守卫及快照流程尚未完全接轨。
- `SnapshotBuilder` 等组件返回占位文本，缺乏结构化 DOM/ARIA 数据。

### 差异焦点与改进建议

| 改进方向 | TypeScript 实践 | .NET 缺口 | 建议措施 |
| --- | --- | --- | --- |
| 快照管理 | `tab.ts` 捕获 `_snapshotForAI`，`response.ts` 控制嵌入、脱敏与落盘。 | `BrowserSnapshotAsync`、`SnapshotBuilder` 未生成结构化数据。 | 构建 `SnapshotManager`，使用 `IPage.Accessibility.SnapshotAsync` 等 API 并统一写入响应。 |
| 标签页/上下文 | `context.ts`/`tab.ts` 管理多页签、模态状态及下载/追踪。 | 缺少真实 `IPage` 绑定与多标签控制。 | 引入 `TabManager` 维护页面集合、事件订阅与模态状态。 |
| 工具元数据 | `defineTool` 暴露 type/capability/schema，并按配置过滤。 | `ToolHelpers` 仅输出占位 Markdown。 | 设计 `ToolMetadata`/`ToolRegistry`，补全 schema 与能力过滤。 |
| 执行模型 | 使用 `progress.race()` 等等待/重试策略，统一在 `Response.finish()` 收尾。 | `ToolExecutionService` 缺少超时、重试与快照收尾。 | 增设 `ExecutionOrchestrator`，在执行后协调等待、清理与响应合并。 |
| 脚本注入 | `callOnPageNoTrace` 等封装无痕脚本执行与代码生成。 | Evaluate/交互工具尚未实现封装。 | 创建统一的脚本执行辅助方法，支持点击、拖拽、Evaluate 等操作。 |

### 融合实施路径

1. **新增核心组件**：`SnapshotManager`、`TabManager`、`ToolRegistry`、`ExecutionOrchestrator`，分别负责快照收集、多 Tab 管理、工具元数据与执行编排。
2. **重构 PlaywrightTools 与 Actions**：拆分为可注入服务、补齐工具实现、在重启流程中恢复 Tab 状态。
3. **对齐参考模块**：重点参考 TypeScript 的 `tab.ts`、`context.ts`、`tools/snapshot.ts`、`tools/tabs.ts`、`tools/evaluate.ts` 与 `response.ts` 的数据流与守卫逻辑。
4. **迭代步骤**：先落地 Snapshot/Tab 管理基础，逐步补齐工具 schema 与响应构建，最后扩展测试能力与追踪/截图等附加特性。

### 后续行动清单

- [ ] 设计核心管理组件接口并撰写实现草稿。
- [ ] 更新 `PlaywrightTools` 与关键 Actions，使其使用新服务并解除占位实现。
- [ ] 调整 `dotnet/mcp` 下 Context、Tab、Tool、Runtime 的对接逻辑。
- [ ] 基于能力过滤完成端到端测试，覆盖多 Tab 快照、模态状态与异步等待场景。

若需完整背景与论证，请查阅原始分析报告以获取更详尽的比较表、能力说明与实施建议。

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
