using Maliev.ComplianceService.Application.Interfaces;
using Maliev.EmployeeService.Domain.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeTerminatedIntegrationEvent to deactivate work authorizations.
/// </summary>
public class EmployeeTerminatedEventConsumer : IConsumer<EmployeeTerminatedIntegrationEvent>
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
    public async Task Consume(ConsumeContext<EmployeeTerminatedIntegrationEvent> context)
    {
        _logger.LogInformation("Processing EmployeeTerminatedIntegrationEvent for Employee: {EmployeeId}", context.Message.EmployeeId);
        await _repository.DeactivateWorkAuthorizationsAsync(context.Message.EmployeeId);
        _logger.LogInformation("Deactivated work authorizations for terminated employee: {EmployeeId}", context.Message.EmployeeId);
    }
}