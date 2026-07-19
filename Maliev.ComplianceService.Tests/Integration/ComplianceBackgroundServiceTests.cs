using Maliev.ComplianceService.Infrastructure.BackgroundServices;
using Maliev.ComplianceService.Infrastructure.Data;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration;

[Collection("IntegrationTests")]
public class ComplianceBackgroundServiceTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public ComplianceBackgroundServiceTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessExpiredAsync_ShouldFlagExpiredAuthorizations()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();

        await context.ComplianceAlerts.ExecuteDeleteAsync();
        await context.WorkAuthorizations.ExecuteDeleteAsync();

        var expiredAuth = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            ExpirationDate = DateTime.UtcNow.AddDays(-5),
            ComplianceStatus = ComplianceStatus.Compliant,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            DocumentNumber = "REF123"
        };
        context.WorkAuthorizations.Add(expiredAuth);
        await context.SaveChangesAsync();

        var flaggingService = _fixture.Services.GetRequiredService<ExpiredWorkAuthorizationFlaggingService>();

        // Act
        await flaggingService.ProcessExpiredAsync(default);

        // Assert
        using var assertScope = _fixture.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var updatedAuth = await assertContext.WorkAuthorizations.AsNoTracking().FirstOrDefaultAsync(w => w.Id == expiredAuth.Id);
        Assert.Equal(ComplianceStatus.Expired, updatedAuth!.ComplianceStatus);
    }

    [Fact]
    public async Task ProcessExpirationsAsync_ShouldGenerateAlertsForExpiringAuthorizations()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();

        await context.ComplianceAlerts.ExecuteDeleteAsync();
        await context.WorkAuthorizations.ExecuteDeleteAsync();

        var expiringAuth = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            ExpirationDate = DateTime.UtcNow.AddDays(25),
            ComplianceStatus = ComplianceStatus.Compliant,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            DocumentNumber = "EXP25"
        };
        context.WorkAuthorizations.Add(expiringAuth);
        await context.SaveChangesAsync();

        var reminderService = _fixture.Services.GetRequiredService<WorkAuthorizationExpirationReminderService>();

        // Act
        await reminderService.ProcessExpirationsAsync(default);

        // Assert
        using var assertScope = _fixture.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<ComplianceDbContext>();

        var alert = await assertContext.ComplianceAlerts.FirstOrDefaultAsync(a => a.WorkAuthorizationId == expiringAuth.Id);
        Assert.NotNull(alert);
        Assert.Equal(AlertType.ExpirationWarning, alert.AlertType);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);

        var updatedAuth = await assertContext.WorkAuthorizations.FindAsync(expiringAuth.Id);
        Assert.Equal(30, updatedAuth!.LastExpirationAlertThreshold);
    }
}
