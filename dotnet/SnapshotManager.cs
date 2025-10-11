using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

internal sealed class SnapshotManager
{
    public async Task<SnapshotPayload> CaptureAsync(TabState tab, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tab);
        cancellationToken.ThrowIfCancellationRequested();

        var page = tab.Page;
        string? ariaSnapshot = null;
        try
        {
            ariaSnapshot = await TryGetAriaSnapshotAsync(page, cancellationToken).ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
            // Fallback: still produce snapshot metadata when accessibility tree is unavailable.
        }

        var title = await page.TitleAsync().ConfigureAwait(false);
        var url = page.Url ?? string.Empty;
        var (console, network) = tab.TakeActivitySnapshot();

        var snapshotPayload = new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = url,
            Title = title,
            AriaSnapshot = ariaSnapshot,
            Console = console,
            Network = network,
            ModalStates = tab.GetModalStatesSnapshot(),
            Downloads = tab.GetDownloadsSnapshot()
        };

        tab.UpdateMetadata(url, title, snapshotPayload);
        return snapshotPayload;
    }

    private static async Task<string?> TryGetAriaSnapshotAsync(IPage page, CancellationToken cancellationToken)
    {
        var method = FindSnapshotMethod(page.GetType());
        if (method is null)
        {
            return null;
        }

        var parameters = method.GetParameters();
        var arguments = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                arguments[i] = cancellationToken;
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
                continue;
            }

            arguments[i] = CreateDefaultValue(parameter.ParameterType);
        }

        var invokeResult = method.Invoke(page, arguments);
        if (invokeResult is Task<string> stringTask)
        {
            return await stringTask.ConfigureAwait(false);
        }

        if (invokeResult is Task task)
        {
            await task.ConfigureAwait(false);
            return GetTaskResult(task) as string;
        }

        return invokeResult as string;
    }

    private static MethodInfo? FindSnapshotMethod(Type pageType)
    {
        static bool Matches(MethodInfo method, string name)
        {
            if (!string.Equals(method.Name, name, StringComparison.Ordinal))
            {
                return false;
            }

            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                return false;
            }

            return !method.IsGenericMethod;
        }

        var methods = pageType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        return methods.FirstOrDefault(method => Matches(method, "AriaSnapshotAsync"))
            ?? methods.FirstOrDefault(method => Matches(method, "_SnapshotForAIAsync"))
            ?? methods.FirstOrDefault(method => Matches(method, "SnapshotForAIAsync"));
    }

    private static object? CreateDefaultValue(Type type)
    {
        if (type == typeof(CancellationToken))
        {
            return CancellationToken.None;
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    private static object? GetTaskResult(Task task)
    {
        var type = task.GetType();
        var resultProperty = type.GetProperty("Result");
        return resultProperty?.GetValue(task);
    }
}
