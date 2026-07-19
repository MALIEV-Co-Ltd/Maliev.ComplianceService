using Maliev.ComplianceService.Application.Interfaces;
using MediatR;

namespace Maliev.ComplianceService.Application.Commands.ResolveAlert;

/// <summary>
/// Handles the ResolveAlertCommand.
/// </summary>
public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, bool>
{
    private readonly IComplianceAlertRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveAlertCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The compliance alert repository.</param>
    public ResolveAlertCommandHandler(IComplianceAlertRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the command to resolve an alert.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the alert was successfully resolved.</returns>
    public async Task<bool> Handle(ResolveAlertCommand command, CancellationToken cancellationToken)
    {
        var alert = await _repository.GetByIdAsync(command.AlertId, cancellationToken);
        if (alert == null)
        {
            throw new KeyNotFoundException("ALERT_NOT_FOUND");
        }

        alert.IsResolved = true;
        alert.ResolvedDate = DateTime.UtcNow;
        alert.ResolvedBy = command.Request.ResolvedBy;
        alert.ResolutionNotes = command.Request.ResolutionNotes;

        await _repository.UpdateAsync(alert, cancellationToken);

        return true;
    }
}
