# ts 目录结构与文件说明

## 顶层

## `ts/mcp`
- `ts/mcp/DEPS.list`：声明 `program.ts` 与 `index.ts` 对 SDK、浏览器和扩展模块的依赖关系。
- `ts/mcp/config.d.ts`：定义 Playwright MCP 的配置类型与工具能力枚举，描述浏览器、服务器、网络、超时及输出等选项。
- `ts/mcp/index.ts`：导出创建 MCP 服务器的工厂方法，解析用户配置并包装 `BrowserServerBackend`；提供可注入自定义上下文的工厂。
- `ts/mcp/log.ts`：封装 debug 日志通道并提供 `logUnhandledError` 辅助方法。
- `ts/mcp/program.ts`：给 Playwright CLI 添加 MCP 相关命令行参数，解析配置后根据 `--extension` 与 `--connect-tool` 等选项启动不同的服务器后端。

### `ts/mcp/browser`
- `ts/mcp/browser/DEPS.list`：声明浏览器模块依赖工具集合、SDK 与日志等内部模块。
- `ts/mcp/browser/actions.d.ts`：类型声明 MCP 录制动作和信号（点击、输入、导航、断言等）及上下文封装。
- `ts/mcp/browser/browserContextFactory.ts`：根据配置创建或共享浏览器上下文，支持持久化、隔离、远程、CDP、共享模式，并处理跟踪、用户数据目录与 init scripts。
- `ts/mcp/browser/browserServerBackend.ts`：MCP 服务器后端实现，负责初始化浏览器上下文、注册工具、调用工具并写入会话日志。
- `ts/mcp/browser/codegen.ts`：提供字符串转义和对象格式化工具，辅助生成可执行的 Playwright 代码片段。
- `ts/mcp/browser/config.ts`：解析 CLI/环境/文件配置，合并默认值，校验设置，处理输出目录、网络过滤、超时与追踪文件等逻辑。
- `ts/mcp/browser/context.ts`：封装浏览器上下文生命周期、标签页管理、模态状态追踪、请求拦截、截图/视频/追踪收集及密钥查找逻辑。
- `ts/mcp/browser/response.ts`：累积工具执行结果、代码、图片与快照，负责格式化 Markdown 响应、附件和密钥脱敏，并支持反向解析。
- `ts/mcp/browser/sessionLog.ts`：若开启会话记录，增量写入工具调用、用户操作与快照，并生成可回放的会话 Markdown。
- `ts/mcp/browser/tab.ts`：代表单个页面，管理控制台消息、下载、模态状态、录制快照、请求追踪及通用交互（导航、等待、ref 定位）。
- `ts/mcp/browser/tools.ts`：组合浏览器工具子集，并根据能力过滤暴露给客户端的工具列表。
- `ts/mcp/browser/watchdog.ts`：注册退出钩子以在进程结束时清理上下文和共享浏览器。

#### `ts/mcp/browser/tools`
- `ts/mcp/browser/tools/DEPS.list`：声明工具依赖浏览器与 SDK 基础设施。
- `ts/mcp/browser/tools/tool.ts`：定义工具/标签页工具的通用接口与包装逻辑（含模态状态检查）。
- `ts/mcp/browser/tools/utils.ts`：提供等待网络完成、locator 生成、文件名格式化等辅助函数。
- `ts/mcp/browser/tools/common.ts`：实现关闭浏览器和调整窗口大小的基础工具。
- `ts/mcp/browser/tools/console.ts`：读取当前标签页的控制台消息。
- `ts/mcp/browser/tools/dialogs.ts`：处理浏览器对话框并清理相应模态状态。
- `ts/mcp/browser/tools/evaluate.ts`：在页面或定位的元素上执行自定义 JavaScript，并返回结果。
- `ts/mcp/browser/tools/files.ts`：处理文件选择器上传，支持传入文件路径或取消。
- `ts/mcp/browser/tools/form.ts`：按 ref 定位批量填充表单字段（文本、复选、下拉、滑块）。
- `ts/mcp/browser/tools/install.ts`：运行 `playwright install` 安装配置指定的浏览器。
- `ts/mcp/browser/tools/keyboard.ts`：提供键盘按键和逐字输入工具，可选择慢速和提交操作。
- `ts/mcp/browser/tools/mouse.ts`：基于坐标执行移动、单击和拖拽操作（vision 能力）。
- `ts/mcp/browser/tools/navigate.ts`：新建或切换标签并导航、返回上一页。
- `ts/mcp/browser/tools/network.ts`：列出自页面加载以来捕获的网络请求。
- `ts/mcp/browser/tools/pdf.ts`：将页面保存为 PDF 文件并输出路径。
- `ts/mcp/browser/tools/screenshot.ts`：截取页面或元素截图，控制格式、范围及附件返回。
- `ts/mcp/browser/tools/snapshot.ts`：捕获可访问性树快照，并提供点击、拖拽、悬停、选项选择、locator 生成等动作工具。
- `ts/mcp/browser/tools/tabs.ts`：列出/新建/关闭/切换浏览器标签页。
- `ts/mcp/browser/tools/tracing.ts`：开始与停止 Playwright 追踪，并返回追踪文件位置说明。
- `ts/mcp/browser/tools/wait.ts`：根据时间或文本出现/消失进行等待，并附带快照。
- `ts/mcp/browser/tools/verify.ts`：提供多种断言工具（元素可见、文本可见、列表内容、输入值）。

