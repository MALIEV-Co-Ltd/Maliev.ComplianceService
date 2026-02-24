using Maliev.MessagingContracts.Contracts.Career;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.Consumers;

/// <summary>
/// Consumes TrainingCompletedEvent for future certification tracking.
/// </summary>
public class TrainingCompletedEventConsumer : IConsumer<TrainingCompletedEvent>
{
    private readonly ILogger<TrainingCompletedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrainingCompletedEventConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public TrainingCompletedEventConsumer(ILogger<TrainingCompletedEventConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task Consume(ConsumeContext<TrainingCompletedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation("Processing TrainingCompletedEvent for Employee: {EmployeeId}, Course: {CourseName}",
            payload.EmployeeId, payload.CourseName);
        // FR-021: Stub for future certification tracking
        return Task.CompletedTask;
    }
}
