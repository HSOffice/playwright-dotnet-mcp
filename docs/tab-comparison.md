# Tab API Gap Analysis

This document tracks functions that exist on the TypeScript `Tab` class but do not yet have direct equivalents in the C# `TabState` implementation.

| TypeScript member | Location | Status in `TabState` | Notes |
| --- | --- | --- | --- |
| `static collectConsoleMessages(page)` | `tab.ts` lines 93-102 | **Missing** | The .NET version attaches console handlers in `AttachHandlers`, but there is no helper to prefetch existing console and page error messages when a tab is registered. |
| `_initialize()` | `tab.ts` lines 104-110 | **Missing** | Relies on `collectConsoleMessages` to seed console logs and outstanding requests before new events arrive; `TabState.AttachHandlers` does not perform a similar bootstrap. |
| `isCurrentTab()` | `tab.ts` lines 175-177 | **Missing** | `TabManager` exposes `ActiveTab`, yet `TabState` offers no convenience method to compare itself against the active tab. |
| `clearModalState(modalState)` | `tab.ts` lines 121-123 | **Not exposed** | `TabState` has a private `ClearModalState` used internally, but there is no public/internal API to clear a modal state from tooling, whereas the TypeScript tab allows tools to call `clearModalState`. |
| `consoleMessages(type?)` awaiting `_initializedPromise` | `tab.ts` lines 212-215 | **Partial** | `TabState.GetConsoleMessages` returns buffered entries, but because the bootstrap step is missing, any messages emitted before handler attachment are unavailable. |
| `requests()` awaiting `_initializedPromise` | `tab.ts` lines 217-220 | **Partial** | `TabState` records `NetworkRequestEntry` instances going forward; there is no method to retrieve the outstanding `IRequest` objects that existed before handler registration. |

These gaps explain subtle behavioral differences that surfaced while comparing the two implementations.
