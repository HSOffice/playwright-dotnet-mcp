using System.Text;
using System.Threading.Tasks;

namespace PlaywrightMcp.Core.Runtime;

/// <summary>
/// Generates textual representations of the current browser state.
/// </summary>
public sealed class SnapshotBuilder
{
    public Task<string> BuildSnapshotAsync(string sourceDescription)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Snapshot");
        builder.AppendLine(sourceDescription);
        return Task.FromResult(builder.ToString());
    }
}
