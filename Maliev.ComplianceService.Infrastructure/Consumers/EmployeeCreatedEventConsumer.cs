using Maliev.EmployeeService.Domain.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeCreatedIntegrationEvent to maintain employee context.
/// </summary>
public class EmployeeCreatedEventConsumer : IConsumer<EmployeeCreatedIntegrationEvent>
{
    private readonly ILogger<EmployeeCreatedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeCreatedEventConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public EmployeeCreatedEventConsumer(ILogger<EmployeeCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Processing EmployeeCreatedIntegrationEvent for Employee: {EmployeeId}", context.Message.EmployeeId);
        return Task.CompletedTask;
    }
}