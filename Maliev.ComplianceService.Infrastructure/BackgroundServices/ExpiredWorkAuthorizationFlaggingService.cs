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
/// Background service that runs daily to identify work authorizations that have passed
/// their expiration date, updates their status, and generates critical alerts.
/// </summary>
public class ExpiredWorkAuthorizationFlaggingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredWorkAuthorizationFlaggingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiredWorkAuthorizationFlaggingService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public ExpiredWorkAuthorizationFlaggingService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredWorkAuthorizationFlaggingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Work Authorization Flagging Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while flagging expired work authorizations.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    internal async Task ProcessExpiredAsync(CancellationToken cancellationToken)

    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkAuthorizationRepository>();
        var alertRepository = scope.ServiceProvider.GetRequiredService<IComplianceAlertRepository>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var expiredAuthorizations = await repository.GetExpiredAsync(cancellationToken);

        foreach (var auth in expiredAuthorizations)
        {
            if (auth.ComplianceStatus == ComplianceStatus.Expired) continue;

            auth.ComplianceStatus = ComplianceStatus.Expired;
            auth.ModifiedDate = DateTime.UtcNow;
            await repository.UpdateAsync(auth, cancellationToken);

            // Create critical alert if not exists
            if (!await alertRepository.HasUnresolvedAlertAsync(auth.Id, AlertType.Expired, cancellationToken))
            {
                var alert = new ComplianceAlert
                {
                    WorkAuthorizationId = auth.Id,
                    EmployeeId = auth.EmployeeId,
                    AlertType = AlertType.Expired,
                    Severity = AlertSeverity.Critical,
                    Message = $"Work authorization {auth.AuthorizationType} expired on {auth.ExpirationDate:yyyy-MM-dd}",
                    CreatedDate = DateTime.UtcNow
                };

                await alertRepository.CreateAsync(alert, cancellationToken);
            }

            // Publish event
            if (!auth.ExpirationDate.HasValue) continue;

            var expiredDays = (DateTime.UtcNow.Date - auth.ExpirationDate.Value.Date).Days;
            await publishEndpoint.Publish(new WorkAuthorizationExpiredEvent(
                auth.Id,
                auth.EmployeeId,
                auth.AuthorizationType.ToString(),
                auth.ExpirationDate.Value,
                expiredDays,
                DateTime.UtcNow), cancellationToken);

            // FR-018: AccessRevocationRequiredEvent after 30 days
            if (expiredDays >= 30 && !auth.AccessRevocationSentDate.HasValue)
            {
                await publishEndpoint.Publish(new AccessRevocationRequiredEvent(
                    auth.EmployeeId,
                    DateTime.UtcNow,
                    "Work authorization expired for more than 30 days",
                    auth.Id,
                    expiredDays,
                    DateTime.UtcNow
                ), cancellationToken);

                auth.AccessRevocationSentDate = DateTime.UtcNow;
                await repository.UpdateAsync(auth, cancellationToken);

                _logger.LogInformation("Published AccessRevocationRequiredEvent for employee {EmployeeId}", auth.EmployeeId);
            }

            _logger.LogInformation("Flagged authorization {AuthId} as EXPIRED", auth.Id);
        }
    }
}
