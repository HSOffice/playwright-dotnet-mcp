using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcpServer;

public sealed class Response
{
    private readonly ResponseContext _context;
    private readonly SecretRedactor _redactor;
    private readonly List<string> _result = new();
    private readonly List<string> _code = new();
    private readonly List<ResponseImage> _images = new();

    private SnapshotPayload? _snapshot;
    private bool _includeSnapshot;
    private bool _includeTabs;
    private bool? _isError;

    public Response(ResponseContext context, string toolName, IReadOnlyDictionary<string, object?> toolArgs, Action<string>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
        ToolArgs = toolArgs ?? throw new ArgumentNullException(nameof(toolArgs));
        Logger = logger;
        _redactor = new SecretRedactor(context.Configuration.Secrets);
    }

    public string ToolName { get; }

    public IReadOnlyDictionary<string, object?> ToolArgs { get; }

    public Action<string>? Logger { get; }

    public void AddResult(string result)
    {
        if (!string.IsNullOrEmpty(result))
        {
            _result.Add(result);
        }
    }

    public void AddError(string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _result.Add(error);
        }

        _isError = true;
    }

    public void AddCode(string code)
    {
        if (!string.IsNullOrEmpty(code))
        {
            _code.Add(code);
        }
    }

    public void AddImage(string contentType, byte[] data)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentType);
        ArgumentNullException.ThrowIfNull(data);
        _images.Add(new ResponseImage(contentType, data));
    }

    public void SetIncludeSnapshot() => _includeSnapshot = true;

    public void SetIncludeTabs() => _includeTabs = true;

    public bool? IsError() => _isError;

    public async Task FinishAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_includeSnapshot && _context.CurrentTab is not null)
        {
            _snapshot = await _context.CaptureSnapshotAsync(_context.CurrentTab, cancellationToken).ConfigureAwait(false);
        }

        if (_includeTabs)
        {
            foreach (var tab in _context.Tabs)
            {
                await tab.UpdateTitleAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public SerializedResponse Serialize(ResponseSerializationOptions? options = null)
    {
        options ??= new ResponseSerializationOptions();

        var sections = new List<string>();

        if (_result.Count > 0)
        {
            sections.Add("### Result");
            sections.Add(string.Join("\n", _result));
            sections.Add(string.Empty);
        }

        if (_code.Count > 0)
        {
            sections.Add("### Ran Playwright code");
            sections.Add("```js");
            sections.Add(string.Join("\n", _code));
            sections.Add("```");
            sections.Add(string.Empty);
        }

        if (_includeSnapshot || _includeTabs)
        {
            sections.AddRange(RenderTabsMarkdown(_context.DescribeTabs(), _includeTabs));
        }

        if (_snapshot is not null)
        {
            sections.AddRange(SnapshotMarkdownBuilder.Build(_snapshot, options.OmitSnapshot));
            sections.Add(string.Empty);
        }

        var text = string.Join("\n", sections);

        var content = new List<IResponseContent>
        {
            new TextContent(text)
        };

        if (_context.Configuration.ImageResponses == ImageResponseMode.Include)
        {
            foreach (var image in _images)
            {
                var payload = options.OmitBlobs ? "<blob>" : Convert.ToBase64String(image.Data);
                content.Add(new ImageContent(payload, image.ContentType));
            }
        }

        _redactor.Redact(content);
        return new SerializedResponse(content, _isError);
    }

    public void LogBegin()
    {
        Logger?.Invoke($"{ToolName}: begin {FormatArgs(ToolArgs)}");
    }

    public void LogEnd()
    {
        var summary = Serialize(new ResponseSerializationOptions { OmitSnapshot = true, OmitBlobs = true });
        var text = summary.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? string.Empty;
        Logger?.Invoke($"{ToolName}: end {text}");
    }

    private static IReadOnlyList<string> RenderTabsMarkdown(IReadOnlyList<TabDescriptor> tabs, bool force)
    {
        if (tabs.Count == 1 && !force)
        {
            return Array.Empty<string>();
        }

        if (tabs.Count == 0)
        {
            return new[]
            {
                "### Open tabs",
                "No open tabs. Use the \"browser_navigate\" tool to navigate to a page first.",
                string.Empty
            };
        }

        var lines = new List<string> { "### Open tabs" };
        for (var i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var current = tab.IsActive ? " (current)" : string.Empty;
            var title = string.IsNullOrEmpty(tab.Title) ? "about:blank" : tab.Title;
            var url = string.IsNullOrEmpty(tab.Url) ? "about:blank" : tab.Url;
            lines.Add($"- {i}:{current} [{title}] ({url})");
        }

        lines.Add(string.Empty);
        return lines;
    }

    private static string FormatArgs(IReadOnlyDictionary<string, object?> args)
    {
        if (args.Count == 0)
        {
            return "{}";
        }

        var parts = args
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={pair.Value}");
        return "{" + string.Join(", ", parts) + "}";
    }

    private sealed record ResponseImage(string ContentType, byte[] Data);
}
