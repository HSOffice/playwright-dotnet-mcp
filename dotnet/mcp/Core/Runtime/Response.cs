using System.Collections.Generic;

namespace PlaywrightMcp.Core.Runtime;

/// <summary>
/// Represents the result of a tool invocation.
/// </summary>
public sealed class Response
{
    private readonly List<ResponseBlock> _blocks = new();

    public bool Success { get; init; } = true;

    public IReadOnlyList<ResponseBlock> Blocks => _blocks;

    public IDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

    public static Response Empty { get; } = new();

    public void AddBlock(ResponseBlock block)
    {
        if (block is not null)
        {
            _blocks.Add(block);
        }
    }
}
