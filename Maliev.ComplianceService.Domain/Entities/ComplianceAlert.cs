using Maliev.ComplianceService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.ComplianceService.Domain.Entities;

/// <summary>
/// Represents a notification about a compliance issue requiring attention
/// </summary>
public class ComplianceAlert
{
    /// <summary>
    /// Unique identifier for this alert
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the work authorization this alert is about
    /// </summary>
    [Required]
    public Guid WorkAuthorizationId { get; set; }

    /// <summary>
    /// Reference to employee (denormalized for fast lookups)
    /// </summary>
    [Required]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Type of alert (ExpirationWarning, Expired, etc.)
    /// </summary>
    [Required]
    public AlertType AlertType { get; set; }

    /// <summary>
    /// Severity level of this alert
    /// </summary>
    [Required]
    public AlertSeverity Severity { get; set; }

    /// <summary>
    /// Human-readable message describing the alert
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this alert has been resolved
    /// </summary>
    [Required]
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// Timestamp when this alert was resolved
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// User ID who resolved this alert
    /// </summary>
    public Guid? ResolvedBy { get; set; }

    /// <summary>
    /// Notes provided when resolving this alert
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Timestamp when this alert was created
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the related work authorization
    /// </summary>
    public WorkAuthorization? WorkAuthorization { get; set; }
}
