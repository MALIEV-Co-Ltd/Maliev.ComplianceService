namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Status of visa sponsorship for work authorizations
/// </summary>
public enum SponsorshipStatus
{
    /// <summary>
    /// Sponsorship not required for this authorization type
    /// </summary>
    NotRequired = 0,

    /// <summary>
    /// Currently sponsored by the organization
    /// </summary>
    Sponsored = 1,

    /// <summary>
    /// Transfer of sponsorship to another employer in progress
    /// </summary>
    TransferPending = 2,

    /// <summary>
    /// Renewal of sponsorship in progress
    /// </summary>
    RenewalPending = 3,

    /// <summary>
    /// Sponsorship has expired
    /// </summary>
    Expired = 4
}
