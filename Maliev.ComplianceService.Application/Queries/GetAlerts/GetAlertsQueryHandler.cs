using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetAlerts;

/// <summary>
/// Handles the GetAlertsQuery.
/// </summary>
public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, IEnumerable<ComplianceAlertResponse>>
{
    private readonly IComplianceAlertRepository _repository;
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlertsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The compliance alert repository.</param>
    /// <param name="employeeService">The employee service client.</param>
    public GetAlertsQueryHandler(IComplianceAlertRepository repository, IEmployeeService employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Handles the query to retrieve alerts.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of compliance alert responses.</returns>
    public async Task<IEnumerable<ComplianceAlertResponse>> Handle(GetAlertsQuery query, CancellationToken cancellationToken)
    {
        var alerts = await _repository.GetAlertsAsync(
            query.IsResolved,
            query.Severity,
            query.EmployeeId,
            query.FromDate,
            query.ToDate,
            query.AlertType,
            query.ResolvedBy,
            cancellationToken);

        var results = new List<ComplianceAlertResponse>();
        foreach (var alert in alerts)
        {
            var name = await _employeeService.GetEmployeeNameAsync(alert.EmployeeId, cancellationToken);
            results.Add(DtoMapper.ToAlertDto(alert, name));
        }

        return results;
    }
}
