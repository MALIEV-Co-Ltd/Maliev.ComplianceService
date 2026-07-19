namespace Maliev.ComplianceService.Domain.Enums;

/// <summary>
/// Specifies the type of compliance alert.
/// </summary>
public enum AlertType
{
    /// <summary> Warning that an authorization is nearing expiration. </summary>
    ExpirationWarning = 0,
    /// <summary> Alert that an authorization has expired. </summary>
    Expired = 1,
    /// <summary> A required document is missing. </summary>
    DocumentMissing = 2,
    /// <summary> Manual verification of the authorization is required. </summary>
    VerificationRequired = 3,
    /// <summary> Reminder to initiate renewal process. </summary>
    RenewalReminder = 4
}
