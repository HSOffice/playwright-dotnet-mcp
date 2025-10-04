using System.Collections.Generic;
using System.Linq;
using PlaywrightMcp.Core.Protocol;

namespace PlaywrightMcp.Tools;

public static class BrowserTools
{
    public static IReadOnlyCollection<IToolDefinition> All { get; } = BuildTools();

    private static IReadOnlyCollection<IToolDefinition> BuildTools()
    {
        var groups = new[]
        {
            RelaunchTools.CreateTools(),
            CommonTools.CreateTools(),
            ConsoleTools.CreateTools(),
            DialogsTools.CreateTools(),
            EvaluateTools.CreateTools(),
            FilesTools.CreateTools(),
            FormTools.CreateTools(),
            InstallTools.CreateTools(),
            KeyboardTools.CreateTools(),
            MouseTools.CreateTools(),
            NavigateTools.CreateTools(),
            NetworkTools.CreateTools(),
            PdfTools.CreateTools(),
            ScreenshotTools.CreateTools(),
            SnapshotTools.CreateTools(),
            TabsTools.CreateTools(),
            TracingTools.CreateTools(),
            VerifyTools.CreateTools(),
            WaitTools.CreateTools(),
        };

        return groups.SelectMany(static g => g).ToList();
    }
}
