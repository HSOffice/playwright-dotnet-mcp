# .NET Playwright MCP 服务代码框架规划

本文档给出在 `dotnet` 目录下实现 Playwright MCP 服务的推荐代码框架，用于指导后续开发。规划遵循 TypeScript 版本的模块划分，确保工具名称、能力与行为一一对应，同时聚焦于“仅提供 MCP 服务，供宿主复用”这一目标。

## 目录结构
```
dotnet/
├── ARCHITECTURE_PLAN.md        # 本规划文档
└── mcp/
    ├── PlaywrightMcpServer.csproj
    ├── Core/
    │   ├── BrowserServerBackend/
    │   │   ├── BrowserServerBackend.cs
    │   │   └── ToolInvocationContext.cs
    │   ├── Context/
    │   │   ├── BrowserContextFactory.cs
    │   │   ├── Context.cs
    │   │   ├── Tab.cs
    │   │   └── TabEvents.cs
    │   ├── Protocol/
    │   │   ├── McpContracts.cs
    │   │   ├── ToolDefinition.cs
    │   │   ├── ToolSchema.cs
    │   │   └── ResponseSerializer.cs
    │   ├── Runtime/
    │   │   ├── Response.cs
    │   │   ├── ResponseBlocks.cs
    │   │   ├── SecretRedactor.cs
    │   │   └── SnapshotBuilder.cs
    │   ├── Services/
    │   │   ├── ToolRegistry.cs
    │   │   ├── ToolExecutionService.cs
    │   │   └── SessionLog.cs
    │   └── Utils/
    │       ├── LocatorParser.cs
    │       ├── PlaywrightExtensions.cs
    │       ├── SerializationHelpers.cs
    │       └── TimeProvider.cs
    ├── Server/
    │   ├── McpServer.cs
    │   ├── McpServerBuilder.cs
    │   ├── IMcpTransport.cs
    │   ├── StdIoTransport.cs
    │   └── HeartbeatService.cs
    ├── Tools/
    │   ├── ToolHelpers.cs
    │   ├── BrowserTools.cs
    │   ├── CommonTools.cs
    │   ├── ConsoleTools.cs
    │   ├── DialogsTools.cs
    │   ├── EvaluateTools.cs
    │   ├── FilesTools.cs
    │   ├── FormTools.cs
    │   ├── InstallTools.cs
    │   ├── KeyboardTools.cs
    │   ├── MouseTools.cs
    │   ├── NavigateTools.cs
    │   ├── NetworkTools.cs
    │   ├── PdfTools.cs
    │   ├── ScreenshotTools.cs
    │   ├── SnapshotTools.cs
    │   ├── TabsTools.cs
    │   ├── TracingTools.cs
    │   ├── VerifyTools.cs
    │   └── WaitTools.cs
    └── Configuration/
        ├── ConfigModels.cs
        ├── ConfigLoader.cs
        └── CapabilityFilters.cs
```

> **说明**：以上为推荐分层，可根据实际进度灵活调整。关键原则是：
> 1. **Core** 包含协议、上下文和运行时基础设施；
> 2. **Server** 暴露 MCP 服务接口，供外部宿主注入传输；
> 3. **Tools** 按 TypeScript 工具文件一一对应；
> 4. **Configuration** 负责配置解析与能力过滤。

## 模块职责

### 1. Configuration
- 解析 JSON/YAML 配置文件、环境变量与 CLI 参数（如需）。
- 定义 `FullConfig` / `BrowserConfig` / `SecretsConfig` 等模型，保持与 TS 结构一致。
- 在 `CapabilityFilters` 中实现对工具集合的裁剪：基于 `capabilities`、`allowFileSystem`、`tracing` 等开关过滤工具。

