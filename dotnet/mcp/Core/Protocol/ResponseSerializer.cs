using System.Collections.Generic;
using PlaywrightMcp.Core.Runtime;
using PlaywrightMcp.Core.Utils;

namespace PlaywrightMcp.Core.Protocol;

/// <summary>
/// Serializes runtime responses into protocol structures.
/// </summary>
public static class ResponseSerializer
{
    public static CallToolResponse Serialize(Response response)
    {
        var blocks = new List<object>();
        foreach (var block in response.Blocks)
        {
            blocks.Add(SerializationHelpers.SerializeBlock(block));
        }

        return new CallToolResponse(response.Success, blocks);
    }
}
