using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PlaywrightMcpServer;

internal static class ResponseJsonSerializer
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(ConfigurePolymorphism);

        return new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                resolver,
                JsonSerializerOptions.Default.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
        };
    }

    private static void ConfigurePolymorphism(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(IResponseContent))
        {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = GetFailUnknownDerivedTypeHandling()
        };

        typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(TextContent), "text"));
        typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(ImageContent), "image"));
    }

    private static JsonUnknownDerivedTypeHandling GetFailUnknownDerivedTypeHandling()
        => Enum.TryParse("Fail", ignoreCase: false, out JsonUnknownDerivedTypeHandling failHandling)
            ? failHandling
            : default;
}
