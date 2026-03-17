using Asp.Versioning;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Queries.GetComplianceReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.ComplianceService.Api.Controllers;

/// <summary>
/// Controller for generating organizational compliance reports.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("compliance/v{version:apiVersion}/compliance-reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generates a compliance summary report with optional department filtering.
    /// </summary>
    /// <param name="departmentId">Optional filter by department unique identifier.</param>
    /// <returns>A compliance summary report.</returns>
    [HttpGet("compliance")]
    public async Task<ActionResult<ComplianceReportResponse>> GetComplianceReport([FromQuery] Guid? departmentId = null)
    {
        var query = new GetComplianceReportQuery(departmentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
