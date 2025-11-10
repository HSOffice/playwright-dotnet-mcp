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
| `PlaywrightMcpServer/` | Playwright MCP 核心实现：`PlaywrightTools` 部分类、响应序列化、Tab 管理与快照构建。|
| `PlaywrightMcpServer.Tests/` | xUnit 测试套件，覆盖响应模型、快照 Markdown 构建、Tab 状态等逻辑。|
| `PlaywrightRemoteBrowserLauncher/` | Windows WinForms 浏览器启动器：封装远程调试端口监听、Playwright 连接、日志/截图/下载管线。|
| `WebView2BrowserHost/` | 基于 WebView2 的极简浏览器宿主，用作远程调试示例目标与 Launcher 的默认测试浏览器。|
| `DetectBrowserProcess/` | Windows 辅助工具，枚举进程并检测是否嵌入 Electron/CEF/WebView2 等浏览器运行时。|
| `docs/` | 设计记录与迁移规划，含快照能力说明、TypeScript 版本对照等。|
| `PlaywrightDotnetMcp.sln` | 主解决方案文件，聚合服务器、测试、桌面工具与示例宿主等多个项目。|

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

- `PlaywrightMcpServer/PlaywrightTools.cs` 仍以单页模型为主，事件追踪与状态管理耦合在静态字段中。
- `PlaywrightMcpServer/PlaywrightTools.Actions/*.cs` 多数尚未实现快照、交互与响应封装，保留 `NotImplementedException` 占位。
- `PlaywrightMcpServer` 虽已搭建类比 TS 的 Context/Tab/Tool 框架，但与真实 Playwright 对象、模态守卫及快照流程尚未完全接轨。
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
- [ ] 调整 `PlaywrightMcpServer` 下 Context、Tab、Tool、Runtime 的对接逻辑。
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

### 工具清单 | Tool Catalog

