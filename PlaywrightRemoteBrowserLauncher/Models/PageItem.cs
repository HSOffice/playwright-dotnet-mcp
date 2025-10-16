using Microsoft.Playwright;

namespace PlaywrightRemoteBrowserLauncher.Models;

public sealed class PageItem
{
    public PageItem(IPage page, string name)
    {
        Page = page;
        Name = name;
    }

    public IPage Page { get; }

    public string Name { get; }

    public override string ToString() => Name;
}
