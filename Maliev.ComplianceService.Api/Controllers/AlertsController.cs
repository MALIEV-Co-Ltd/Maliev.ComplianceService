using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.ComplianceService.Application.Authorization;
using Maliev.ComplianceService.Application.Commands.ResolveAlert;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Queries.GetAlerts;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.ComplianceService.Api.Controllers;

/// <summary>
/// Controller for managing compliance alerts.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("compliance/v{version:apiVersion}/compliance-alerts")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public AlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves compliance alerts with optional filtering.
    /// </summary>
    /// <param name="isResolved">Filter by resolution status.</param>
    /// <param name="severity">Filter by alert severity.</param>
    /// <param name="employeeId">Filter by employee unique identifier.</param>
    /// <param name="fromDate">Filter by alerts created on or after this date.</param>
    /// <param name="toDate">Filter by alerts created on or before this date.</param>
    /// <param name="alertType">Filter by alert type.</param>
    /// <param name="resolvedBy">Filter by the user who resolved the alert.</param>
    /// <returns>A collection of compliance alerts.</returns>
    [HttpGet]
    [RequirePermission(CompliancePermissions.RecordRead)]
    public async Task<ActionResult<IEnumerable<ComplianceAlertResponse>>> GetAlerts(
        [FromQuery] bool? isResolved = null,
        [FromQuery] AlertSeverity? severity = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] AlertType? alertType = null,
        [FromQuery] Guid? resolvedBy = null)
    {
        var query = new GetAlertsQuery(isResolved, severity, employeeId, fromDate, toDate, alertType, resolvedBy);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Resolves an existing compliance alert.
    /// </summary>
    /// <param name="alertId">The unique identifier of the alert.</param>
    /// <param name="request">The resolution details.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{alertId}/resolve")]
    [RequirePermission(CompliancePermissions.RecordUpdate)]
    public async Task<IActionResult> ResolveAlert(Guid alertId, [FromBody] ResolveAlertRequest request)
    {
        var command = new ResolveAlertCommand(alertId, request);
        await _mediator.Send(command);
        return NoContent();
    }
}
