using System;

namespace PlaywrightMcp.Core.Utils;

/// <summary>
/// Provides testable access to the current time.
/// </summary>
public class TimeProvider
{
    public virtual DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
}
