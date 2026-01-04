using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetExpiringAuthorizations;

/// <summary>
/// Query to retrieve work authorizations approaching expiration.
/// </summary>
/// <param name="DaysUntilExpiration">The threshold in days for expiration (default 90).</param>
public record GetExpiringAuthorizationsQuery(int DaysUntilExpiration = 90) : IRequest<IEnumerable<ExpiringAuthorizationResponse>>;
