using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

    public TabState? ForPage(IPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        lock (_gate)
        {
            return _tabs.TryGetValue(page, out var tab) ? tab : null;
        }
    }

    public IReadOnlyList<ConsoleMessageEntry> CollectConsoleMessages(IPage page, bool onlyErrors = false)
    {
        var tab = ForPage(page);
        if (tab is null)
        {
            return Array.Empty<ConsoleMessageEntry>();
        }

        return tab.GetConsoleMessages(onlyErrors);
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
    private readonly List<ConsoleMessageEntry> _recentConsoleMessages = new();
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
                _recentConsoleMessages.Add(entry);
                if (_consoleMessages.Count > 200)
                {
                    _consoleMessages.RemoveAt(0);
                }

                if (_recentConsoleMessages.Count > 200)
                {
                    _recentConsoleMessages.RemoveAt(0);
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

    internal readonly record struct RefLocatorRequest(string ElementDescription, string Reference);

    internal async Task UpdateTitleAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? title = null;
        var modalStates = await RaceAgainstModalStatesAsync(async _ =>
        {
            title = await Page.TitleAsync().ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        if (modalStates.Count > 0 || title is null)
        {
            return;
        }

        lock (_gate)
        {
            Title = title;
        }
    }

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

            await WaitForLoadStateAsync(
                options: new PageWaitForLoadStateOptions
                {
                    Timeout = 5000
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);
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

    public async Task WaitForLoadStateAsync(
        LoadState state = LoadState.Load,
        PageWaitForLoadStateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (state != LoadState.Load)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "Only LoadState.Load is supported.");
        }

        await RaceAgainstModalStatesAsync(
            ct => WaitForLoadStateCoreAsync(state, options, ct),
            cancellationToken).ConfigureAwait(false);
    }

    public (IReadOnlyList<ConsoleMessageEntry> console, IReadOnlyList<NetworkRequestEntry> network) TakeActivitySnapshot()
    {
        lock (_gate)
        {
            IReadOnlyList<ConsoleMessageEntry> consoleSnapshot;
            if (_recentConsoleMessages.Count == 0)
            {
                consoleSnapshot = Array.Empty<ConsoleMessageEntry>();
            }
            else
            {
                consoleSnapshot = _recentConsoleMessages.ToArray();
                _recentConsoleMessages.Clear();
            }

            return (
                consoleSnapshot,
                CloneNetworkRequestsUnsafe());
        }
    }

    public IReadOnlyList<ConsoleMessageEntry> GetConsoleMessages(bool onlyErrors)
    {
        lock (_gate)
        {
            if (_consoleMessages.Count == 0)
            {
                return Array.Empty<ConsoleMessageEntry>();
            }

            if (!onlyErrors)
            {
                return _consoleMessages.ToArray();
            }

            return _consoleMessages
                .Where(static entry => string.Equals(entry.Type, "error", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public IReadOnlyList<NetworkRequestEntry> GetNetworkRequests()
    {
        lock (_gate)
        {
            if (_networkRequests.Count == 0)
            {
                return Array.Empty<NetworkRequestEntry>();
            }

            return CloneNetworkRequestsUnsafe();
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

    internal async Task<ILocator> GetLocatorByRefAsync(string elementDescription, string reference, CancellationToken cancellationToken)
    {
        var requests = new[] { new RefLocatorRequest(elementDescription, reference) };
        var locators = await GetLocatorsByRefCoreAsync(requests, cancellationToken).ConfigureAwait(false);
        return locators[0];
    }

    internal async Task<ILocator> GetLocatorByRefAsync(RefLocatorRequest request, CancellationToken cancellationToken)
    {
        var requests = new[] { request };
        var locators = await GetLocatorsByRefCoreAsync(requests, cancellationToken).ConfigureAwait(false);
        return locators[0];
    }

    internal Task<IReadOnlyList<ILocator>> GetLocatorsByRefAsync(IReadOnlyList<RefLocatorRequest> requests, CancellationToken cancellationToken)
        => GetLocatorsByRefCoreAsync(requests, cancellationToken);

    private async Task<IReadOnlyList<ILocator>> GetLocatorsByRefCoreAsync(IReadOnlyList<RefLocatorRequest> requests, CancellationToken cancellationToken)
    {
        if (requests is null)
        {
            throw new ArgumentNullException(nameof(requests));
        }

        if (requests.Count == 0)
        {
            return Array.Empty<ILocator>();
        }

        cancellationToken.ThrowIfCancellationRequested();

        var snapshotMarkup = await GetSnapshotMarkupAsync(cancellationToken).ConfigureAwait(false);

        var missing = requests
            .Where(request => string.IsNullOrWhiteSpace(request.Reference) || !SnapshotContainsReference(snapshotMarkup, request.Reference))
            .Select(request => request.Reference)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (missing.Length > 0)
        {
            var prefix = missing.Length == 1 ? "Ref" : "Refs";
            var formatted = string.Join(", ", missing.Select(reference => $"'{reference}'"));
            throw new InvalidOperationException($"{prefix} {formatted} not found in the current page snapshot. Capture a new snapshot and try again.");
        }

        var result = new ILocator[requests.Count];
        for (var i = 0; i < requests.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = requests[i];
            var locator = Page.Locator($"aria-ref={request.Reference}");
            result[i] = DescribeLocator(locator, request.ElementDescription);
        }

        return result;
    }

    private async Task<string> GetSnapshotMarkupAsync(CancellationToken cancellationToken)
    {
        var page = Page;
        var markup = await TryInvokeSnapshotForAiAsync(page, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(markup))
        {
            return markup;
        }

        if (!string.IsNullOrEmpty(LastSnapshot?.AriaSnapshot))
        {
            return LastSnapshot.AriaSnapshot!;
        }

        var snapshotManager = new SnapshotManager();
        var snapshot = await snapshotManager.CaptureAsync(this, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(snapshot.AriaSnapshot))
        {
            return snapshot.AriaSnapshot!;
        }

        return string.Empty;
    }

    private static async Task<string?> TryInvokeSnapshotForAiAsync(IPage page, CancellationToken cancellationToken)
    {
        var pageType = page.GetType();
        var methods = new[]
        {
            pageType.GetMethod("_SnapshotForAIAsync", Type.EmptyTypes),
            pageType.GetMethod("SnapshotForAIAsync", Type.EmptyTypes),
            pageType.GetMethod("_SnapshotForAIAsync", new[] { typeof(CancellationToken) }),
            pageType.GetMethod("SnapshotForAIAsync", new[] { typeof(CancellationToken) })
        };

        foreach (var method in methods)
        {
            if (method is null)
            {
                continue;
            }

            try
            {
                var parameters = method.GetParameters().Length == 0
                    ? Array.Empty<object?>()
                    : new object?[] { cancellationToken };

                var invokeResult = method.Invoke(page, parameters);
                if (invokeResult is Task<string> stringTask)
                {
                    return await stringTask.ConfigureAwait(false);
                }

                if (invokeResult is Task task)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                    if (resultProperty?.GetValue(task) is string text)
                    {
                        return text;
                    }
                }
            }
            catch
            {
                // Ignore and fall back to snapshot manager / cached snapshot.
            }
        }

        return null;
    }

    private static bool SnapshotContainsReference(string snapshotMarkup, string reference)
    {
        if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(snapshotMarkup))
        {
            return false;
        }

        var token = $"[ref={reference}]";
        return snapshotMarkup.Contains(token, StringComparison.Ordinal);
    }

    private static ILocator DescribeLocator(ILocator locator, string elementDescription)
    {
        if (locator is null)
        {
            throw new ArgumentNullException(nameof(locator));
        }

        if (string.IsNullOrWhiteSpace(elementDescription))
        {
            return locator;
        }

        try
        {
            var method = locator.GetType().GetMethod("Describe", new[] { typeof(string) });
            if (method is not null)
            {
                var described = method.Invoke(locator, new object?[] { elementDescription }) as ILocator;
                if (described is not null)
                {
                    return described;
                }
            }
        }
        catch
        {
        }

        return locator;
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
            _recentConsoleMessages.Clear();
            _networkRequests.Clear();
            _requestMap.Clear();
        }
    }

    public IReadOnlyList<string> GetModalStatesMarkdown()
    {
        return ModalStateMarkdownBuilder.Build(GetModalStatesSnapshot());
    }

    private NetworkRequestEntry[] CloneNetworkRequestsUnsafe()
    {
        if (_networkRequests.Count == 0)
        {
            return Array.Empty<NetworkRequestEntry>();
        }

        var result = new NetworkRequestEntry[_networkRequests.Count];
        for (var i = 0; i < _networkRequests.Count; i++)
        {
            result[i] = _networkRequests[i].Clone();
        }

        return result;
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
            AriaSnapshot = null,
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

    private async Task WaitForLoadStateCoreAsync(LoadState state, PageWaitForLoadStateOptions? options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var waitTask = Page.WaitForLoadStateAsync(state, options);
        if (!cancellationToken.CanBeCanceled)
        {
            await ObserveLoadStateTaskAsync(waitTask, CancellationToken.None).ConfigureAwait(false);
            return;
        }

        var cancellationSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellationToken.Register(static state =>
        {
            var source = (TaskCompletionSource<bool>)state!;
            source.TrySetResult(true);
        }, cancellationSignal);

        var completedTask = await Task.WhenAny(waitTask, cancellationSignal.Task).ConfigureAwait(false);
        if (ReferenceEquals(completedTask, waitTask))
        {
            await ObserveLoadStateTaskAsync(waitTask, cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static async Task ObserveLoadStateTaskAsync(Task waitTask, CancellationToken cancellationToken)
    {
        try
        {
            await waitTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException)
        {
        }
        catch (PlaywrightException ex) when (IsTimeoutException(ex))
        {
        }
        catch (PlaywrightException ex)
        {
            Debug.WriteLine(ex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
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

    public async Task WaitForTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        if (timeout == TimeSpan.Zero)
        {
            return;
        }

        if (IsJavaScriptBlocked())
        {
            await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
            return;
        }

        await RaceAgainstModalStatesAsync(async ct =>
        {
            ct.ThrowIfCancellationRequested();
            var milliseconds = Math.Max(0, timeout.TotalMilliseconds);
            await Page.EvaluateAsync<object>(
                "(ms) => new Promise(resolve => setTimeout(resolve, ms))",
                milliseconds).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    private bool IsJavaScriptBlocked()
    {
        lock (_gate)
        {
            return _modalStates.Any(state =>
                string.Equals(state.Type, "dialog", StringComparison.OrdinalIgnoreCase));
        }
    }

    private async Task WaitForCompletionCoreAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var outstandingRequests = new HashSet<IRequest>(new ReferenceEqualityComparer<IRequest>());
        var requestsGate = new object();
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var disposeGate = 0;
        var frameNavigated = false;

        EventHandler<IRequest>? requestHandler = null;
        EventHandler<IResponse>? responseHandler = null;
        EventHandler<IRequest>? requestFailedHandler = null;
        EventHandler<IFrame>? frameNavigatedHandler = null;
        IDisposable? timeoutRegistration = null;

        void DisposeHandlers()
        {
            if (Interlocked.Exchange(ref disposeGate, 1) != 0)
            {
                return;
            }

            if (requestHandler is not null)
            {
                Page.Request -= requestHandler;
            }

            if (responseHandler is not null)
            {
                Page.Response -= responseHandler;
            }

            if (requestFailedHandler is not null)
            {
                Page.RequestFailed -= requestFailedHandler;
            }

            if (frameNavigatedHandler is not null)
            {
                Page.FrameNavigated -= frameNavigatedHandler;
            }
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

            _ = WaitForLoadStateAsync(
                options: new PageWaitForLoadStateOptions
                {
                    Timeout = 5000
                },
                cancellationToken: CancellationToken.None).ContinueWith(static (task, state) =>
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
            await WaitForTimeoutAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
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

        return new System.Threading.Timer(static state =>
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
