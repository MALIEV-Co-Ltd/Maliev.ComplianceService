using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs daily to identify work authorizations approaching expiration
/// and generates compliance alerts and events at 30, 60, and 90-day thresholds.
/// </summary>
public class WorkAuthorizationExpirationReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkAuthorizationExpirationReminderService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkAuthorizationExpirationReminderService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public WorkAuthorizationExpirationReminderService(
        IServiceProvider serviceProvider, 
        ILogger<WorkAuthorizationExpirationReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Work Authorization Expiration Reminder Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpirationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing work authorization expirations.");
            }

            // Run daily (or more frequently for demo/test)
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ProcessExpirationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkAuthorizationRepository>();
        var alertRepository = scope.ServiceProvider.GetRequiredService<IComplianceAlertRepository>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var thresholds = new[] { 30, 60, 90 };
        foreach (var days in thresholds)
        {
            var authorizations = await repository.GetExpiringWithinDaysAsync(days, cancellationToken);
            
            foreach (var auth in authorizations)
            {
                if (!auth.ExpirationDate.HasValue) continue;

                var remainingDays = (auth.ExpirationDate.Value.Date - DateTime.UtcNow.Date).Days;
                if (remainingDays != days) continue;

                // Check if alert already exists for this threshold
                var alertType = AlertType.ExpirationWarning;
                if (await alertRepository.HasUnresolvedAlertAsync(auth.Id, alertType, cancellationToken))
                {
                    // Optionally check message or metadata to avoid duplicates for SAME threshold
                    // For now, let's assume we want to avoid multiple "ExpirationWarning" if one is already open
                    continue;
                }

                var severity = days switch
                {
                    30 => AlertSeverity.Critical,
                    60 => AlertSeverity.High,
                    _ => AlertSeverity.Medium
                };

                var alert = new ComplianceAlert
                {
                    WorkAuthorizationId = auth.Id,
                    EmployeeId = auth.EmployeeId,
                    AlertType = alertType,
                    Severity = severity,
                    Message = $"Work authorization {auth.AuthorizationType} expires in {days} days on {auth.ExpirationDate:yyyy-MM-dd}",
                    CreatedDate = DateTime.UtcNow
                };

                await alertRepository.CreateAsync(alert, cancellationToken);

                // Publish event
                await publishEndpoint.Publish(new WorkAuthorizationExpiringEvent(
                    auth.Id,
                    auth.EmployeeId,
                    auth.AuthorizationType,
                    auth.ExpirationDate.Value,
                    days,
                    DateTime.UtcNow
                ), cancellationToken);

                _logger.LogInformation("Generated {Days} day expiration alert for authorization {AuthId}", days, auth.Id);
            }
        }
    }
}
