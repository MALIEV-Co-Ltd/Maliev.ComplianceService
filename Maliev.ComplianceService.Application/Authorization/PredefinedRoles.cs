namespace Maliev.ComplianceService.Application.Authorization;

/// <summary>
/// Provides access to predefined roles for the Compliance Service.
/// </summary>
public static class CompliancePredefinedRoles
{
    public const string Admin = "roles.compliance.admin";
    public const string Auditor = "roles.compliance.auditor";
    public const string Viewer = "roles.compliance.viewer";

    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (
            Admin,
            "Compliance Administrator with full access",
            new[]
            {
                CompliancePermissions.RecordCreate,
                CompliancePermissions.RecordRead,
                CompliancePermissions.RecordUpdate,
                CompliancePermissions.RecordDelete,
                CompliancePermissions.RequirementCreate,
                CompliancePermissions.RequirementRead,
                CompliancePermissions.RequirementUpdate,
                CompliancePermissions.RequirementApprove,
                CompliancePermissions.AuditCreate,
                CompliancePermissions.AuditRead,
                CompliancePermissions.AuditClose,
                CompliancePermissions.TrainingView,
                CompliancePermissions.TrainingManage,
                CompliancePermissions.ReportGenerate,
                CompliancePermissions.ReportExport,
            }
        ),
        (
            Auditor,
            "Compliance Auditor with audit and report access",
            new[]
            {
                CompliancePermissions.RecordRead,
                CompliancePermissions.RequirementRead,
                CompliancePermissions.AuditCreate,
                CompliancePermissions.AuditRead,
                CompliancePermissions.AuditClose,
                CompliancePermissions.TrainingView,
                CompliancePermissions.ReportGenerate,
                CompliancePermissions.ReportExport,
            }
        ),
        (
            Viewer,
            "Compliance Viewer with read-only access",
            new[]
            {
                CompliancePermissions.RecordRead,
                CompliancePermissions.RequirementRead,
                CompliancePermissions.AuditRead,
                CompliancePermissions.TrainingView,
                CompliancePermissions.ReportGenerate,
            }
        ),
    };
}
