# Snapshot 能力对比：`tabs.ts` vs `TabManager.cs`

| 维度 | `tabs.ts` | `TabManager.cs` |
| --- | --- | --- |
| 触发快照时机 | 在 `close` 与 `select` 操作中，要求响应包含最新快照，确保调用方能在关闭或切换标签后获取页面状态。 | 在内部捕获与管理快照：调用 `CaptureSnapshotAsync` 生成结构化快照，并在切换、导航等流程中更新 `LastSnapshot` 元数据。 |
| 快照生成方式 | 不直接负责生成，只是通过 `response.setIncludeSnapshot()` 标记需要由上下文生成。 | 使用 `SnapshotManager` 捕获快照，或调用 Playwright 的 `SnapshotForAIAsync` 扩展；当捕获失败时提供带时间戳的兜底快照。 |
| 快照内容管理 | 无内建内容字段，依赖上游返回。 | 将 ARIA 标记、控制台消息、网络请求、模态与下载信息整合入 `SnapshotPayload`，并保存到 `LastSnapshot`。 |
| 快照重用 | 不具备缓存或复用能力。 | 若已有快照且包含 ARIA 信息，可复用 `LastSnapshot.AriaSnapshot` 避免重复捕获；还可生成恢复计划与引用校验。 |
| 快照关联操作 | 仅在执行标签操作时附带快照标志。 | 提供 `GetSnapshotMarkupAsync` 等方法用于定位器引用校验、模态/下载摘要生成，并在导航等待、操作完成等高级流程中依赖快照数据。 |

