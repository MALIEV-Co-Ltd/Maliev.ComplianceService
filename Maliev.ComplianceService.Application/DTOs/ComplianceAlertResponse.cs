using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object representing a compliance alert.
/// </summary>
public record ComplianceAlertResponse
{
    /// <summary> Unique identifier. </summary>
    public Guid Id { get; init; }
    
    /// <summary> Employee unique identifier. </summary>
    public Guid EmployeeId { get; init; }
    
    /// <summary> Name of the employee. </summary>
    public string EmployeeName { get; init; } = "(name unavailable)";
    
    /// <summary> Type of alert. </summary>
    public AlertType AlertType { get; init; }
    
    /// <summary> Severity of the alert. </summary>
    public AlertSeverity Severity { get; init; }
    
    /// <summary> Human-readable message. </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary> Indicates if the alert is resolved. </summary>
    public bool IsResolved { get; init; }
    
    /// <summary> Timestamp when the alert was resolved. </summary>
    public DateTime? ResolvedDate { get; init; }
    
    /// <summary> Name of the user who resolved the alert. </summary>
    public string? ResolvedByName { get; init; }
    
    /// <summary> Notes provided during resolution. </summary>
    public string? ResolutionNotes { get; init; }
    
    /// <summary> Alert creation timestamp. </summary>
    public DateTime CreatedDate { get; init; }
}
