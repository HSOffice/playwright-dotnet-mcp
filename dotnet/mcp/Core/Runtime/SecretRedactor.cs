using System.Collections.Generic;
using System.Linq;

namespace PlaywrightMcp.Core.Runtime;

/// <summary>
/// Provides simple utilities for removing secrets from logs and responses.
/// </summary>
public sealed class SecretRedactor
{
    private readonly IReadOnlyCollection<string> _sensitiveValues;

    public SecretRedactor(IReadOnlyCollection<string> sensitiveValues)
    {
        _sensitiveValues = sensitiveValues ?? new List<string>();
    }

    public string Redact(string input)
    {
        if (string.IsNullOrEmpty(input) || _sensitiveValues.Count == 0)
        {
            return input;
        }

        return _sensitiveValues.Aggregate(input, (current, secret) =>
            string.IsNullOrEmpty(secret) ? current : current.Replace(secret, "***"));
    }
}
