using ExternalBrowserWinForms.Core.Models;

namespace ExternalBrowserWinForms.Core.Validation;

/// <summary>
/// Validates user input prior to launching an external browser.
/// </summary>
public interface ILaunchRequestValidator
{
    ValidationResult Validate(BrowserLaunchRequest request);
}
