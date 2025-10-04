using System.Collections.Generic;

namespace PlaywrightMcp.Core.Context;

/// <summary>
/// Represents the lifecycle of a browser session handled by the MCP server.
/// </summary>
public sealed class Context
{
    private readonly List<Tab> _tabs = new();

    public IReadOnlyList<Tab> Tabs => _tabs;

    public Tab CreateTab(string id)
    {
        var tab = new Tab(id);
        _tabs.Add(tab);
        return tab;
    }
}
