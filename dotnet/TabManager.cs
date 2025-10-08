using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

internal sealed class TabManager
{
    private readonly object _gate = new();
    private readonly Dictionary<IPage, TabState> _tabs = new(new ReferenceEqualityComparer<IPage>());
    private TabState? _activeTab;
    private int _sequence;

    public TabState? ActiveTab
    {
        get
        {
            lock (_gate)
            {
                return _activeTab;
            }
        }
    }

    public IReadOnlyList<TabState> Tabs
    {
        get
        {
            lock (_gate)
            {
                return _tabs.Values.OrderBy(t => t.CreatedAt).ToArray();
            }
        }
    }

    public TabState Register(IPage page, bool makeActive = true)
    {
        ArgumentNullException.ThrowIfNull(page);

        TabState tab;
        bool attachHandlers = false;
        lock (_gate)
        {
            if (!_tabs.TryGetValue(page, out tab!))
            {
                tab = new TabState(page, $"tab-{++_sequence}", DateTimeOffset.UtcNow, Remove);
                _tabs.Add(page, tab);
                attachHandlers = true;
            }

            if (makeActive || _activeTab is null)
            {
                _activeTab = tab;
            }
        }

        if (attachHandlers)
        {
            tab.AttachHandlers();
        }

        return tab;
    }

    public void Activate(IPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        lock (_gate)
        {
            if (_tabs.TryGetValue(page, out var tab))
            {
                _activeTab = tab;
            }
        }
    }

    public void Activate(TabState tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        lock (_gate)
        {
            if (_tabs.ContainsKey(tab.Page))
            {
                _activeTab = tab;
            }
        }
    }

    public IReadOnlyList<TabDescriptor> DescribeTabs()
    {
        lock (_gate)
        {
            return _tabs.Values
                .OrderBy(t => t.CreatedAt)
                .Select(t => t.ToDescriptor(ReferenceEquals(t, _activeTab)))
                .ToArray();
        }
    }

    public IReadOnlyList<TabRestoreEntry> CreateRestorePlan()
    {
        lock (_gate)
        {
            return _tabs.Values
                .OrderBy(t => t.CreatedAt)
                .Select(t => new TabRestoreEntry(t.Url))
                .ToArray();
        }
    }

    public void Remove(TabState tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        lock (_gate)
        {
            if (_tabs.Remove(tab.Page))
            {
                tab.Dispose();
                if (ReferenceEquals(tab, _activeTab))
                {
                    _activeTab = _tabs.Values.OrderBy(t => t.CreatedAt).FirstOrDefault();
                }
            }
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            foreach (var tab in _tabs.Values)
            {
                tab.Dispose();
            }

            _tabs.Clear();
            _activeTab = null;
            _sequence = 0;
        }
    }
}

internal sealed class TabState : IDisposable
{
    private readonly object _gate = new();
    private readonly List<ConsoleMessageEntry> _consoleMessages = new();
    private readonly List<NetworkRequestEntry> _networkRequests = new();
    private readonly Dictionary<IRequest, NetworkRequestEntry> _requestMap = new(new ReferenceEqualityComparer<IRequest>());
    private readonly List<ModalStateEntry> _modalStates = new();
    private readonly List<DownloadEntry> _downloads = new();
    private readonly List<TaskCompletionSource<IReadOnlyList<ModalStateEntry>>> _modalStateWaiters = new();
    private readonly List<TaskCompletionSource<bool>> _downloadWaiters = new();
    private readonly Action<TabState> _onClosed;

    private readonly EventHandler<IConsoleMessage> _consoleHandler;
    private readonly EventHandler<IRequest> _requestHandler;
    private readonly EventHandler<IResponse> _responseHandler;
    private readonly EventHandler<IRequest> _requestFailedHandler;
    private readonly EventHandler<IPage> _closeHandler;
    private readonly EventHandler<IDialog> _dialogHandler;
    private readonly EventHandler<IFileChooser> _fileChooserHandler;
    private readonly EventHandler<IDownload> _downloadHandler;

