using System;

namespace PlaywrightMcp.Core.Runtime;

/// <summary>
/// Base class for a structured block within a response payload.
/// </summary>
public abstract class ResponseBlock
{
    protected ResponseBlock(string kind)
    {
        Kind = kind;
    }

    public string Kind { get; }
}

public sealed class MarkdownBlock : ResponseBlock
{
    public MarkdownBlock(string text)
        : base("markdown")
    {
        Text = text;
    }

    public string Text { get; }
}

public sealed class ImageBlock : ResponseBlock
{
    public ImageBlock(byte[] data, string mediaType)
        : base("image")
    {
        Data = data;
        MediaType = mediaType;
    }

    public byte[] Data { get; }

    public string MediaType { get; }
}

public sealed class JsonBlock : ResponseBlock
{
    public JsonBlock(object value)
        : base("json")
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public object Value { get; }
}
