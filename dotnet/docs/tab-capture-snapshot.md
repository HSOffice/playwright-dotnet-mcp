# Translating `Tab.captureSnapshot()` to C#

The TypeScript runtime exposes a `Tab.captureSnapshot()` helper that wraps Playwright's private `_snapshotForAI()` API and packages the result together with tab metadata:

```ts
async captureSnapshot(): Promise<TabSnapshot> {
  let tabSnapshot: TabSnapshot | undefined;
  const modalStates = await this._raceAgainstModalStates(async () => {
    const snapshot = await (this.page as PageEx)._snapshotForAI();
    tabSnapshot = {
      url: this.page.url(),
      title: await this.page.title(),
      ariaSnapshot: snapshot,
      modalStates: [],
      consoleMessages: [],
      downloads: this._downloads,
    };
  });

  if (tabSnapshot) {
    tabSnapshot.consoleMessages = this._recentConsoleMessages;
    this._recentConsoleMessages = [];
  }

  return tabSnapshot ?? {
    url: this.page.url(),
    title: '',
    ariaSnapshot: '',
    modalStates,
    consoleMessages: [],
    downloads: [],
  };
}
```

In the C# implementation we model the same responsibility with `TabState` and `SnapshotManager`.  The `SnapshotManager` already performs the Playwright call, captures console/network activity, and updates the owning tab state.  The C# equivalent of `captureSnapshot()` therefore composes these building blocks instead of calling `_snapshotForAI()` directly:

```csharp
internal sealed class TabState
{
    internal async Task<SnapshotPayload> CaptureSnapshotAsync(SnapshotManager snapshotManager, CancellationToken cancellationToken)
    {
        SnapshotPayload? snapshot = null;
        var modalStates = await RaceAgainstModalStatesAsync(async ct =>
        {
            snapshot = await snapshotManager
                .CaptureAsync(this, ct)
                .ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        if (snapshot is not null)
        {
            return snapshot with
            {
                ModalStates = GetModalStatesSnapshot()
            };
        }

        var fallbackStates = modalStates.Count == 0
            ? GetModalStatesSnapshot()
            : modalStates;

        return new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = Page.Url ?? string.Empty,
            Title = string.Empty,
            AriaSnapshot = null,
            Console = Array.Empty<ConsoleMessageEntry>(),
            Network = Array.Empty<NetworkRequestEntry>(),
            ModalStates = fallbackStates
        };
    }
}
```

The helper uses the public `SnapshotManager.CaptureAsync` API【F:dotnet/SnapshotManager.cs†L11-L83】, which now delegates to Playwright's `AriaSnapshotAsync`/`_SnapshotForAIAsync` entry points to capture the YAML-formatted accessibility snapshot while persisting console/network metadata back onto the `TabState` instance.【F:dotnet/TabManager.cs†L207-L341】 The `RaceAgainstModalStatesAsync` helper mirrors the same modal handling that the TypeScript version performs before producing a fallback snapshot payload.
