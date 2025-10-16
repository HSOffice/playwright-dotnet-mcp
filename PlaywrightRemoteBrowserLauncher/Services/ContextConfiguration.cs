namespace PlaywrightRemoteBrowserLauncher.Services;

public sealed class ContextConfiguration
{
    public bool AcceptDownloads { get; init; } = true;

    public bool IgnoreHttpsErrors { get; init; }

    public bool RecordHar { get; init; }

    public string? RecordHarPath { get; init; }

    public string? InitScript { get; init; }

    public bool ExposeDotnet { get; init; }

    public string ExposedFunctionName { get; init; } = "dotnetPing";
}
