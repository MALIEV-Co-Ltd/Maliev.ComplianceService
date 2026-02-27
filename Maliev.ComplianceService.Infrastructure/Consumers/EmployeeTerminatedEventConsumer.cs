using Maliev.ComplianceService.Application.Interfaces;
using Maliev.MessagingContracts.Contracts.Employee;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeTerminatedEvent to deactivate work authorizations.
/// </summary>
public class EmployeeTerminatedEventConsumer : IConsumer<EmployeeTerminatedEvent>
{
    private readonly IWorkAuthorizationRepository _repository;
    private readonly ILogger<EmployeeTerminatedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeTerminatedEventConsumer"/> class.
    /// </summary>
    /// <param name="repository">The work authorization repository.</param>
    /// <param name="logger">The logger.</param>
    public EmployeeTerminatedEventConsumer(
        IWorkAuthorizationRepository repository,
        ILogger<EmployeeTerminatedEventConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<EmployeeTerminatedEvent> context)
    {
        _logger.LogInformation("Processing EmployeeTerminatedEvent for Employee: {EmployeeId}", context.Message.Payload.EmployeeId);
        await _repository.DeactivateWorkAuthorizationsAsync(context.Message.Payload.EmployeeId);
        _logger.LogInformation("Deactivated work authorizations for terminated employee: {EmployeeId}", context.Message.Payload.EmployeeId);
    }
}