### `ts/mcp/extension`
- `ts/mcp/extension/DEPS.list`：标注扩展桥接层依赖 SDK HTTP 工具等。
- `ts/mcp/extension/cdpRelay.ts`：实现 WebSocket 中继，将 MCP 与浏览器扩展之间的 CDP 流量桥接，并管理连接、会话和命令转发。
- `ts/mcp/extension/extensionContextFactory.ts`：通过扩展桥接启动或复用浏览器，确保 MCP 上下文能与扩展通信。
- `ts/mcp/extension/protocol.ts`：定义扩展桥接的命令、事件及协议版本。

### `ts/mcp/sdk`
- `ts/mcp/sdk/DEPS.list`：指向外部打包实现 `mcpBundleImpl`。
- `ts/mcp/sdk/bundle.ts`：从 `mcpBundleImpl` re-export MCP SDK、zod 与 JSON schema 转换等依赖。
- `ts/mcp/sdk/exports.ts`：聚合导出 SDK 内部模块（传输、服务器、工具、HTTP、MDB）。
- `ts/mcp/sdk/http.ts`：创建/装饰 HTTP 服务器、安装 SSE 与流式传输，实现会话管理与 DNS 保护。
- `ts/mcp/sdk/inProcessTransport.ts`：提供进程内传输层，用于直接把客户端连接到服务器实现。
- `ts/mcp/sdk/mdb.ts`：实现 Microsoft Developer Box 集成的后端代理，支持工具堆栈推送、进度中断及 HTTP/STDIO 启动。
- `ts/mcp/sdk/proxyBackend.ts`：包装多个 MCP 提供者，动态切换并暴露统一工具列表。
- `ts/mcp/sdk/server.ts`：构建 MCP 服务器实例，处理工具注册、初始化、心跳、HTTP/STDIO 启动及文本内容合并。
- `ts/mcp/sdk/tool.ts`：定义工具 schema，提供转换为 MCP 工具及可选 `intent` 字段扩展。

### `ts/mcp/test`
- `ts/mcp/test/DEPS.list`：列出测试后端依赖（浏览器工具、报告器、运行器等）。
- `ts/mcp/test/browserBackend.ts`：在测试结束或失败时挂起 MCP 浏览器后端，输出页面状态和快照供人工处理。
- `ts/mcp/test/generatorTools.ts`：生成器工具集，支持页面预置、读取执行日志及将生成的测试代码写入文件。
- `ts/mcp/test/plannerTools.ts`：规划工具，运行种子测试以准备页面供规划流程使用。
- `ts/mcp/test/seed.ts`：查找或生成种子测试文件，确保存在基础测试入口。
- `ts/mcp/test/streams.ts`：把 `Writable` 输出转换为 MCP 进度通知。
- `ts/mcp/test/testBackend.ts`：为测试生成/调试场景实现 MCP 后端，注册规划、生成及测试运行工具并与浏览器工具联动。
- `ts/mcp/test/testContext.ts`：维护测试运行环境、配置定位、种子测试执行、生成日志与进度输出。
- `ts/mcp/test/testTool.ts`：定义测试工具接口及工厂函数。
- `ts/mcp/test/testTools.ts`：实现列出测试、执行测试、单测调试等工具。

## 其他文件
- `ts/mcp/index.ts`、`ts/mcp/program.ts`、`ts/mcp/log.ts` 已在上方分类描述。
