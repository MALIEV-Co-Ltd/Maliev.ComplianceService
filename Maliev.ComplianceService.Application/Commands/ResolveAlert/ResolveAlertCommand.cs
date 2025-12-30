using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Commands.ResolveAlert;

/// <summary>
/// Command to resolve a compliance alert.
/// </summary>
/// <param name="AlertId">The unique identifier of the alert.</param>
/// <param name="Request">The resolution details.</param>
public record ResolveAlertCommand(Guid AlertId, ResolveAlertRequest Request) : IRequest<bool>;