### 2. Core
#### 2.1 Protocol
- `ToolDefinition`：定义工具名称、描述、参数 Schema、返回类型。
- `ToolSchema`：把 Zod Schema 对应到 `System.Text.Json` / `JsonSchema` 表达。
- `McpContracts`：包含 `ListToolsResponse`、`CallToolRequest`/`Response` 等协议实体。
- `ResponseSerializer`：将 `Runtime.Response` 序列化为协议响应结构。

#### 2.2 BrowserServerBackend
- `BrowserServerBackend`：核心协调者。接收工具调用请求，拉取上下文、执行工具、记录日志、构造响应。
- `ToolInvocationContext`：封装单次调用的状态，如当前 Tab、配置、计时器、取消令牌等。

#### 2.3 Context
- `BrowserContextFactory`：创建/复用 Playwright 浏览器上下文，支持连接到现有浏览器实例。
- `Context`：维护当前上下文、Tab 列表、输出目录、模态状态。
- `Tab`：封装单个页面的事件订阅、快照缓存、下载保存。
- `TabEvents`：将控制台、网络、对话框等事件转换为 MCP 需要的记录。

#### 2.4 Runtime
- `Response`：聚合工具执行结果、代码块、图片、Tab 摘要、快照文本。
- `ResponseBlocks`：类型化的响应片段定义（Markdown、图像、YAML 等）。
- `SecretRedactor`：按配置对日志与响应进行脱敏。
- `SnapshotBuilder`：生成与 TS 相同格式的页面快照文本。

#### 2.5 Services
- `ToolRegistry`：从 `Tools` 模块收集 `IToolDefinition`，并按能力过滤。
- `ToolExecutionService`：统一处理执行流程、超时控制、异常映射。
- `SessionLog`：可选的调用日志记录器，记录工具调用历史、输入/输出。

#### 2.6 Utils
- 放置通用帮助方法，如定位器解析、Playwright 扩展、时间/文件命名工具等。

### 3. Tools
- 每个文件对应 TypeScript 中的同名工具集合，导出实现 `IToolDefinition`。
- `ToolHelpers` 提供定位器构造、等待包装、文件保存、错误格式化等共用逻辑。
- `BrowserTools` 负责聚合所有工具并对外暴露只读集合。

### 4. Server
- `McpServer`：实现 `list_tools`、`call_tool`、`ping` 等核心方法，只依赖 `BrowserServerBackend`。
- `McpServerBuilder`：用于配置 `BrowserServerBackend`、注入传输层与配置对象。
- `IMcpTransport`：抽象不同传输方式（StdIO、WebSocket、自定义宿主调用）。
- `StdIoTransport`（可选示例）：如需通过命令行运行，可使用标准输入输出传输。
- `HeartbeatService`：向宿主发送心跳，保持连接与健康状态（如需要）。

## 与现有 .NET 代码的衔接
- 参考 `PlaywrightTools.cs` 与 `PlaywrightTools.Actions.Relaunch.cs` 中的逻辑，提取可复用的 Playwright 辅助方法，迁移到 `Core/Utils` 或 `Tools`。
- 删除当前 `Class1.cs`，改为在 `Server` 模块导出公开的 `McpServer` 构建入口。

## 开发建议
1. **先搭建骨架**：创建项目文件夹、接口定义、空类，确保编译通过。
2. **实现协议/配置**：先完成 `Configuration`、`Protocol`、`Server` 基础设施，可实现 `list_tools` 空实现用于集成测试。
3. **逐步迁移工具**：按优先级移植工具，实现过程中同步完善 `Context`/`Tab` 功能。
4. **保持与 TS 同步**：对照 `ts/mcp` 目录实现相同的调用流程、响应格式与错误处理。
5. **文档与示例**：后续在 `docs/` 或 `README` 中补充示例，指导宿主如何引用该库并提供自定义传输。

## 下一步
- 根据本文档创建对应的空类与接口。
- 迁移 TypeScript 版核心逻辑与工具实现。
- 提供简单的宿主集成示例（如单元测试或控制台 demo）。
