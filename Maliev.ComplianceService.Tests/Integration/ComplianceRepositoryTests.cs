using Maliev.ComplianceService.Infrastructure.Data;
using Maliev.ComplianceService.Infrastructure.Repositories;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration;

[Collection("IntegrationTests")]
public class ComplianceRepositoryTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public ComplianceRepositoryTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAlertsAsync_AllFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new ComplianceAlertRepository(context);

        var employeeId = Guid.NewGuid();
        var resolvedBy = Guid.NewGuid();

        var auth = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "DOC123",
            IssueDate = DateTime.UtcNow.AddYears(-1)
        };
        context.WorkAuthorizations.Add(auth);

        var alert = new ComplianceAlert
        {
            Id = Guid.NewGuid(),
            WorkAuthorizationId = auth.Id,
            EmployeeId = employeeId,
            AlertType = AlertType.Expired,
            Severity = AlertSeverity.Critical,
            Message = "Test",
            IsResolved = true,
            ResolvedBy = resolvedBy,
            ResolvedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.ComplianceAlerts.Add(alert);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.GetAlertsAsync(
            isResolved: true,
            severity: AlertSeverity.Critical,
            employeeId: employeeId,
            fromDate: DateTime.UtcNow.AddDays(-2),
            toDate: DateTime.UtcNow.AddDays(1),
            alertType: AlertType.Expired,
            resolvedBy: resolvedBy
        );

        // Assert
        Assert.Single(results);
        Assert.Equal(alert.Id, results.First().Id);
    }

    [Fact]
    public async Task GetUnresolvedByAuthorizationIdAsync_ShouldReturnOnlyUnresolved()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new ComplianceAlertRepository(context);

        var employeeId = Guid.NewGuid();
        var authId = Guid.NewGuid();

        var auth = new WorkAuthorization
        {
            Id = authId,
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "UNRES",
            IssueDate = DateTime.UtcNow
        };
        context.WorkAuthorizations.Add(auth);

        var unresolved = new ComplianceAlert
        {
            WorkAuthorizationId = authId,
            EmployeeId = employeeId,
            AlertType = AlertType.Expired,
            Severity = AlertSeverity.Critical,
            Message = "Unresolved",
            IsResolved = false
        };
        var resolved = new ComplianceAlert
        {
            WorkAuthorizationId = authId,
            EmployeeId = employeeId,
            AlertType = AlertType.Expired,
            Severity = AlertSeverity.Critical,
            Message = "Resolved",
            IsResolved = true
        };
        context.ComplianceAlerts.AddRange(unresolved, resolved);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.GetUnresolvedByAuthorizationIdAsync(authId);

        // Assert
        Assert.Single(results);
        Assert.Equal("Unresolved", results.First().Message);
    }
}
