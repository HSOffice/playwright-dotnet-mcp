using System;
using System.Collections.Generic;

namespace PlaywrightMcpServer;

internal sealed class SecretRedactor
{
    private readonly IReadOnlyDictionary<string, string>? _secrets;

    public SecretRedactor(IReadOnlyDictionary<string, string>? secrets)
    {
        _secrets = secrets;
    }

    public void Redact(IList<IResponseContent> content)
    {
        if (_secrets is null || _secrets.Count == 0)
        {
            return;
        }

        for (var i = 0; i < content.Count; i++)
        {
            if (content[i] is not TextContent textContent)
            {
                continue;
            }

            var value = textContent.Text;
            foreach (var kvp in _secrets)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    continue;
                }

                value = value.Replace(kvp.Value, $"<secret>{kvp.Key}</secret>");
            }

            content[i] = new TextContent(value);
        }
    }
}
