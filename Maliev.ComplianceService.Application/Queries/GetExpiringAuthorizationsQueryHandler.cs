using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetExpiringAuthorizations;

/// <summary>
/// Handles the GetExpiringAuthorizationsQuery.
/// </summary>
public class GetExpiringAuthorizationsQueryHandler : IRequestHandler<GetExpiringAuthorizationsQuery, IEnumerable<ExpiringAuthorizationResponse>>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetExpiringAuthorizationsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    /// <param name="employeeService">The employee service client.</param>
    public GetExpiringAuthorizationsQueryHandler(IWorkAuthorizationRepository repository, IEmployeeService employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Handles the query to retrieve expiring authorizations.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of expiring authorization responses.</returns>
    public async Task<IEnumerable<ExpiringAuthorizationResponse>> Handle(GetExpiringAuthorizationsQuery query, CancellationToken cancellationToken)
    {
        var authorizations = await _repository.GetExpiringWithinDaysAsync(query.DaysUntilExpiration, cancellationToken);

        var results = new List<ExpiringAuthorizationResponse>();
        foreach (var auth in authorizations)
        {
            var name = await _employeeService.GetEmployeeNameAsync(auth.EmployeeId, cancellationToken);
            var response = DtoMapper.ToExpiringDto(auth, name);
            results.Add(response);
        }

        return results;
    }
}
