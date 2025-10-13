using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Microsoft.Playwright;

namespace PlaywrightMcpServer;

public sealed partial class PlaywrightTools
{
    [McpServerTool(Name = "browser_pdf_save")]
    [Description("Save page as PDF.")]
    public static async Task<string> BrowserPdfSaveAsync(
        [Description("File name to save the pdf to. Defaults to `page-{timestamp}.pdf` if not specified.")] string? filename = null,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["filename"] = filename
        };

        return await ExecuteWithResponseAsync(
            "browser_pdf_save",
            args,
            async (response, token) =>
            {
                var tab = await GetActiveTabAsync(token).ConfigureAwait(false);
                var fileName = string.IsNullOrWhiteSpace(filename)
                    ? GenerateTimestampedFileName("pdf")
                    : filename!;
                var outputPath = ResolvePdfOutputPath(fileName);
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                response.AddCode($"await page.pdf({{ path: {QuoteJsString(outputPath)} }});");
                response.AddResult($"Saved page as {outputPath}");
                await tab.Page.PdfAsync(new PagePdfOptions
                {
                    Path = outputPath
                }).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
