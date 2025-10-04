using System;
using System.Collections.Generic;

namespace PlaywrightMcp.Core.Context;

/// <summary>
/// Represents the lifecycle of a browser session handled by the MCP server.
/// </summary>
public sealed class Context
{
    private readonly List<Tab> _tabs = new();

    public bool IsBrowserLaunched { get; private set; }

    public IReadOnlyList<Tab> Tabs => _tabs;

    public Tab CreateTab(string id)
    {
        var tab = new Tab(id);
        _tabs.Add(tab);
        IsBrowserLaunched = true;
        return tab;
    }

    public void CloseBrowser()
    {
        _tabs.Clear();
        IsBrowserLaunched = false;
    }

    public Tab RelaunchBrowser()
    {
        CloseBrowser();
        var tab = CreateTab(Guid.NewGuid().ToString());
        tab.RecordEvent("Browser relaunched");
        return tab;
    }
}
