using System.ComponentModel.DataAnnotations;
using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Domain.Entities;

/// <summary>
/// Represents an employee's legal authorization to work
/// </summary>
public class WorkAuthorization
{
    /// <summary>
    /// Unique identifier for this work authorization
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to employee in Employee Service
    /// </summary>
    [Required]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Type of work authorization (Citizen, WorkVisa, etc.)
    /// </summary>
    [Required]
    public AuthorizationType AuthorizationType { get; set; }

    /// <summary>
    /// Official document number (e.g., EAC1234567890)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date when authorization was issued
    /// </summary>
    [Required]
    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Date when authorization expires (null for permanent residents)
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Authority that issued the authorization (e.g., USCIS)
    /// </summary>
    [MaxLength(200)]
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Status of visa sponsorship
    /// </summary>
    public SponsorshipStatus? SponsorshipStatus { get; set; }

    /// <summary>
    /// Reference to right-to-work document in Upload Service
    /// </summary>
    public Guid? RightToWorkDocumentId { get; set; }

    /// <summary>
    /// Additional notes about this authorization
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if this authorization is active (soft delete)
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The last expiration threshold (90, 60, 30) for which an alert was generated.
    /// </summary>
    public int? LastExpirationAlertThreshold { get; set; }

    /// <summary>
    /// Timestamp when the access revocation event was published.
    /// </summary>
    public DateTime? AccessRevocationSentDate { get; set; }

    /// <summary>
    /// Current compliance status (auto-calculated from expiration date)
    /// </summary>
    [Required]
    public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.PendingVerification;

    /// <summary>
    /// Timestamp when this record was created
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this record was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Optimistic concurrency token
    /// </summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Navigation property for compliance alerts related to this authorization
    /// </summary>
    public ICollection<ComplianceAlert> ComplianceAlerts { get; set; } = new List<ComplianceAlert>();
}
