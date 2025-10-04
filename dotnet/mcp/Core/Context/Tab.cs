using System;
using System.Collections.Generic;

namespace PlaywrightMcp.Core.Context;

/// <summary>
/// Represents a single browser tab or page.
/// </summary>
public sealed class Tab
{
    private readonly List<string> _events = new();

    public Tab(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public IReadOnlyList<string> Events => _events;

    public void RecordEvent(string description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            _events.Add(description);
        }
    }
}
