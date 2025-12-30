namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Overall compliance status for a work authorization
/// </summary>
public enum ComplianceStatus
{
    /// <summary>
    /// Work authorization is valid and not expiring soon (>90 days)
    /// </summary>
    Compliant = 0,

    /// <summary>
    /// Work authorization expiring within 30-90 days
    /// </summary>
    ExpiringSoon = 1,

    /// <summary>
    /// Work authorization has passed its expiration date
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Work authorization pending verification or review
    /// </summary>
    PendingVerification = 3,

    /// <summary>
    /// Work authorization is non-compliant for reasons other than expiration
    /// </summary>
    NonCompliant = 4
}
