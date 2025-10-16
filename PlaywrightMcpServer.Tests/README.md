# PlaywrightMcpServer.Tests

## 概览
PlaywrightMcpServer.Tests 项目针对 Playwright MCP 服务器端核心组件提供单元与集成测试，目标框架为 .NET 8，同时依赖 xUnit、Moq 与 Microsoft.Playwright 以便构造浏览器交互场景并进行行为验证。项目直接引用 PlaywrightMcpServer 主项目，使测试能够访问真实的选项卡管理、响应序列化等实现。【F:PlaywrightMcpServer.Tests/PlaywrightMcpServer.Tests.csproj†L1-L17】

## 测试目录结构与职责
- `NavigateIntegrationTests.cs`：模拟浏览器下载流程，验证 `TabState.NavigateAsync` 能够在下载触发时清理网络请求并生成下载快照，同时确保 `Response` 在序列化结果中包含下载信息，从而覆盖 Tab 管理、快照捕获与响应拼装的协同行为。【F:PlaywrightMcpServer.Tests/NavigateIntegrationTests.cs†L13-L77】
- `ResponseTests.cs`：覆盖 `Response` 的核心功能，包括 Finish 阶段的 Tab 标题刷新、结果/代码段落的 Markdown 输出、机密信息脱敏与图片附件配置，确保响应上下文与配置对象正确驱动输出内容。【F:PlaywrightMcpServer.Tests/ResponseTests.cs†L14-L145】
- `ResponseSerializationTests.cs`：验证 `ResponseJsonSerializer` 在多态内容集合下正确写出文本与图片特定字段，保证 JSON 输出兼容 MCP 协议消费者。【F:PlaywrightMcpServer.Tests/ResponseSerializationTests.cs†L9-L25】
- `TabManagerTests.cs`：测试 `TabManager` 的基本生命周期操作，包括注册、按页面查找与控制台消息聚合，确认选项卡管理器在引用相等比较器下仍能返回正确实例。【F:PlaywrightMcpServer.Tests/TabManagerTests.cs†L10-L73】
- `TabStateTests.cs`：针对 `TabState` 的高频功能编写大量场景测试，涵盖按 ref 标记检索 `Locator`、快照元数据更新、控制台/网络活动缓存、导航时清空历史、加载状态等待、对话框阻塞下的超时处理以及模态状态 Markdown 输出，确保单个选项卡状态对象在多线程访问下维持一致性与可观测性。【F:PlaywrightMcpServer.Tests/TabStateTests.cs†L14-L396】
- `TabStateWaitForCompletionTests.cs`：验证 `TabState.WaitForCompletionAsync` 对未完成请求、模态对话与动作后延迟的处理逻辑，确保工具调用可等待浏览器活动自然结束。【F:PlaywrightMcpServer.Tests/TabStateWaitForCompletionTests.cs†L12-L102】
- `SnapshotMarkdownBuilderTests.cs`：针对快照 Markdown 构建器输出的下载段落进行断言，保证快照描述符合用户可读格式。【F:PlaywrightMcpServer.Tests/SnapshotMarkdownBuilderTests.cs†L9-L46】
- `PlaywrightTools.TestStubs.cs`：覆写 `PlaywrightTools.ResolveDownloadOutputPath`，在测试环境内避免真实的下载目录依赖并复用主项目的部分类设计。【F:PlaywrightMcpServer.Tests/PlaywrightTools.TestStubs.cs†L5-L9】

## 与 PlaywrightMcpServer 主项目的关系
- **项目引用**：测试项目通过 `ProjectReference` 直接依赖 PlaywrightMcpServer，因此所有测试均针对生产代码进行验证，而非模拟实现。【F:PlaywrightMcpServer.Tests/PlaywrightMcpServer.Tests.csproj†L14-L16】
- **选项卡管理与状态**：测试覆盖的 `TabManager` 与 `TabState` 对象来自主项目，用于维护浏览器页面集合、事件处理器以及快照数据（含下载、网络、模态状态等），与主服务器的浏览器会话管理逻辑保持一致。【F:PlaywrightMcpServer/Tabs/TabManager.cs†L13-L176】【F:PlaywrightMcpServer/Tabs/TabManager.cs†L323-L520】【F:PlaywrightMcpServer/Tabs/TabManager.cs†L760-L834】
- **快照与 Markdown**：`SnapshotManager` 负责从真实页面提取标题、URL、ARIA 树及近期活动，`SnapshotMarkdownBuilder` 则将快照转为多段 Markdown，测试验证两者输出能够被响应对象消费，反映 MCP 返回 payload 的核心组成。【F:PlaywrightMcpServer/Snapshots/SnapshotManager.cs†L8-L44】【F:PlaywrightMcpServer/Snapshots/SnapshotMarkdownBuilder.cs†L6-L67】
- **响应生成管线**：`Response`、`ResponseContext` 与 `ResponseConfiguration` 组成的响应管线负责收集执行结果、控制 Tab 元数据刷新、决定是否包含图片/快照并执行秘密脱敏；测试项目直接调用这些类型以保证工具响应与 TypeScript 版本保持功能一致。【F:PlaywrightMcpServer/Responses/Response.cs†L9-L188】【F:PlaywrightMcpServer/Responses/ResponseContext.cs†L8-L29】【F:PlaywrightMcpServer/Responses/ResponseConfiguration.cs†L6-L32】
- **下载路径解析**：生产代码中的 `PlaywrightTools.ResolveDownloadOutputPath` 根据服务器默认目录生成下载文件路径，测试通过部分类 stub 将其重定向至当前工作目录，既保持行为一致又避免依赖真实的工具目录结构。【F:PlaywrightMcpServer/Tools/PlaywrightTools.cs†L306-L315】【F:PlaywrightMcpServer/Tabs/TabManager.cs†L799-L834】【F:PlaywrightMcpServer.Tests/PlaywrightTools.TestStubs.cs†L5-L9】

## 运行测试
在仓库根目录执行以下命令即可运行全部测试用例：

```bash
dotnet test
```

该命令会构建主项目与测试项目，并使用 xUnit 运行上述测试类。【F:PlaywrightMcpServer.Tests/PlaywrightMcpServer.Tests.csproj†L1-L17】
