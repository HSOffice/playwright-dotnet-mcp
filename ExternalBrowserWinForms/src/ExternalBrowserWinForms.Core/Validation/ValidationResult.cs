namespace ExternalBrowserWinForms.Core.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public static ValidationResult Success() => new(true, null);

    public static ValidationResult Failure(string message) => new(false, message);
}
