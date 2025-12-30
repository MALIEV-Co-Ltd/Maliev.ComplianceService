using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Provides a statistical breakdown for a specific authorization type.
/// </summary>
public record AuthorizationTypeBreakdown
{
    /// <summary> The type of authorization. </summary>
    public AuthorizationType Type { get; init; }
    
    /// <summary> Total count of this type. </summary>
    public int Count { get; init; }
    
    /// <summary> Number of this type expiring soon. </summary>
    public int ExpiringSoon { get; init; }
    
    /// <summary> Number of this type that have expired. </summary>
    public int Expired { get; init; }
}
