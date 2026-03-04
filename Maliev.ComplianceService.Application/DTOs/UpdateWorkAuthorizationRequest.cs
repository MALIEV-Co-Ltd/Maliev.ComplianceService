using Maliev.ComplianceService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object for updating an existing work authorization.
/// </summary>
public record UpdateWorkAuthorizationRequest
{
    /// <summary> New expiration date. </summary>
    public DateTime? ExpirationDate { get; init; }

    /// <summary> New sponsorship status. </summary>
    public SponsorshipStatus? SponsorshipStatus { get; init; }

    /// <summary> Updated notes. </summary>
    public string? Notes { get; init; }

    /// <summary> Current row version for optimistic locking. </summary>
    [Required]
    public byte[] RowVersion { get; init; } = null!;
}
