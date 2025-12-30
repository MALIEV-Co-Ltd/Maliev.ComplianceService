namespace Maliev.ComplianceService.Domain.Authorization;

/// <summary>
/// Defines permission constants for the Compliance Service
/// </summary>
public static class CompliancePermissions
{
    /// <summary>
    /// Permission to create, update, and view work authorizations and alerts.
    /// Required for HR administrators managing compliance.
    /// </summary>
    public const string Manage = "compliance.authorizations.manage";

    /// <summary>
    /// Permission to view compliance reports.
    /// Required for HR managers and executives viewing organizational compliance status.
    /// </summary>
    public const string Reports = "compliance.reports.view";
}