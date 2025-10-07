using System.Text.Json.Serialization;

namespace PlaywrightMcpServer;

public interface IResponseContent
{
    [JsonPropertyName("type")]
    string Type { get; }
}

public sealed record TextContent(string Text) : IResponseContent
{
    [JsonPropertyName("type")]
    public string Type => "text";

    [JsonPropertyName("text")]
    public string TextValue => Text;
}

public sealed record ImageContent(string Data, string MimeType) : IResponseContent
{
    [JsonPropertyName("type")]
    public string Type => "image";

    [JsonPropertyName("data")]
    public string DataValue => Data;

    [JsonPropertyName("mimeType")]
    public string MimeTypeValue => MimeType;
}

public sealed record SerializedResponse(
    [property: JsonPropertyName("content")] IReadOnlyList<IResponseContent> Content,
    [property: JsonPropertyName("isError")] bool? IsError);

public sealed class ResponseSerializationOptions
{
    public bool OmitSnapshot { get; init; }

    public bool OmitBlobs { get; init; }
}
