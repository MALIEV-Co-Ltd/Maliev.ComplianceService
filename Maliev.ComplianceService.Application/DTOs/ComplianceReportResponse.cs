using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object for the organizational compliance report.
/// </summary>
public record ComplianceReportResponse
{
    /// <summary> The date the report was generated. </summary>
    public DateTime ReportDate { get; init; }
    
    /// <summary> Total number of employees in scope. </summary>
    public int TotalEmployees { get; init; }
    
    /// <summary> Number of employees requiring work authorization. </summary>
    public int RequiresAuthorization { get; init; }
    
    /// <summary> Number of compliant authorizations. </summary>
    public int Compliant { get; init; }
    
    /// <summary> Number of authorizations expiring soon. </summary>
    public int ExpiringSoon { get; init; }
    
    /// <summary> Number of expired authorizations. </summary>
    public int Expired { get; init; }
    
    /// <summary> Overall compliance percentage. </summary>
    public decimal ComplianceRate { get; init; }
    
    /// <summary> Statistics broken down by authorization type. </summary>
    public List<AuthorizationTypeBreakdown> ByAuthorizationType { get; init; } = new();
    
    /// <summary> List of the most recent active alerts. </summary>
    public List<ComplianceAlertResponse> Alerts { get; init; } = new();
}
