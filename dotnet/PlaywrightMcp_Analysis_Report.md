# Playwright MCP 分析与改进方案报告

## 一、TypeScript 版本实现要点综述

| 路径 | 关键职责 |
| --- | --- |
| ts/mcp/browser/browserContextFactory.ts | 构建持久化、隔离、远程、CDP 或共享等多种浏览器上下文，同时串联追踪、初始化脚本与退出清理逻辑。 |
| ts/mcp/browser/context.ts | 统一管理上下文生命周期、标签页列表、模态状态、请求拦截、录制挂钩与产物（视频、追踪、下载）持久化。 |
| ts/mcp/browser/response.ts | 汇总工具结果、生成代码、快照与图像；在返回 MCP 内容前格式化 Markdown、输出标签页摘要并执行敏感信息脱敏。 |
| ts/mcp/browser/sessionLog.ts | 将工具调用、用户操作、代码与快照流水化写入 Markdown 会话日志，同时保存快照文件。 |
| ts/mcp/browser/tab.ts | 表征单个页面，订阅控制台、请求、对话框与下载事件；实现 `_snapshotForAI`，维护模态状态并提供引用定位与等待工具。 |
| ts/mcp/browser/tools/tool.ts | 统一的工具定义入口，绑定能力元数据与模态状态守卫，再委派至标签页级处理器。 |
| ts/mcp/browser/tools/utils.ts | 等待协调、定位器解析、无追踪执行与文件名格式化等公共工具逻辑。 |
| ts/mcp/browser/tools/snapshot.ts | 定义 `browser_snapshot` 及点击、拖拽、悬停、选择、定位器生成等交互工具，产出快照与可执行 Playwright 代码。 |
| ts/mcp/browser/tools/tabs.ts | 实现 core-tabs 能力，支持列出、新建、关闭与切换标签页并触发快照/标签页摘要更新。 |
| ts/mcp/browser/tools/evaluate.ts | 在授权引用后执行 `_evaluateFunction`，输出结果、快照与代码片段。 |

**能力特征摘要**

- 快照体系：`tab.ts` 通过 `_snapshotForAI` 汇聚 DOM/ARIA 树、可见文本、控制台、下载等信息，并由 `response.ts` 决定是否嵌入。
- 上下文/标签页管理：`context.ts` 负责上下文生命周期、模态状态寄存和事件绑定；`tabs.ts` 工具通过能力声明对外暴露多标签控制。
- 工具注册：`tools.ts` 汇集所有工具，依照配置过滤能力（core/testing/tracing/vision 等），工具元数据包含 type、capability、input schema。
- 执行模型：工具执行过程中使用 `progress.race()`、`retryWithProgressAndTimeouts()` 等等待/重试策略，确保操作完成与模态状态恢复。
- 注入脚本：交互工具通过 `injectedScript.evaluate()`、`callOnPageNoTrace()` 等封装实现无痕注入，并生成 Playwright 代码段。

## 二、.NET 版本现状概览

- `dotnet/PlaywrightTools.cs`：集中处理浏览器启动/重启/关闭，当前仅维持单一 `IPage` 实例，事件追踪与状态管理耦合在静态字段中。
- `dotnet/PlaywrightTools.Actions/*.cs`：多数工具文件仍为 `NotImplementedException` 占位，缺乏实际的快照、交互与返回值封装。
- `dotnet/mcp` 目录：已搭好类比 TS 的骨架（Context、Tab、Tool 定义、执行服务等），但仍是最小实现，缺乏 Playwright 对象绑定、模态状态守卫及快照整合。
- `SnapshotBuilder` 等运行时组件只返回占位字符串，未生成结构化 DOM/ARIA 数据，也未与响应聚合流程打通。

## 三、差异分析与改进建议

