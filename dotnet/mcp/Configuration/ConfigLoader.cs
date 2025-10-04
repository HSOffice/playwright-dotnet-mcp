using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightMcp.Configuration;

/// <summary>
/// Responsible for loading configuration from disk or other sources.
/// </summary>
public static class ConfigLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<FullConfig> LoadAsync(Stream? stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            return new FullConfig();
        }

        try
        {
            return (await JsonSerializer.DeserializeAsync<FullConfig>(stream, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false))
                ?? new FullConfig();
        }
        catch (JsonException)
        {
            // Surface a deterministic configuration even if parsing fails.
            return new FullConfig();
        }
    }
}
