using System.Collections.Generic;
using System.Linq;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Configuration;

/// <summary>
/// Applies capability-based filtering to the tool registry.
/// </summary>
public static class CapabilityFilters
{
    public static IReadOnlyCollection<IToolDefinition> FilterTools(
        IEnumerable<IToolDefinition> tools,
        CapabilitySettings settings)
    {
        if (tools is null)
        {
            return new List<IToolDefinition>();
        }

        IEnumerable<IToolDefinition> result = tools;

        if (settings.IncludedTools is { Count: > 0 })
        {
            var allowed = settings.IncludedTools.ToHashSet();
            result = result.Where(t => allowed.Contains(t.Name));
        }

        if (settings.ExcludedTools is { Count: > 0 })
        {
            var excluded = settings.ExcludedTools.ToHashSet();
            result = result.Where(t => !excluded.Contains(t.Name));
        }

        return result.ToList();
    }
}