| 改进方向 | TS 版实践 | .NET 缺口 | 建议措施 |
| --- | --- | --- | --- |
| 快照管理 | `tab.ts` 捕获 `_snapshotForAI`，`response.ts` 控制快照嵌入、脱敏与文件落盘。 | `BrowserSnapshotAsync` 未实现，`SnapshotBuilder` 仅返回文本占位。 | 引入 `SnapshotManager`，基于 `IPage.Accessibility.SnapshotAsync` 等 API 生成结构化 JSON，并统一写入响应。 |
| 标签页/上下文 | `context.ts`/`tab.ts` 维护多页签、模态状态、下载/追踪产物。 | 仅存储 ID 字符串，缺乏真实 `IPage` 绑定与多标签控制。 | 新增 `TabManager`，在 `PlaywrightTools` 中维护页面集合；完善 `Context`/`Tab` 以追踪模态状态与事件。 |
| 工具元数据 | `defineTool` 提供 type、capability、schema，`tools.ts` 依据配置过滤。 | `ToolHelpers` 输出占位 Markdown，缺乏能力声明。 | 设计 `ToolMetadata` 结构，工具注册时填写 type/capability/schema，并与配置联动过滤。 |
| 执行模型 | 利用 `progress.race()`、`retryWithProgressAndTimeouts()` 等等待/重试包装工具执行，结束后调用 `Response.finish()`。 | `ToolExecutionService` 仅直接调用处理器，无超时、重试、快照收尾。 | 构建异步封装（等待网络静止、模态清理、可选重试），并将结果注入统一响应对象。 |
| 脚本注入 | `evaluate.ts` 等通过 `callOnPageNoTrace` 封装脚本执行，具备授权与代码生成。 | `PlaywrightTools.Actions.Evaluate.cs` 未实现，缺少统一注入封装。 | 在扩展方法中封装 `IPage.EvaluateAsync`/`ILocator` 操作，复用到点击、拖拽等交互工具。 |

## 四、融合设计方案

1. **新增核心组件**
   - `SnapshotManager`：负责收集 ARIA/DOM 树、截图路径、控制台/下载事件，并支持敏感信息脱敏；供所有工具调用。
   - `TabManager`：管理 `IPage` 实例集合、当前活动标签页、模态状态与事件订阅，支持多标签创建、选择与关闭。
   - `ToolRegistry`：维护工具元数据（名称、标题、type、capability、schema），提供按能力过滤的注册接口。
   - `ExecutionOrchestrator`：封装等待/重试策略、超时控制、进度回调，并在执行结束时协调快照/响应合并。

2. **PlaywrightTools 及 Actions 改造**
   - 将 `PlaywrightTools.cs` 拆分为可注入服务：浏览器上下文工厂、标签页控制、快照服务、工具执行服务。
   - 补全 `PlaywrightTools.Actions.*` 目录下工具实现，调用上述服务完成快照捕获、事件清理与响应生成。
   - `Relaunch` 流程需恢复 TabManager 状态，确保支持多标签重连。

3. **迁移/参考的 TS 模块**
   - `ts/mcp/browser/tab.ts` 与 `context.ts`：参考其模态状态、事件订阅、下载/追踪收集逻辑，映射到 .NET 的 TabManager/Context 中。
   - `ts/mcp/browser/tools/snapshot.ts`、`tabs.ts`、`evaluate.ts`：对照其工具输入 schema、权限校验与代码生成模式，完善 .NET 对应 Actions。
   - `ts/mcp/browser/response.ts`：借鉴响应聚合、快照附加与脱敏流程，在 .NET 端建立统一的响应构建器。
   - `ts/mcp/browser/tools/tool.ts`：对齐工具定义接口，确保 .NET ToolRegistry 支持能力声明与模态守卫。

4. **实施步骤建议**
   1. 优先实现 SnapshotManager 与基础 TabManager，解除核心工具（snapshot、tabs、navigate、evaluate）的 `NotImplementedException`。
   2. 引入 ToolMetadata/ToolRegistry，并逐步为工具补全输入 schema 与能力配置，建立与配置文件的联动过滤。
   3. 构建 ExecutionOrchestrator，将等待/重试逻辑整合进 `ToolExecutionService`，并在响应层添加 `ResponseBuilder`（可参考 TS `Response` 类）。
   4. 最后扩展测试工具链（verify/wait/testing capability），并接入追踪、截图、PDF 等扩展能力。

## 五、后续行动清单

- [ ] 设计 `SnapshotManager`、`TabManager`、`ToolRegistry`、`ExecutionOrchestrator` 的接口与实现草稿。
- [ ] 更新 `PlaywrightTools.cs` 与关键 Actions 文件以使用新服务，逐步补齐工具逻辑。
- [ ] 调整 `dotnet/mcp` 下的 Context、Tab、Tool、Runtime 模块，对接新的服务与响应输出。
- [ ] 结合配置与能力过滤开展端到端测试，验证多标签快照、模态状态、异步等待等场景。

