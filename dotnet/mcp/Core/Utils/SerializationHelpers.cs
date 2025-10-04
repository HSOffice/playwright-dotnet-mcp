using System;
using System.Collections.Generic;
using PlaywrightMcp.Core.Runtime;

namespace PlaywrightMcp.Core.Utils;

public static class SerializationHelpers
{
    public static object SerializeBlock(ResponseBlock block) => block switch
    {
        MarkdownBlock markdown => new Dictionary<string, object>
        {
            ["kind"] = markdown.Kind,
            ["text"] = markdown.Text
        },
        ImageBlock image => new Dictionary<string, object>
        {
            ["kind"] = image.Kind,
            ["mediaType"] = image.MediaType,
            ["data"] = Convert.ToBase64String(image.Data)
        },
        JsonBlock json => new Dictionary<string, object>
        {
            ["kind"] = json.Kind,
            ["value"] = json.Value
        },
        _ => new Dictionary<string, object> { ["kind"] = block.Kind }
    };
}
