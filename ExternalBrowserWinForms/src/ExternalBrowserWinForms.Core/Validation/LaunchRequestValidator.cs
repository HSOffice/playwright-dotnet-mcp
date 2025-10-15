using ExternalBrowserWinForms.Core.Models;

namespace ExternalBrowserWinForms.Core.Validation;

/// <summary>
/// Default implementation for validating browser launch requests.
/// </summary>
public sealed class LaunchRequestValidator : ILaunchRequestValidator
{
    public ValidationResult Validate(BrowserLaunchRequest request)
    {
        if (request is null)
        {
            return ValidationResult.Failure("请求不能为空。");
        }

        if (!request.Url.IsAbsoluteUri)
        {
            return ValidationResult.Failure("请输入一个有效的网址。");
        }

        if (!request.UseDefaultBrowser && string.IsNullOrWhiteSpace(request.BrowserPath))
        {
            return ValidationResult.Failure("请选择一个浏览器可执行文件。");
        }

        return ValidationResult.Success();
    }
}
