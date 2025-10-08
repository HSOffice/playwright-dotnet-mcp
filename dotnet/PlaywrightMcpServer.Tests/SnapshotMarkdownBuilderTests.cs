using System;
using System.Linq;
using Xunit;

namespace PlaywrightMcpServer.Tests;

public class SnapshotMarkdownBuilderTests
{
    [Fact]
    public void Build_IncludesDownloadsSection()
    {
        var snapshot = new SnapshotPayload
        {
            Timestamp = DateTimeOffset.UtcNow,
            Url = "https://example.com",
            Title = "Example",
            Aria = null,
            Console = Array.Empty<ConsoleMessageEntry>(),
            Network = Array.Empty<NetworkRequestEntry>(),
            ModalStates = Array.Empty<ModalStateEntry>(),
            Downloads = new[]
            {
                new DownloadEntry
                {
                    SuggestedFileName = "report.pdf",
                    OutputPath = "/tmp/report.pdf",
                    Finished = true
                },
                new DownloadEntry
                {
                    SuggestedFileName = "data.csv",
                    OutputPath = "/tmp/data.csv",
                    Finished = false
                }
            }
        };

        var lines = SnapshotMarkdownBuilder.Build(snapshot, omitSnapshot: true);
        var downloadsSection = lines
            .SkipWhile(line => line != "### Downloads")
            .TakeWhile(line => !string.IsNullOrEmpty(line))
            .ToArray();

        Assert.Contains("- Downloaded file report.pdf to /tmp/report.pdf", downloadsSection);
        Assert.Contains("- Downloading file data.csv ...", downloadsSection);
    }
}
