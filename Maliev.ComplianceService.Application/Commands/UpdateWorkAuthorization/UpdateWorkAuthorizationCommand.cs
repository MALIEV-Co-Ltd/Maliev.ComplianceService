using Maliev.ComplianceService.Application.DTOs;
using MediatR;

namespace Maliev.ComplianceService.Application.Commands.UpdateWorkAuthorization;

/// <summary>
/// Command to update an existing work authorization.
/// </summary>
/// <param name="AuthId">The unique identifier of the work authorization to update.</param>
/// <param name="Request">The updated details.</param>
public record UpdateWorkAuthorizationCommand(Guid AuthId, UpdateWorkAuthorizationRequest Request) : IRequest<WorkAuthorizationResponse>;
