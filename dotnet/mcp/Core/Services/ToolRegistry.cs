using System.Collections.Generic;
using System.Linq;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Core.Services;

/// <summary>
/// Stores tool definitions exposed by the server.
/// </summary>
public sealed class ToolRegistry
{
    private readonly Dictionary<string, IToolDefinition> _tools = new();

    public void RegisterTools(IEnumerable<IToolDefinition> tools)
    {
        foreach (var tool in tools)
        {
            _tools[tool.Name] = tool;
        }
    }

    public IReadOnlyCollection<IToolDefinition> GetTools() => _tools.Values.ToList();

    public bool TryGetTool(string name, out IToolDefinition tool) => _tools.TryGetValue(name, out tool!);
}
