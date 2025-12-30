using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.DTOs;

/// <summary>
/// Data transfer object for work authorizations that are nearing expiration.
/// </summary>
public record ExpiringAuthorizationResponse : WorkAuthorizationResponse
{
}
