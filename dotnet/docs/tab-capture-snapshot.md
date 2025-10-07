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
    private readonly SnapshotManager _snapshotManager;

    public async Task<SnapshotPayload> CaptureSnapshotAsync(CancellationToken cancellationToken = default)
    {
        SnapshotPayload? snapshot = null;
        var modalStates = await RaceAgainstModalStatesAsync(async () =>
        {
            snapshot = await _snapshotManager
                .CaptureAsync(this, cancellationToken)
                .ConfigureAwait(false);
        }).ConfigureAwait(false);

        if (snapshot is not null)
        {
            // console/network data is already populated by SnapshotManager
            return snapshot;
        }

        return new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = Page.Url ?? string.Empty,
            Title = string.Empty,
            Console = Array.Empty<ConsoleMessageEntry>(),
            Network = Array.Empty<NetworkRequestEntry>(),
            Aria = null,
        };
    }
}
```

The helper uses the public `SnapshotManager.CaptureAsync` API【F:dotnet/SnapshotManager.cs†L11-L45】, which in turn mirrors the TypeScript snapshot logic by reading the tab's Playwright page, serialising the accessibility tree, and persisting console/network metadata back onto the `TabState` instance.【F:dotnet/TabManager.cs†L243-L280】 The `RaceAgainstModalStatesAsync` placeholder represents the same modal handling that the TypeScript version performs before producing a fallback snapshot payload.
