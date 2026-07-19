namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Severity levels for compliance alerts
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Low priority alert - informational
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority - action recommended within reasonable timeframe
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority - action needed soon
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - immediate action required
    /// </summary>
    Critical = 3
}
