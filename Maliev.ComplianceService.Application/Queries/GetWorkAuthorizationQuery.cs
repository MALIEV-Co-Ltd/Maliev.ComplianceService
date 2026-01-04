using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetWorkAuthorization;

/// <summary>
/// Query to retrieve a single work authorization by its unique identifier.
/// </summary>
/// <param name="AuthId">The unique identifier of the work authorization.</param>
public record GetWorkAuthorizationQuery(Guid AuthId) : IRequest<WorkAuthorizationResponse>;
