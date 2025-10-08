using System.IO;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    internal static string ResolveDownloadOutputPath(string outputPath)
        => Path.GetFullPath(outputPath);
}
