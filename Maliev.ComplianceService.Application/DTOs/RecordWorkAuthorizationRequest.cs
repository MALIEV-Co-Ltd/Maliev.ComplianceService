using System.ComponentModel.DataAnnotations;
using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object for recording a new work authorization.
/// </summary>
public record RecordWorkAuthorizationRequest
{
    /// <summary> The type of authorization. </summary>
    [Required]
    public AuthorizationType AuthorizationType { get; init; }

    /// <summary> The official document number. </summary>
    [StringLength(100)]
    public string? DocumentNumber { get; init; }

    /// <summary> The date the authorization was issued. </summary>
    [Required]
    public DateTime IssueDate { get; init; }

    /// <summary> The date the authorization expires. </summary>
    public DateTime? ExpirationDate { get; init; }

    /// <summary> The authority that issued the document. </summary>
    [StringLength(200)]
    public string? IssuingAuthority { get; init; }

    /// <summary> The sponsorship status of the authorization. </summary>
    public SponsorshipStatus? SponsorshipStatus { get; init; }

    /// <summary> Reference to the supporting document. </summary>
    public Guid? RightToWorkDocumentId { get; init; }

    /// <summary> Additional notes. </summary>
    [StringLength(2000)]
    public string? Notes { get; init; }
}
