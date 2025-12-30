using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object representing a work authorization response.
/// </summary>
public record WorkAuthorizationResponse
{
    /// <summary> Unique identifier. </summary>
    public Guid Id { get; init; }
    
    /// <summary> Employee unique identifier. </summary>
    public Guid EmployeeId { get; init; }
    
    /// <summary> Name of the employee. </summary>
    public string EmployeeName { get; init; } = "(name unavailable)";
    
    /// <summary> Type of authorization. </summary>
    public AuthorizationType AuthorizationType { get; init; }
    
    /// <summary> Official document number. </summary>
    public string DocumentNumber { get; init; } = string.Empty;
    
    /// <summary> Date of issue. </summary>
    public DateTime IssueDate { get; init; }
    
    /// <summary> Expiration date. </summary>
    public DateTime? ExpirationDate { get; init; }
    
    /// <summary> Number of days until expiration. </summary>
    public int? DaysUntilExpiration { get; init; }
    
    /// <summary> Issuing authority. </summary>
    public string? IssuingAuthority { get; init; }
    
    /// <summary> Sponsorship status. </summary>
    public SponsorshipStatus? SponsorshipStatus { get; init; }
    
    /// <summary> Current compliance status. </summary>
    public ComplianceStatus ComplianceStatus { get; init; }
    
    /// <summary> Record creation timestamp. </summary>
    public DateTime CreatedDate { get; init; }
    
    /// <summary> Last modification timestamp. </summary>
    public DateTime? ModifiedDate { get; init; }
    
    /// <summary> Concurrency token. </summary>
    public Guid RowVersion { get; init; }
}
