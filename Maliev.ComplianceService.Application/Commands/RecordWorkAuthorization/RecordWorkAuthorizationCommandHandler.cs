using System.ComponentModel.DataAnnotations;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Application.Mappers;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using MediatR;

namespace Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;

/// <summary>
/// Handles the RecordWorkAuthorizationCommand.
/// </summary>
public class RecordWorkAuthorizationCommandHandler : IRequestHandler<RecordWorkAuthorizationCommand, WorkAuthorizationResponse>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly RecordWorkAuthorizationValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordWorkAuthorizationCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    public RecordWorkAuthorizationCommandHandler(IWorkAuthorizationRepository repository)
    {
        _repository = repository;
        _validator = new RecordWorkAuthorizationValidator();
    }

    /// <summary>
    /// Handles the command to record a new work authorization.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created work authorization response.</returns>
    public async Task<WorkAuthorizationResponse> Handle(RecordWorkAuthorizationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors[0].ErrorCode);
        }

        if (await _repository.HasActiveAuthorizationAsync(command.EmployeeId, command.Request.AuthorizationType, cancellationToken))
        {
            throw new InvalidOperationException("DUPLICATE_AUTHORIZATION");
        }

        var entity = new WorkAuthorization
        {
            EmployeeId = command.EmployeeId,
            AuthorizationType = command.Request.AuthorizationType,
            DocumentNumber = command.Request.DocumentNumber!,
            IssueDate = command.Request.IssueDate,
            ExpirationDate = command.Request.ExpirationDate,
            IssuingAuthority = command.Request.IssuingAuthority,
            SponsorshipStatus = command.Request.SponsorshipStatus,
            RightToWorkDocumentId = command.Request.RightToWorkDocumentId,
            Notes = command.Request.Notes,
            ComplianceStatus = ComplianceStatusExtensions.CalculateStatus(command.Request.ExpirationDate)
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        
        return DtoMapper.ToDto(created);
    }
}
