using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Maliev.ComplianceService.Application.Commands.UpdateWorkAuthorization;

/// <summary>
/// Handles the UpdateWorkAuthorizationCommand.
/// </summary>
public class UpdateWorkAuthorizationCommandHandler : IRequestHandler<UpdateWorkAuthorizationCommand, WorkAuthorizationResponse>
{
    private readonly IWorkAuthorizationRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWorkAuthorizationCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    public UpdateWorkAuthorizationCommandHandler(IWorkAuthorizationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the command to update an existing work authorization.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated work authorization response.</returns>
    public async Task<WorkAuthorizationResponse> Handle(UpdateWorkAuthorizationCommand command, CancellationToken cancellationToken)
    {
        var auth = await _repository.GetByIdAsync(command.AuthId, cancellationToken);
        if (auth == null)
        {
            throw new KeyNotFoundException("AUTHORIZATION_NOT_FOUND");
        }

        // Optimistic locking check
        if (auth.RowVersion != command.Request.RowVersion)
        {
            // For debugging: Console.WriteLine($"Mismatch: DB={auth.RowVersion}, REQ={command.Request.RowVersion}");
            throw new DbUpdateConcurrencyException("CONCURRENT_MODIFICATION");
        }

        // Update fields
        if (command.Request.ExpirationDate.HasValue)
        {
            auth.ExpirationDate = command.Request.ExpirationDate;
            auth.ComplianceStatus = ComplianceStatusExtensions.CalculateStatus(auth.ExpirationDate);
        }

        if (command.Request.SponsorshipStatus.HasValue)
        {
            auth.SponsorshipStatus = command.Request.SponsorshipStatus;
        }

        if (command.Request.Notes != null)
        {
            auth.Notes = command.Request.Notes;
        }

        auth.ModifiedDate = DateTime.UtcNow;
        auth.RowVersion = Guid.NewGuid();

        var updated = await _repository.UpdateAsync(auth, cancellationToken);
        
        return DtoMapper.ToDto(updated);
    }
}
