using Maliev.MessagingContracts.Generated;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeCreatedEvent to maintain employee context.
/// </summary>
public class EmployeeCreatedEventConsumer : IConsumer<EmployeeCreatedEvent>
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
    public Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        _logger.LogInformation("Processing EmployeeCreatedEvent for Employee: {EmployeeId}", context.Message.Payload.EmployeeId);
        return Task.CompletedTask;
    }
}