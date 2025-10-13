using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class ResponseSerializationTests
{
    [Fact]
    public void Serialize_IncludesContentSpecificProperties()
    {
        var contents = new List<IResponseContent>
        {
            new TextContent("hello"),
            new ImageContent("ZGF0YQ==", "image/png")
        };

        var response = new SerializedResponse(contents, false);

        var json = JsonSerializer.Serialize(response, ResponseJsonSerializer.Options);

        Assert.Contains("\"text\":\"hello\"", json);
        Assert.Contains("\"data\":\"ZGF0YQ==\"", json);
        Assert.Contains("\"mimeType\":\"image/png\"", json);
    }
}
