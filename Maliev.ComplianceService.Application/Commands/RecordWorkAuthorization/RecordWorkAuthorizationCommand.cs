using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;

/// <summary>
/// Command to record a new work authorization for an employee.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="Request">The work authorization details.</param>
public record RecordWorkAuthorizationCommand(Guid EmployeeId, RecordWorkAuthorizationRequest Request) : IRequest<WorkAuthorizationResponse>;
