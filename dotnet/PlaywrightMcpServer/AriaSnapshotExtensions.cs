using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlaywrightMcpServer;

public static class AriaSnapshotExtensions
{
    /// <summary>
    /// 生成“只含交互控件”的 ARIA YAML（整页）。
    /// </summary>
    public static async Task<string> GetInteractiveAriaSnapshotAsync(
        this IPage page,
        bool keepHeadings = false,
        bool keepUrls = true)
    {
        var locator = page.Locator("html");
        return await locator.GetInteractiveAriaSnapshotAsync(keepHeadings, keepUrls);
    }

    /// <summary>
    /// 生成“只含交互控件”的 ARIA YAML（定位器作用域）。
    /// </summary>
    public static async Task<string> GetInteractiveAriaSnapshotAsync(
        this ILocator locator,
        bool keepHeadings = false,
        bool keepUrls = true)
    {
        // 不使用任何选项；兼容各版本
        var yaml = await locator.AriaSnapshotAsync();
        return FilterYamlToInteractive(yaml, keepHeadings, keepUrls);
    }

    /// <summary>
    /// 过滤 YAML：保留交互控件与必要容器，去掉纯文本等非交互项。
    /// </summary>
    private static string FilterYamlToInteractive(string yaml, bool keepHeadings, bool keepUrls)
    {
        var interactive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "button","link","textbox","searchbox","checkbox","radio","switch",
            "combobox","option","menuitem","tab","treeitem","slider","spinbutton"
        };

        var containers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "document","main","navigation","complementary","form","group",
            "table","rowgroup","row","cell","list","listitem","heading","region"
        };

        bool IsText(string line) => Regex.IsMatch(line, @"^\s*(-\s*)?text:\s");
        bool IsUrlProp(string line) => Regex.IsMatch(line, @"^\s*-\s*/url:\s|^\s*/url:\s");
        bool IsHeading(string line) => Regex.IsMatch(line, @"^\s*-\s*heading\b");
        bool IsContainer(string line)
            => Regex.IsMatch(line, @"^\s*-\s*([A-Za-z]+)\s*:\s*$") &&
               containers.Contains(Regex.Match(line, @"^\s*-\s*([A-Za-z]+)").Groups[1].Value);
        bool IsInteractive(string line)
            => Regex.IsMatch(line, @"^\s*-\s*([A-Za-z]+)\b") &&
               interactive.Contains(Regex.Match(line, @"^\s*-\s*([A-Za-z]+)\b").Groups[1].Value);

        var lines = yaml.Replace("\r\n", "\n").Split('\n');
        var kept = new List<string>(lines.Length);
        bool lastKept = false;

        bool IsAttrOrIndentedOfKept(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return lastKept;
            if (Regex.IsMatch(current, @"^\s*/[A-Za-z]+:\s")) return lastKept;      // 属性行，如 /url:
            if (Regex.IsMatch(current, @"^\s*-\s*/[A-Za-z]+:\s")) return lastKept;  // 以 "- /url:" 形式
            if (Regex.IsMatch(current, @"^\s+") && !Regex.IsMatch(current, @"^\s*-\s+"))
                return lastKept;                                                    // 更深缩进行
            return false;
        }

        foreach (var line in lines)
        {
            if (IsText(line)) { lastKept = false; continue; }
            if (!keepUrls && IsUrlProp(line)) { lastKept = false; continue; }

            if (IsContainer(line) || IsInteractive(line) || (keepHeadings && IsHeading(line)))
            {
                kept.Add(line);
                lastKept = true;
                continue;
            }

            if (IsAttrOrIndentedOfKept(line))
            {
                kept.Add(line);
                lastKept = true;
                continue;
            }

            // 其它未知行：保守保留
            kept.Add(line);
            lastKept = true;
        }

        return string.Join("\n", kept);
    }
}
