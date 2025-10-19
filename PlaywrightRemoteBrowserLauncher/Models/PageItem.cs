using Microsoft.Playwright;

namespace PlaywrightRemoteBrowserLauncher.Models;

public sealed class PageItem
{
    public PageItem(IPage page, string name, string? title = null)
    {
        Page = page;
        Name = name;
        Title = title;
    }

    public IPage Page { get; }

    public string Name { get; }

    public string? Title { get; }

    private string DisplayText => string.IsNullOrWhiteSpace(Title) ? Name : $"{Name} - {Title}";

    public override string ToString() => DisplayText;
}
