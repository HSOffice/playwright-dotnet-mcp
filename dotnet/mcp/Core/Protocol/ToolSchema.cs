using System.Text.Json;

namespace PlaywrightMcp.Core.Protocol;

/// <summary>
/// Represents the schema for tool parameters and response metadata.
/// </summary>
public sealed class ToolSchema
{
    public ToolSchema(JsonElement jsonSchema)
    {
        JsonSchema = jsonSchema;
    }

    public JsonElement JsonSchema { get; }

    public static ToolSchema Empty => new(JsonDocument.Parse("{}").RootElement);
}
