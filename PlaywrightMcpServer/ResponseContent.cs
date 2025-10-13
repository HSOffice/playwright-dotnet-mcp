using System.Text.Json.Serialization;

namespace PlaywrightMcpServer;

public interface IResponseContent
{
    [JsonPropertyName("type")]
    string Type { get; }
}

public sealed record TextContent([property: JsonPropertyName("text")] string Text) : IResponseContent
{
    [JsonPropertyName("type")]
    public string Type => "text";
}

public sealed record ImageContent(
    [property: JsonPropertyName("data")] string Data,
    [property: JsonPropertyName("mimeType")] string MimeType) : IResponseContent
{
    [JsonPropertyName("type")]
    public string Type => "image";
}

public sealed record SerializedResponse(
    [property: JsonPropertyName("content")] IReadOnlyList<IResponseContent> Content,
    [property: JsonPropertyName("isError")] bool? IsError);

public sealed class ResponseSerializationOptions
{
    public bool OmitSnapshot { get; init; }

    public bool OmitBlobs { get; init; }
}