| 能力分类 | 文件名 | Tool 名称 | 功能简介 | 读写性质 | title | description | inputSchema |
| --- | --- | --- | --- | --- | --- | --- | --- |
| core / 核心操作 | `common.ts` | `browser_close` | 关闭浏览器页面 | 读 | Close browser | Close the page | *(no parameters)* |
| core / 核心操作 | `common.ts` | `browser_resize` | 调整浏览器窗口大小 | 读 | Resize browser window | Resize the browser window | width: Width of the browser window<br>height: Height of the browser window |
| core / 核心操作 | `navigate.ts` | `browser_navigate` | 跳转到指定 URL | 写 | Navigate to a URL | Navigate to a URL | url: The URL to navigate to |
| core / 核心操作 | `navigate.ts` | `browser_navigate_back` | 返回上一页 | 读 | Go back | Go back to the previous page | *(no parameters)* |
| core / 核心操作 | `files.ts` | `browser_file_upload` | 上传文件 | 写 | Upload files | Upload one or multiple files | paths (optional): The absolute paths to the files to upload. Can be single file or multiple files. If omitted, file chooser is cancelled. |
| core / 核心操作 | `dialogs.ts` | `browser_handle_dialog` | 处理对话框 | 写 | Handle a dialog | Handle a dialog | accept: Whether to accept the dialog.<br>promptText (optional): The text of the prompt in case of a prompt dialog. |
| core / 核心操作 | `evaluate.ts` | `browser_evaluate` | 在页面或元素上执行 JS 代码 | 写 | Evaluate JavaScript | Evaluate JavaScript expression on page or element | function: () => { /* code */ } or (element) => { /* code */ } when element is provided<br>element (optional): Human-readable element description used to obtain permission to interact with the element<br>ref (optional): Exact target element reference from the page snapshot |
| core / 核心操作 | `wait.ts` | `browser_wait_for` | 等待文本出现/消失或等待时间 | 断言 | Wait for | Wait for text to appear or disappear or a specified time to pass | time (optional): The time to wait in seconds<br>text (optional): The text to wait for<br>textGone (optional): The text to wait for to disappear |
| core / 核心操作 | `form.ts` | `browser_fill_form` | 填写多个表单字段 | 写 | Fill form | Fill multiple form fields | fields: Fields to fill in (name, type, ref, value) |
| core / 核心操作 | `keyboard.ts` | `browser_press_key` | 模拟键盘按键 | 写 | Press a key | Press a key on the keyboard | key: Name of the key to press or a character to generate, such as `ArrowLeft` or `a` |
| core / 核心操作 | `keyboard.ts` | `browser_type` | 在元素中输入文本 | 写 | Type text | Type text into editable element | element: Human-readable element description used to obtain permission to interact with the element<br>ref: Exact target element reference from the page snapshot<br>text: Text to type into the element<br>submit (optional): Whether to submit entered text (press Enter after)<br>slowly (optional): Whether to type one character at a time. By default entire text is filled in at once. |
| core / 核心操作 | `snapshot.ts` | `browser_click` | 点击元素（可双击/改键/带修饰键） | 写 | Click | Perform click on a web page | element: Human-readable element description used to obtain permission to interact with the element<br>ref: Exact target element reference from the page snapshot<br>doubleClick (optional): Whether to perform a double click instead of a single click<br>button (optional): Button to click, defaults to left<br>modifiers (optional): Modifier keys to press |
| core / 核心操作 | `snapshot.ts` | `browser_drag` | 在两个元素间拖拽 | 写 | Drag mouse | Perform drag and drop between two elements | startElement: Human-readable source element description used to obtain the permission to interact with the element<br>startRef: Exact source element reference from the page snapshot<br>endElement: Human-readable target element description used to obtain the permission to interact with the element<br>endRef: Exact target element reference from the page snapshot |
| core / 核心操作 | `snapshot.ts` | `browser_hover` | 悬停元素 | 读 | Hover mouse | Hover over element on page | element: Human-readable element description used to obtain permission to interact with the element<br>ref: Exact target element reference from the page snapshot |
| core / 核心操作 | `snapshot.ts` | `browser_select_option` | 在下拉框中选择选项 | 写 | Select option | Select an option in a dropdown | element: Human-readable element description used to obtain permission to interact with the element<br>ref: Exact target element reference from the page snapshot<br>values: Array of values to select in the dropdown. This can be a single value or multiple values. |
| core / 核心操作 | `snapshot.ts` | `browser_snapshot` | 页面无障碍快照 | 读 | Page snapshot | Capture accessibility snapshot of the current page, this is better than screenshot | *(no parameters)* |
| core / 核心操作 | `snapshot.ts` | `browser_generate_locator` | 生成测试定位器 | 读 | Create locator for element | Generate locator for the given element to use in tests | element: Human-readable element description used to obtain permission to interact with the element<br>ref: Exact target element reference from the page snapshot |
| core / 核心操作 | `network.ts` | `browser_network_requests` | 列出网络请求 | 读 | List network requests | Returns all network requests since loading the page | *(no parameters)* |
| core / 核心操作 | `console.ts` | `browser_console_messages` | 获取控制台消息 | 读 | Get console messages | Returns all console messages | onlyErrors (optional): Only return error messages |
| core / 核心操作 | `screenshot.ts` | `browser_take_screenshot` | 截图 | 读 | Take a screenshot | Take a screenshot of the current page | type: Image format for the screenshot. Default is png.<br>filename (optional): File name to save the screenshot to. Defaults to `page-{timestamp}.{png|jpeg}` if not specified.<br>element (optional): Human-readable element description used to obtain permission to screenshot the element.<br>ref (optional): Exact target element reference from the page snapshot.<br>fullPage (optional): When true, takes a screenshot of the full scrollable page, instead of the currently visible viewport. Cannot be used with element screenshots. |
| vision / 坐标操作 | `mouse.ts` | `browser_mouse_move_xy` | 移动鼠标到指定坐标 | 读 | Move mouse | Move mouse to a given position | element: Human-readable element description used to obtain permission to interact with the element<br>x: X coordinate<br>y: Y coordinate |
| vision / 坐标操作 | `mouse.ts` | `browser_mouse_click_xy` | 在指定坐标点击 | 写 | Click | Click left mouse button at a given position | element: Human-readable element description used to obtain permission to interact with the element<br>x: X coordinate<br>y: Y coordinate |
| vision / 坐标操作 | `mouse.ts` | `browser_mouse_drag_xy` | 在坐标间拖拽 | 写 | Drag mouse | Drag left mouse button to a given position | element: Human-readable element description used to obtain permission to interact with the element<br>startX: Start X coordinate<br>startY: Start Y coordinate<br>endX: End X coordinate<br>endY: End Y coordinate |
| testing / 测试断言 | `verify.ts` | `browser_verify_element_visible` | 验证元素（角色+名称）可见 | 断言 | Verify element visible | Verify element is visible on the page | role: ROLE of the element<br>accessibleName: ACCESSIBLE_NAME of the element |
| testing / 测试断言 | `verify.ts` | `browser_verify_text_visible` | 验证文本可见 | 断言 | Verify text visible | Verify text is visible on the page | text: TEXT to verify |
| testing / 测试断言 | `verify.ts` | `browser_verify_list_visible` | 验证列表及其条目 | 断言 | Verify list visible | Verify list is visible on the page | element: Human-readable list description<br>ref: Exact target element reference that points to the list<br>items: Items to verify |
| testing / 测试断言 | `verify.ts` | `browser_verify_value` | 验证元素值/状态 | 断言 | Verify value | Verify element value | type: Type of the element (`textbox`/`checkbox`/`radio`/`combobox`/`slider`)<br>element: Human-readable element description<br>ref: Exact target element reference that points to the element<br>value: Value to verify. For checkbox, use "true" or "false" |
| core-tabs / 标签页管理 | `tabs.ts` | `browser_tabs` | 管理标签页（列出/新建/关闭/切换） | 写 | Manage tabs | List, create, close, or select a browser tab. | action: Operation to perform<br>index (optional): Tab index, used for close/select. If omitted for close, current tab is closed. |
| pdf / PDF 导出 | `pdf.ts` | `browser_pdf_save` | 保存页面为 PDF | 读 | Save as PDF | Save page as PDF | filename (optional): File name to save the pdf to. Defaults to `page-{timestamp}.pdf` if not specified. |
| tracing / 追踪管理 | `tracing.ts` | `browser_start_tracing` | 开始性能追踪 | 读 | Start tracing | Start trace recording | *(no parameters)* |
| tracing / 追踪管理 | `tracing.ts` | `browser_stop_tracing` | 停止性能追踪 | 读 | Stop tracing | Stop trace recording | *(no parameters)* |
| core-install / 浏览器安装 | `install.ts` | `browser_install` | 安装配置的浏览器 | 写 | Install the browser specified in the config | Install the browser specified in the config. Call this if you get an error about the browser not being installed. | *(no parameters)* |

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
   dotnet test PlaywrightMcpServer.Tests
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
