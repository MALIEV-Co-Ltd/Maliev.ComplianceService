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
public class WorkAuthorizationRepositoryTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public WorkAuthorizationRepositoryTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateWorkAuthorization()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var entity = new WorkAuthorization
        {
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "ADD-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            IssuingAuthority = "USCIS",
            SponsorshipStatus = SponsorshipStatus.Sponsored,
            ComplianceStatus = ComplianceStatus.Compliant
        };

        var result = await repository.AddAsync(entity);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(default, result.CreatedDate);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_ShouldReturnEmployeeAuthorizations()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var entity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "EMP-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        await repository.AddAsync(entity);

        var results = await repository.GetByEmployeeIdAsync(employeeId);

        Assert.Single(results);
        Assert.Equal(employeeId, results.First().EmployeeId);
    }

    [Fact]
    public async Task GetExpiringWithinDaysAsync_ShouldReturnExpiringAuthorizations()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var expiringEntity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "EXP-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-60),
            ExpirationDate = DateTime.UtcNow.AddDays(15),
            ComplianceStatus = ComplianceStatus.ExpiringSoon
        };
        await repository.AddAsync(expiringEntity);

        var notExpiringEntity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "EXP-TEST-002",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        await repository.AddAsync(notExpiringEntity);

        var results = await repository.GetExpiringWithinDaysAsync(30);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.DocumentNumber == "EXP-TEST-001");
    }

    [Fact]
    public async Task GetExpiredAsync_ShouldReturnExpiredAuthorizations()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var entity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "EXPR-TEST-001",
            IssueDate = DateTime.UtcNow.AddYears(-2),
            ExpirationDate = DateTime.UtcNow.AddDays(-10),
            ComplianceStatus = ComplianceStatus.Expired
        };
        await repository.AddAsync(entity);

        var results = await repository.GetExpiredAsync();

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task GetByComplianceStatusAsync_ShouldFilterByStatus()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var compliantEntity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "STAT-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        await repository.AddAsync(compliantEntity);

        var expiredEntity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "STAT-TEST-002",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            ComplianceStatus = ComplianceStatus.Expired
        };
        await repository.AddAsync(expiredEntity);

        var results = await repository.GetByComplianceStatusAsync(ComplianceStatus.Compliant);

        Assert.Contains(results, r => r.DocumentNumber == "STAT-TEST-001");
        Assert.DoesNotContain(results, r => r.DocumentNumber == "STAT-TEST-002");
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyWorkAuthorization()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var entity = new WorkAuthorization
        {
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "UPD-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(100),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        var created = await repository.AddAsync(entity);

        var newExpirationDate = DateTime.UtcNow.AddDays(200);
        created.ExpirationDate = newExpirationDate;
        created.ComplianceStatus = ComplianceStatus.ExpiringSoon;

        var updated = await repository.UpdateAsync(created);

        Assert.Equal(newExpirationDate.Date, updated.ExpirationDate?.Date);
    }

    [Fact]
    public async Task HasActiveAuthorizationAsync_ShouldCheckForDuplicates()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var entity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DUP-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        await repository.AddAsync(entity);

        var hasActive = await repository.HasActiveAuthorizationAsync(employeeId, AuthorizationType.WorkVisa);

        Assert.True(hasActive);
    }

    [Fact]
    public async Task GetComplianceStatsAsync_ShouldReturnStatusCounts()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        var repository = new WorkAuthorizationRepository(context);

        var employeeId = Guid.NewGuid();
        var entity = new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "STAT-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ComplianceStatus = ComplianceStatus.Compliant
        };
        await repository.AddAsync(entity);

        var stats = await repository.GetComplianceStatsAsync();

        Assert.True(stats.ContainsKey(ComplianceStatus.Compliant));
    }
}
