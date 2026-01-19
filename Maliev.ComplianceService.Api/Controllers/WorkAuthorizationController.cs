using Asp.Versioning;
using Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;
using Maliev.ComplianceService.Application.Commands.UpdateWorkAuthorization;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Queries.GetEmployeeWorkAuthorizations;
using Maliev.ComplianceService.Application.Queries.GetExpiringAuthorizations;
using Maliev.ComplianceService.Application.Queries.GetWorkAuthorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.ComplianceService.Api.Controllers;

/// <summary>
/// Controller for managing employee work authorizations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("compliance/v{version:apiVersion}/work-authorizations")]
public class WorkAuthorizationController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkAuthorizationController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public WorkAuthorizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Records a new work authorization for an employee.
    /// </summary>
    /// <param name="employeeId">The employee unique identifier.</param>
    /// <param name="request">The work authorization details.</param>
    /// <returns>The created work authorization details.</returns>
    [HttpPost("employees/{employeeId}")]
    public async Task<ActionResult<WorkAuthorizationResponse>> RecordWorkAuthorization(
        Guid employeeId,
        [FromBody] RecordWorkAuthorizationRequest request)
    {
        var command = new RecordWorkAuthorizationCommand(employeeId, request);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetWorkAuthorization), new { authId = result.Id }, result);
    }

    /// <summary>
    /// Retrieves all work authorizations for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee unique identifier.</param>
    /// <returns>A collection of work authorizations.</returns>
    [HttpGet("employees/{employeeId}")]
    public async Task<ActionResult<IEnumerable<WorkAuthorizationResponse>>> GetEmployeeWorkAuthorizations(Guid employeeId)
    {
        var query = new GetEmployeeWorkAuthorizationsQuery(employeeId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single work authorization by its ID.
    /// </summary>
    /// <param name="authId">The work authorization unique identifier.</param>
    /// <returns>The work authorization details.</returns>
    [HttpGet("{authId}")]
    public async Task<ActionResult<WorkAuthorizationResponse>> GetWorkAuthorization(Guid authId)
    {
        var query = new GetWorkAuthorizationQuery(authId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing work authorization.
    /// </summary>
    /// <param name="authId">The work authorization unique identifier.</param>
    /// <param name="request">The updated details.</param>
    /// <returns>The updated work authorization details.</returns>
    [HttpPut("{authId}")]
    public async Task<ActionResult<WorkAuthorizationResponse>> UpdateWorkAuthorization(
        Guid authId,
        [FromBody] UpdateWorkAuthorizationRequest request)
    {
        var command = new UpdateWorkAuthorizationCommand(authId, request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves work authorizations approaching expiration.
    /// </summary>
    /// <param name="daysUntilExpiration">The threshold in days for expiration (default 90).</param>
    /// <returns>A collection of expiring work authorizations.</returns>
    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<ExpiringAuthorizationResponse>>> GetExpiringAuthorizations(
        [FromQuery] int daysUntilExpiration = 90)
    {
        var query = new GetExpiringAuthorizationsQuery(daysUntilExpiration);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
