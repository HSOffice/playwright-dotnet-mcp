# Playwright MCP Server

## 概述
Playwright MCP Server 是一个将 Playwright 浏览器自动化能力以 Model Context Protocol（MCP）工具形式对外暴露的 .NET 8 库。它面向 `net8.0`，启用了可空引用类型，并依赖官方的 `Microsoft.Playwright` SDK。【F:PlaywrightMcpServer/PlaywrightMcpServer.csproj†L1-L9】
该服务器通过提供用于装饰工具宿主和工具方法的最小化特性存根，在不依赖上游 MCP SDK 的情况下构建。【F:PlaywrightMcpServer/ModelContextProtocolStubs.cs†L5-L43】所有工具实现都位于局部的 `PlaywrightTools` 类中，该类注册为 MCP 工具提供者，并预置描述核心浏览器操作的元数据。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L14-L44】【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L422-L422】

## 目录结构
- `Actions/` – `PlaywrightTools` 的局部定义，用于实现导航、输入、诊断和媒体捕获等具体 MCP 工具。例如，导航辅助工具封装了 `page.goto`/`goBack`，控制台工具暴露捕获到的控制台日志，网络工具汇总请求，截图辅助工具捕获页面或元素图像并将其附加到响应中。【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Navigate.cs†L13-L99】【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Console.cs†L12-L44】【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Network.cs†L13-L66】【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Screenshot.cs†L15-L118】
- `Models/` – 可序列化的记录类型，描述工具 API 中使用的模态对话框、控制台输出、网络流量、下载、快照以及标签页描述符。【F:PlaywrightMcpServer/Models/BrowserModels.cs†L8-L80】
- `Responses/` – 响应组合管线，包括序列化、附件处理、配置开关，以及从渲染好的 Markdown 段落中重建结构化数据的解析器。【F:PlaywrightMcpServer/Responses/Response.cs†L9-L200】【F:PlaywrightMcpServer/Responses/ResponseContent.cs†L5-L34】【F:PlaywrightMcpServer/Responses/ResponseConfiguration.cs†L6-L26】【F:PlaywrightMcpServer/Responses/ResponseParser.cs†L7-L88】
- `Security/` – 在响应返回前用占位符替换已配置密钥值的密钥编辑器。【F:PlaywrightMcpServer/Security/SecretRedactor.cs†L6-L42】
- `Snapshots/` – 捕获无障碍快照与最近活动的工具，并为模态状态、控制台输出、下载、网络流量和页面元数据生成 Markdown 段落。【F:PlaywrightMcpServer/Snapshots/SnapshotManager.cs†L8-L52】【F:PlaywrightMcpServer/Snapshots/SnapshotMarkdownBuilder.cs†L6-L84】【F:PlaywrightMcpServer/Snapshots/ModalStateMarkdownBuilder.cs†L5-L23】
- `Tabs/` – 浏览器标签页/页面的生命周期管理，包括事件订阅、活动缓冲和恢复元数据。【F:PlaywrightMcpServer/Tabs/TabManager.cs†L13-L167】【F:PlaywrightMcpServer/Tabs/TabManager.cs†L170-L399】
- `Tools/` – Playwright 进程管理、基于环境的配置、标签页激活、下载/输出路径和工具元数据注册等核心编排逻辑。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L17-L353】【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L339-L344】
- `ModelContextProtocolStubs.cs` – 本地 MCP Server SDK 属性的占位实现。【F:PlaywrightMcpServer/ModelContextProtocolStubs.cs†L5-L43】

## 工具编排
`PlaywrightTools` 集中处理浏览器启动、工具注册元数据以及供子局部类复用的辅助工具。它为当前的 Playwright 实例、浏览器、上下文、标签页管理器、快照管理器和响应配置保存单例。静态构造函数注册内置工具描述符，以确保各处理程序之间的元数据保持一致。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L17-L44】
环境变量可用于调整执行行为，例如 `MCP_PLAYWRIGHT_HEADLESS`、`MCP_PLAYWRIGHT_DOWNLOADS_DIR`、`MCP_PLAYWRIGHT_VIDEOS_DIR` 和 `MCP_PLAYWRIGHT_CHROMIUM_CHANNEL`；辅助方法则负责解析下载、视频、截图、PDF 与追踪文件的输出目录。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L46-L353】
该类同时负责规范化 URL、创建带时间戳的文件名，并向需要标签页元数据的客户端公开 `DescribeTabs`。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L323-L344】【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L334-L344】【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L402-L422】

