using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetEmployeeWorkAuthorizations;

/// <summary>
/// Query to retrieve all work authorizations for a specific employee.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
public record GetEmployeeWorkAuthorizationsQuery(Guid EmployeeId) : IRequest<IEnumerable<WorkAuthorizationResponse>>;
