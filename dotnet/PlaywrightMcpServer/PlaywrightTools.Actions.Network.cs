using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_network_requests")]
    [Description("Returns all network requests since loading the page.")]
    public static async Task<string> BrowserNetworkRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithResponseAsync(
            "browser_network_requests",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var requests = tab.GetNetworkRequests();

                if (requests.Count == 0)
                {
                    response.AddResult("No network requests recorded.");
                    return;
                }

                foreach (var request in requests)
                {
                    response.AddResult(FormatNetworkRequest(request));
                }
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static string FormatNetworkRequest(NetworkRequestEntry request)
    {
        var builder = new StringBuilder();
        var method = string.IsNullOrEmpty(request.Method)
            ? string.Empty
            : request.Method.ToUpperInvariant();

        builder.Append('[').Append(method).Append("] ");
        builder.Append(request.Url);

        if (!string.IsNullOrEmpty(request.ResourceType))
        {
            builder.Append(" (").Append(request.ResourceType).Append(')');
        }

        if (request.Status.HasValue)
        {
            builder.Append(" => [").Append(request.Status.Value).Append(']');
        }

        if (!string.IsNullOrEmpty(request.Failure))
        {
            builder.Append(" => Failed: ").Append(request.Failure);
        }

        return builder.ToString();
    }
}
