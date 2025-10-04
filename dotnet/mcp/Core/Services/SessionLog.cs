using System.Collections.Generic;

namespace PlaywrightMcp.Core.Services;

/// <summary>
/// Maintains an in-memory history of tool invocations.
/// </summary>
public sealed class SessionLog
{
    private readonly List<string> _entries = new();

    public void Add(string entry)
    {
        if (!string.IsNullOrWhiteSpace(entry))
        {
            _entries.Add(entry);
        }
    }

    public IReadOnlyList<string> Entries => _entries;
}
