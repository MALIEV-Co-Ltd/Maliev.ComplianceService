using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetAlerts;

/// <summary>
/// Query to retrieve compliance alerts with optional filtering.
/// </summary>
/// <param name="IsResolved">Filter by resolution status.</param>
/// <param name="Severity">Filter by alert severity.</param>
/// <param name="EmployeeId">Filter by employee unique identifier.</param>
/// <param name="FromDate">Filter by alerts created on or after this date.</param>
/// <param name="ToDate">Filter by alerts created on or before this date.</param>
/// <param name="AlertType">Filter by alert type.</param>
/// <param name="ResolvedBy">Filter by the user who resolved the alert.</param>
public record GetAlertsQuery(
    bool? IsResolved = null,
    AlertSeverity? Severity = null,
    Guid? EmployeeId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    AlertType? AlertType = null,
    Guid? ResolvedBy = null) : IRequest<IEnumerable<ComplianceAlertResponse>>;
