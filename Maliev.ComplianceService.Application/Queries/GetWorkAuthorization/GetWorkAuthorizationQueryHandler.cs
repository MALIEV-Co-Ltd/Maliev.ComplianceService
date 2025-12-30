using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetWorkAuthorization;

/// <summary>
/// Handles the GetWorkAuthorizationQuery.
/// </summary>
public class GetWorkAuthorizationQueryHandler : IRequestHandler<GetWorkAuthorizationQuery, WorkAuthorizationResponse>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWorkAuthorizationQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    /// <param name="employeeService">The employee service client.</param>
    public GetWorkAuthorizationQueryHandler(IWorkAuthorizationRepository repository, IEmployeeService employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Handles the query to retrieve a single work authorization.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A work authorization response.</returns>
    public async Task<WorkAuthorizationResponse> Handle(GetWorkAuthorizationQuery query, CancellationToken cancellationToken)
    {
        var auth = await _repository.GetByIdAsync(query.AuthId, cancellationToken);
        if (auth == null)
        {
            throw new KeyNotFoundException("AUTHORIZATION_NOT_FOUND");
        }

        var name = await _employeeService.GetEmployeeNameAsync(auth.EmployeeId, cancellationToken);
        return DtoMapper.ToDto(auth, name);
    }
}
