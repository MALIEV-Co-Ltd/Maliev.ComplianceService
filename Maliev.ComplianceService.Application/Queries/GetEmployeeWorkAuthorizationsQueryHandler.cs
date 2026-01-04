using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetEmployeeWorkAuthorizations;

/// <summary>
/// Handles the GetEmployeeWorkAuthorizationsQuery.
/// </summary>
public class GetEmployeeWorkAuthorizationsQueryHandler : IRequestHandler<GetEmployeeWorkAuthorizationsQuery, IEnumerable<WorkAuthorizationResponse>>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEmployeeWorkAuthorizationsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    /// <param name="employeeService">The employee service client.</param>
    public GetEmployeeWorkAuthorizationsQueryHandler(IWorkAuthorizationRepository repository, IEmployeeService employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Handles the query to retrieve all work authorizations for an employee.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of work authorization responses.</returns>
    public async Task<IEnumerable<WorkAuthorizationResponse>> Handle(GetEmployeeWorkAuthorizationsQuery query, CancellationToken cancellationToken)
    {
        var authorizations = await _repository.GetByEmployeeIdAsync(query.EmployeeId, cancellationToken);
        
        var name = await _employeeService.GetEmployeeNameAsync(query.EmployeeId, cancellationToken);
        
        return authorizations.Select(a => DtoMapper.ToDto(a, name)).ToList();
    }
}
