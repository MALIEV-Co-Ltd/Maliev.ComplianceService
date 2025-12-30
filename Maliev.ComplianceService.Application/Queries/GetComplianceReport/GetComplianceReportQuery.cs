using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Queries.GetComplianceReport;

/// <summary>
/// Query to generate an organizational compliance report.
/// </summary>
/// <param name="DepartmentId">Optional filter by department unique identifier.</param>
public record GetComplianceReportQuery(Guid? DepartmentId = null) : IRequest<ComplianceReportResponse>;
