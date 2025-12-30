using System.ComponentModel.DataAnnotations;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object for resolving a compliance alert.
/// </summary>
public record ResolveAlertRequest
{
    /// <summary> The unique identifier of the user resolving the alert. </summary>
    [Required]
    public Guid ResolvedBy { get; init; }

    /// <summary> Notes explaining how the alert was resolved. </summary>
    [Required, MinLength(10)]
    public string ResolutionNotes { get; init; } = string.Empty;
}
