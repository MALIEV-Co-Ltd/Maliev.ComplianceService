namespace Maliev.ComplianceService.Domain.Authorization;

/// <summary>
/// Constants for Compliance Service permissions.
/// Follows GCP-style naming: {service}.{resource}.{action}
/// </summary>
public static class CompliancePermissions
{
    /// <summary>Permission to manage work authorizations and documents.</summary>
    public const string Manage = "compliance.authorizations.manage";

    /// <summary>Permission to view reports and audits.</summary>
    public const string Reports = "compliance.reports.view";

    /// <summary>
    /// Collection of all permissions for easy registration.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        { Manage, "Manage work authorizations and compliance documents" },
        { Reports, "View compliance reports and audits" }
    };
}