    public TabState(IPage page, string id, DateTimeOffset createdAt, Action<TabState> onClosed)
    {
        Page = page ?? throw new ArgumentNullException(nameof(page));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        CreatedAt = createdAt;
        _onClosed = onClosed ?? throw new ArgumentNullException(nameof(onClosed));

        _consoleHandler = (_, message) =>
        {
            var entry = new ConsoleMessageEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Type = message.Type,
                Text = message.Text,
                Args = message.Args.Select(arg => arg.ToString()).Where(arg => arg is not null).Cast<string>().ToArray()
            };

            lock (_gate)
            {
                _consoleMessages.Add(entry);
                if (_consoleMessages.Count > 200)
                {
                    _consoleMessages.RemoveAt(0);
                }
            }
        };

        _requestHandler = (_, request) =>
        {
            var entry = new NetworkRequestEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Method = request.Method,
                Url = request.Url,
                ResourceType = request.ResourceType
            };

            lock (_gate)
            {
                _networkRequests.Add(entry);
                _requestMap[request] = entry;
                if (_networkRequests.Count > 500)
                {
                    var oldest = _networkRequests.First();
                    _networkRequests.RemoveAt(0);
                    var toRemove = _requestMap.FirstOrDefault(kvp => kvp.Value == oldest).Key;
                    if (toRemove is not null)
                    {
                        _requestMap.Remove(toRemove);
                    }
                }
            }
        };

        _responseHandler = (_, response) =>
        {
            lock (_gate)
            {
                if (_requestMap.TryGetValue(response.Request, out var entry))
                {
                    entry.Status = response.Status;
                }
            }
        };

        _requestFailedHandler = (_, request) =>
        {
            lock (_gate)
            {
                if (_requestMap.TryGetValue(request, out var entry))
                {
                    entry.Failure = request.Failure;
                }
            }
        };

        _closeHandler = (_, _) => _onClosed(this);
        _dialogHandler = (_, dialog) => OnDialog(dialog);
        _fileChooserHandler = (_, chooser) => OnFileChooser(chooser);
        _downloadHandler = (_, download) =>
        {
            lock (_gate)
            {
                foreach (var waiter in _downloadWaiters.ToArray())
                {
                    waiter.TrySetResult(true);
                }

                _downloadWaiters.Clear();
            }

            _ = HandleDownloadAsync(download);
        };
    }

    public string Id { get; }
    public IPage Page { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? Title { get; private set; }
    public string? Url { get; private set; }
    public SnapshotPayload? LastSnapshot { get; private set; }

    public void AttachHandlers()
    {
        Page.Console += _consoleHandler;
        Page.Request += _requestHandler;
        Page.Response += _responseHandler;
        Page.RequestFailed += _requestFailedHandler;
        Page.Close += _closeHandler;
        Page.Dialog += _dialogHandler;
        Page.FileChooser += _fileChooserHandler;
        Page.Download += _downloadHandler;
        Url = Page.Url;
    }

    public void Dispose()
    {
        Page.Console -= _consoleHandler;
        Page.Request -= _requestHandler;
        Page.Response -= _responseHandler;
        Page.RequestFailed -= _requestFailedHandler;
        Page.Close -= _closeHandler;
        Page.Dialog -= _dialogHandler;
        Page.FileChooser -= _fileChooserHandler;
        Page.Download -= _downloadHandler;
        CancelDownloadWaiters();
    }

    internal async Task<IResponse?> NavigateAsync(string url, PageGotoOptions? options, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        options ??= new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        };

        ClearActivityQueues();

        using var downloadCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var downloadSignalTask = WaitForDownloadSignalAsync(downloadCancellation.Token);

        try
        {
            var response = await Page.GotoAsync(url, options).ConfigureAwait(false);
            downloadCancellation.Cancel();

            await WaitForLoadStateWithTimeoutAsync().ConfigureAwait(false);
            return response;
        }
        catch (PlaywrightException ex) when (IsDownloadNavigationException(ex))
        {
            var completedTask = await Task.WhenAny(downloadSignalTask, Task.Delay(TimeSpan.FromSeconds(3), cancellationToken)).ConfigureAwait(false);
            if (ReferenceEquals(completedTask, downloadSignalTask) && await downloadSignalTask.ConfigureAwait(false))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                return null;
            }

            throw;
        }
        finally
        {
            downloadCancellation.Cancel();
            try
            {
                await downloadSignalTask.ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    public (IReadOnlyList<ConsoleMessageEntry> console, IReadOnlyList<NetworkRequestEntry> network) TakeActivitySnapshot()
    {
        lock (_gate)
        {
            return (
                _consoleMessages.ToArray(),
                _networkRequests.Select(entry => entry.Clone()).ToArray());
        }
    }

    public void UpdateMetadata(string? url, string? title, SnapshotPayload snapshot)
    {
        lock (_gate)
        {
            Url = url;
            Title = title;
            LastSnapshot = snapshot;
        }
    }

    public async Task WaitForCompletionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        await RaceAgainstModalStatesAsync(ct => WaitForCompletionCoreAsync(action, ct), cancellationToken).ConfigureAwait(false);
    }

    private void ClearActivityQueues()
    {
        lock (_gate)
        {
            _consoleMessages.Clear();
            _networkRequests.Clear();
            _requestMap.Clear();
        }
    }

    internal IReadOnlyList<ModalStateEntry> GetModalStatesSnapshot()
    {
        lock (_gate)
        {
            return _modalStates.Count == 0
                ? Array.Empty<ModalStateEntry>()
                : _modalStates.ToArray();
        }
    }

    internal IReadOnlyList<DownloadEntry> GetDownloadsSnapshot()
    {
        lock (_gate)
        {
            if (_downloads.Count == 0)
            {
                return Array.Empty<DownloadEntry>();
            }

            return _downloads.Select(entry => entry.Clone()).ToArray();
        }
    }

    internal async Task<SnapshotPayload> CaptureSnapshotAsync(SnapshotManager snapshotManager, CancellationToken cancellationToken)
    {
        if (snapshotManager is null)
        {
            throw new ArgumentNullException(nameof(snapshotManager));
        }

        SnapshotPayload? snapshot = null;
        var modalStates = await RaceAgainstModalStatesAsync(async ct =>
        {
            snapshot = await snapshotManager.CaptureAsync(this, ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        if (snapshot is not null)
        {
            return snapshot with
            {
                ModalStates = GetModalStatesSnapshot(),
                Downloads = GetDownloadsSnapshot()
            };
        }

        var fallbackStates = modalStates.Count == 0
            ? GetModalStatesSnapshot()
            : modalStates;
        var downloads = GetDownloadsSnapshot();

        return new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = Page.Url ?? string.Empty,
            Title = string.Empty,
            Aria = null,
            Console = Array.Empty<ConsoleMessageEntry>(),
            Network = Array.Empty<NetworkRequestEntry>(),
            ModalStates = fallbackStates.Count == 0 ? Array.Empty<ModalStateEntry>() : fallbackStates,
            Downloads = downloads.Count == 0 ? Array.Empty<DownloadEntry>() : downloads
        };
    }

    public TabDescriptor ToDescriptor(bool isActive)
    {
        lock (_gate)
        {
            return new TabDescriptor
            {
                Id = Id,
                Url = Url,
                Title = Title,
                IsActive = isActive,
                CreatedAt = CreatedAt
            };
        }
    }

    private Task HandleDownloadAsync(IDownload download)
    {
        ArgumentNullException.ThrowIfNull(download);

        var suggestedFileName = download.SuggestedFilename ?? $"download-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        var outputPath = PlaywrightTools.ResolveDownloadOutputPath(suggestedFileName);
        var entry = new DownloadEntry
        {
            SuggestedFileName = suggestedFileName,
            OutputPath = outputPath,
            Finished = false
        };

        lock (_gate)
        {
            _downloads.Add(entry);
        }

        return SaveAsync();

        async Task SaveAsync()
        {
            try
            {
                await download.SaveAsAsync(outputPath).ConfigureAwait(false);
                lock (_gate)
                {
                    entry.Finished = true;
                }
            }
            catch
            {
                // Keep the entry to reflect the failed download attempt.
            }
        }
    }

    private async Task<bool> WaitForDownloadSignalAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationTokenRegistration registration = default;

        lock (_gate)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                tcs.TrySetResult(false);
                return false;
            }

            _downloadWaiters.Add(tcs);
            registration = cancellationToken.Register(static state =>
            {
                var self = (TabState)state!;
                self.CancelDownloadWaiters();
            }, this);
        }

        try
        {
            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            registration.Dispose();
        }
    }

    private void CancelDownloadWaiters()
    {
        lock (_gate)
        {
            if (_downloadWaiters.Count == 0)
            {
                return;
            }

            foreach (var waiter in _downloadWaiters)
            {
                waiter.TrySetResult(false);
            }

            _downloadWaiters.Clear();
        }
    }

    private async Task WaitForLoadStateWithTimeoutAsync()
    {
        try
        {
            await Page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions
            {
                Timeout = 5000
            }).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
        }
        catch (PlaywrightException ex) when (IsTimeoutException(ex))
        {
        }
    }

    private static bool IsDownloadNavigationException(PlaywrightException exception)
    {
        var message = exception.Message ?? string.Empty;
        return message.Contains("net::ERR_ABORTED", StringComparison.Ordinal)
               || message.Contains("Download is starting", StringComparison.Ordinal);
    }

    private static bool IsTimeoutException(PlaywrightException exception)
        => (exception.Message ?? string.Empty).Contains("Timeout", StringComparison.OrdinalIgnoreCase);

    private async Task WaitForCompletionCoreAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var outstandingRequests = new HashSet<IRequest>(new ReferenceEqualityComparer<IRequest>());
        var requestsGate = new object();
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var disposeGate = 0;
        var frameNavigated = false;

        void DisposeHandlers()
        {
            if (Interlocked.Exchange(ref disposeGate, 1) != 0)
            {
                return;
            }

            Page.Request -= requestHandler;
            Page.Response -= responseHandler;
            Page.RequestFailed -= requestFailedHandler;
            Page.FrameNavigated -= frameNavigatedHandler;
        }

        void TrySignalCompletion()
        {
            if (frameNavigated)
            {
                return;
            }

            lock (requestsGate)
            {
                if (outstandingRequests.Count == 0)
                {
                    completionSource.TrySetResult();
                }
            }
        }

        void OnRequestSettled(IRequest request)
        {
            if (request is null)
            {
                return;
            }

            lock (requestsGate)
            {
                if (!outstandingRequests.Remove(request) || frameNavigated || outstandingRequests.Count > 0)
                {
                    return;
                }
            }

            completionSource.TrySetResult();
        }

        async Task ObserveRequestAsync(IRequest request)
        {
            try
            {
                _ = await request.ResponseAsync().ConfigureAwait(false);
            }
            catch
            {
            }
            finally
            {
                OnRequestSettled(request);
            }
        }

        void OnFrameNavigated(IFrame frame)
        {
            if (frame is null || frame.ParentFrame is not null)
            {
                return;
            }

            frameNavigated = true;
            DisposeHandlers();
            timeoutRegistration?.Dispose();

            _ = WaitForLoadStateWithTimeoutAsync().ContinueWith(static (task, state) =>
            {
                var source = (TaskCompletionSource)state!;
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch
                {
                }
                finally
                {
                    source.TrySetResult();
                }
            }, completionSource, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        EventHandler<IRequest>? requestHandler = null;
        EventHandler<IResponse>? responseHandler = null;
        EventHandler<IRequest>? requestFailedHandler = null;
        EventHandler<IFrame>? frameNavigatedHandler = null;
        IDisposable? timeoutRegistration = null;

        requestHandler = (_, request) =>
        {
            if (request is null)
            {
                return;
            }

            lock (requestsGate)
            {
                outstandingRequests.Add(request);
            }

            _ = ObserveRequestAsync(request);
        };

        responseHandler = (_, response) =>
        {
            if (response?.Request is { } request)
            {
                OnRequestSettled(request);
            }
        };

        requestFailedHandler = (_, request) => OnRequestSettled(request);
        frameNavigatedHandler = (_, frame) => OnFrameNavigated(frame);

        Page.Request += requestHandler;
        Page.Response += responseHandler;
        Page.RequestFailed += requestFailedHandler;
        Page.FrameNavigated += frameNavigatedHandler;

        timeoutRegistration = StartWaitForCompletionTimeout(() =>
        {
            DisposeHandlers();
            completionSource.TrySetResult();
        });

        try
        {
            await action(cancellationToken).ConfigureAwait(false);
            TrySignalCompletion();
            await WaitForCompletionBarrierAsync(completionSource.Task, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            await WaitForPostActionDelayAsync().ConfigureAwait(false);
        }
        finally
        {
            timeoutRegistration?.Dispose();
            DisposeHandlers();
        }
    }

    private async Task WaitForPostActionDelayAsync()
    {
        try
        {
            await Page.WaitForTimeoutAsync(1000).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
        }
        catch (PlaywrightException ex) when (IsTimeoutException(ex))
        {
        }
    }

    private static async Task WaitForCompletionBarrierAsync(Task barrier, CancellationToken cancellationToken)
    {
        if (barrier.IsCompleted)
        {
            await barrier.ConfigureAwait(false);
            return;
        }

        var cancellationCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(static state =>
        {
            var (source, token) = ((TaskCompletionSource<bool> source, CancellationToken token))state!;
            source.TrySetCanceled(token);
        }, (cancellationCompletion, cancellationToken)))
        {
            var completed = await Task.WhenAny(barrier, cancellationCompletion.Task).ConfigureAwait(false);
            if (!ReferenceEquals(completed, barrier))
            {
                await completed.ConfigureAwait(false);
            }

            await barrier.ConfigureAwait(false);
        }
    }

    private static IDisposable StartWaitForCompletionTimeout(Action callback)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return new Timer(static state =>
        {
            var action = (Action)state!;
            action();
        }, callback, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
    }

    private async Task<IReadOnlyList<ModalStateEntry>> RaceAgainstModalStatesAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<IReadOnlyList<ModalStateEntry>>? waiter;
        lock (_gate)
        {
            if (_modalStates.Count > 0)
            {
                return _modalStates.ToArray();
            }

            waiter = new TaskCompletionSource<IReadOnlyList<ModalStateEntry>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _modalStateWaiters.Add(waiter);
        }

        using var registration = cancellationToken.Register(state =>
        {
            var source = (TaskCompletionSource<IReadOnlyList<ModalStateEntry>>)state!;
            source.TrySetCanceled(cancellationToken);
        }, waiter);

        try
        {
            var operationTask = action(cancellationToken);
            var completedTask = await Task.WhenAny(operationTask, waiter!.Task).ConfigureAwait(false);
            if (ReferenceEquals(completedTask, operationTask))
            {
                await operationTask.ConfigureAwait(false);
                return Array.Empty<ModalStateEntry>();
            }

            return await waiter.Task.ConfigureAwait(false);
        }
        finally
        {
            lock (_gate)
            {
                _modalStateWaiters.Remove(waiter!);
            }
        }
    }

    private void OnDialog(IDialog dialog)
    {
        if (dialog is null)
        {
            return;
        }

        var entry = new ModalStateEntry
        {
            Type = "dialog",
            Description = $"\"{dialog.Type ?? string.Empty}\" dialog with message \"{dialog.Message ?? string.Empty}\"",
            ClearedBy = "browser_handle_dialog",
            Dialog = dialog
        };

        PushModalState(entry);
        ObserveModalResolution(dialog, entry);
    }

    private void OnFileChooser(IFileChooser chooser)
    {
        if (chooser is null)
        {
            return;
        }

        var entry = new ModalStateEntry
        {
            Type = "fileChooser",
            Description = "File chooser",
            ClearedBy = "browser_file_upload",
            FileChooser = chooser
        };

        PushModalState(entry);
        ObserveModalResolution(chooser, entry);
    }

    private void PushModalState(ModalStateEntry entry)
    {
        TaskCompletionSource<IReadOnlyList<ModalStateEntry>>[] waiters;
        IReadOnlyList<ModalStateEntry> snapshot;
        lock (_gate)
        {
            _modalStates.Add(entry);
            snapshot = _modalStates.ToArray();
            waiters = _modalStateWaiters.ToArray();
            _modalStateWaiters.Clear();
        }

        foreach (var waiter in waiters)
        {
            waiter.TrySetResult(snapshot);
        }
    }

    private void ClearModalState(ModalStateEntry entry)
    {
        lock (_gate)
        {
            _modalStates.Remove(entry);
        }
    }

    private void ObserveModalResolution(object modal, ModalStateEntry entry)
    {
        if (modal is null)
        {
            return;
        }

        try
        {
            var waitForClose = modal.GetType().GetMethod("WaitForCloseAsync", Type.EmptyTypes);
            if (waitForClose?.Invoke(modal, Array.Empty<object>()) is Task task)
            {
                _ = task.ContinueWith(
                    static (_, state) =>
                    {
                        var (tab, modalEntry) = ((TabState tab, ModalStateEntry entry))state!;
                        tab.ClearModalState(modalEntry);
                    },
                    (this, entry),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
        catch
        {
            // Swallow reflection failures; modal state can still be cleared manually by tools.
        }
    }
}

internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}