`EnsureLaunchedAsync` 负责初始化 Playwright、启动配置的浏览器内核、创建带有下载和视频支持的上下文、授予默认权限，并在交由工具处理程序处理之前确保存在一个活动标签页。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L117-L171】配套的辅助函数可创建新的标签页、关闭上下文、选择正确的浏览器渠道，以及通过选择器或快照引用定位元素。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L174-L400】

## 浏览器与标签页生命周期
`TabManager` 跟踪所有打开的 Playwright 页面，保持活动标签页同步，并注册事件处理程序，缓冲控制台消息、网络流量、模态对话框、文件选择器活动和下载信息。它能够描述当前的标签页集合、收集控制台错误、准备恢复计划，并在标签页关闭后安全释放资源。【F:PlaywrightMcpServer/Tabs/TabManager.cs†L13-L167】
`TabState` 类型将 Playwright 事件连接到滚动缓冲区，支持等待加载状态和下载的导航辅助，并暴露帮助器，将依赖快照的 ARIA 引用还原为实时定位器。【F:PlaywrightMcpServer/Tabs/TabManager.cs†L170-L559】快照元数据会被写入 `TabState.UpdateMetadata`，确保后续工具可以找到之前的无障碍树或活动日志。【F:PlaywrightMcpServer/Tabs/TabManager.cs†L415-L555】

## 响应管线
每个工具都会调用 `ExecuteWithResponseAsync`，该方法为处理程序提供日志记录、错误处理和响应序列化封装。响应会收集结果文本、生成的 JavaScript 片段、标签页列表、图像以及可选的快照，然后序列化为结构化内容。默认情况下附件会包含图像，但可通过配置省略二进制内容或整个快照。所有输出都会经过 `SecretRedactor`，以将已知密钥替换为占位符。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L86-L115】【F:PlaywrightMcpServer/Responses/Response.cs†L22-L145】【F:PlaywrightMcpServer/Responses/ResponseContent.cs†L5-L34】【F:PlaywrightMcpServer/Responses/ResponseConfiguration.cs†L12-L25】【F:PlaywrightMcpServer/Security/SecretRedactor.cs†L6-L42】
对于需要从 Markdown 中还原结构化数据的客户端，`ResponseParser` 可以重建结果、代码、标签页、页面状态、控制台消息、模态状态和下载等段落。【F:PlaywrightMcpServer/Responses/ResponseParser.cs†L7-L88】

## 快照与诊断
当工具选择启用快照时，`SnapshotManager` 会捕获当前页面标题、URL、无障碍树、最新的控制台/网络活动、模态对话框以及下载状态，然后用得到的负载更新所属的 `TabState`。【F:PlaywrightMcpServer/Snapshots/SnapshotManager.cs†L10-L43】快照 Markdown 会组合模态指引、控制台摘要、下载、网络请求以及以 YAML 形式呈现的页面状态；专用构建器则会格式化模态条目，以突出可清除它们的工具。【F:PlaywrightMcpServer/Snapshots/SnapshotMarkdownBuilder.cs†L8-L67】【F:PlaywrightMcpServer/Snapshots/ModalStateMarkdownBuilder.cs†L5-L22】
`BrowserModels` 中的模型类型定义了这些诊断数据的 JSON 结构，以便调用方按需以编程方式消费。【F:PlaywrightMcpServer/Models/BrowserModels.cs†L8-L69】

## 工具目录
工具实现按关注点分布在多个局部类中。导航工具涵盖 URL 变更与历史遍历，并记录执行过的 JavaScript。【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Navigate.cs†L13-L99】诊断工具为活动标签页提供控制台输出和累积的网络流量。【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Console.cs†L12-L44】【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Network.cs†L13-L66】
媒体工具负责截取视口或元素截图、验证参数（格式、目标、全页规则），将文件保存到配置的截图目录，并内联缩略图以便快速查看。【F:PlaywrightMcpServer/Actions/PlaywrightTools.Actions.Screenshot.cs†L15-L118】
其他局部文件（键盘、鼠标、输入、表单、PDF、追踪、等待等）遵循相同模式：验证参数、通过共享助手调用 Playwright API、添加便于阅读的 Markdown 摘要、输出等效的 JavaScript，并按需附加媒体或快照。

## 扩展服务器
新增工具时，只需在 `Actions/` 下创建另一个局部的 `PlaywrightTools` 类，并用 `McpServerTool` 注解方法。浏览器启动编排、标签页解析、定位器查找、响应构建和元数据注册等共享辅助，使得新的处理程序可以专注于业务逻辑。通过 `RegisterTool` 添加新的工具描述符可保持元数据注册表一致，而本地 MCP 特性存根则使得无需额外依赖即可构建和测试。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L31-L44】【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L339-L344】【F:PlaywrightMcpServer/ModelContextProtocolStubs.cs†L5-L43】
