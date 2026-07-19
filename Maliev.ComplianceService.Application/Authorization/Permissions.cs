namespace Maliev.ComplianceService.Application.Authorization;

/// <summary>
/// Defines the permissions for the Compliance Service.
/// </summary>
public static class CompliancePermissions
{
    public const string RecordCreate = "compliance.records.create";
    public const string RecordRead = "compliance.records.read";
    public const string RecordUpdate = "compliance.records.update";
    public const string RecordDelete = "compliance.records.delete";

    public const string RequirementCreate = "compliance.requirements.create";
    public const string RequirementRead = "compliance.requirements.read";
    public const string RequirementUpdate = "compliance.requirements.update";
    public const string RequirementApprove = "compliance.requirements.approve";

    public const string AuditCreate = "compliance.audits.create";
    public const string AuditRead = "compliance.audits.read";
    public const string AuditClose = "compliance.audits.close";

    public const string TrainingView = "compliance.training.view";
    public const string TrainingManage = "compliance.training.manage";

    public const string ReportGenerate = "compliance.reports.generate";
    public const string ReportExport = "compliance.reports.export";

    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { RecordCreate, "Create compliance records" },
        { RecordRead, "Read compliance records" },
        { RecordUpdate, "Update compliance records" },
        { RecordDelete, "Delete compliance records" },
        { RequirementCreate, "Create compliance requirements" },
        { RequirementRead, "Read compliance requirements" },
        { RequirementUpdate, "Update compliance requirements" },
        { RequirementApprove, "Approve compliance requirements" },
        { AuditCreate, "Create compliance audits" },
        { AuditRead, "Read compliance audits" },
        { AuditClose, "Close compliance audits" },
        { TrainingView, "View compliance training" },
        { TrainingManage, "Manage compliance training" },
        { ReportGenerate, "Generate compliance reports" },
        { ReportExport, "Export compliance reports" },
    };

    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
