using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Mappers;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Mappers;

public class DtoMapperTests
{
    [Fact]
    public void ToDto_WithEmployeeName_MapsCorrectly()
    {
        var entity = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DTO-TEST-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            IssuingAuthority = "USCIS",
            SponsorshipStatus = SponsorshipStatus.Sponsored,
            ComplianceStatus = ComplianceStatus.Compliant,
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            ModifiedDate = DateTime.UtcNow,
            RowVersion = Guid.NewGuid()
        };

        var result = DtoMapper.ToDto(entity, "Test Employee");

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.EmployeeId, result.EmployeeId);
        Assert.Equal("Test Employee", result.EmployeeName);
        Assert.Equal(AuthorizationType.WorkVisa, result.AuthorizationType);
        Assert.Equal("DTO-TEST-001", result.DocumentNumber);
        Assert.Equal(entity.IssueDate, result.IssueDate);
        Assert.Equal(entity.ExpirationDate, result.ExpirationDate);
        Assert.NotNull(result.DaysUntilExpiration);
        Assert.Equal("USCIS", result.IssuingAuthority);
        Assert.Equal(SponsorshipStatus.Sponsored, result.SponsorshipStatus);
        Assert.Equal(ComplianceStatus.Compliant, result.ComplianceStatus);
    }

    [Fact]
    public void ToDto_WithoutEmployeeName_UsesDefaultName()
    {
        var entity = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "DTO-TEST-002"
        };

        var result = DtoMapper.ToDto(entity);

        Assert.Equal("(name unavailable)", result.EmployeeName);
    }

    [Fact]
    public void ToDto_NullExpirationDate_DaysUntilExpirationIsNull()
    {
        var entity = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            ExpirationDate = null
        };

        var result = DtoMapper.ToDto(entity);

        Assert.Null(result.DaysUntilExpiration);
    }

    [Fact]
    public void ToExpiringDto_MapsCorrectly()
    {
        var entity = new WorkAuthorization
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DTO-EXP-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            ComplianceStatus = ComplianceStatus.ExpiringSoon,
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            ModifiedDate = DateTime.UtcNow,
            RowVersion = Guid.NewGuid()
        };

        var result = DtoMapper.ToExpiringDto(entity, "Expiring Employee");

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Expiring Employee", result.EmployeeName);
        Assert.Equal(ComplianceStatus.ExpiringSoon, result.ComplianceStatus);
    }

    [Fact]
    public void ToAlertDto_MapsCorrectly()
    {
        var entity = new ComplianceAlert
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AlertType = AlertType.ExpirationWarning,
            Severity = AlertSeverity.High,
            Message = "Test alert message",
            IsResolved = false,
            ResolutionNotes = "Fix in progress",
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = DtoMapper.ToAlertDto(entity, "Alert Employee");

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.EmployeeId, result.EmployeeId);
        Assert.Equal("Alert Employee", result.EmployeeName);
        Assert.Equal(AlertType.ExpirationWarning, result.AlertType);
        Assert.Equal(AlertSeverity.High, result.Severity);
        Assert.Equal("Test alert message", result.Message);
        Assert.False(result.IsResolved);
        Assert.Equal("Fix in progress", result.ResolutionNotes);
    }

    [Fact]
    public void ToAlertDto_WithoutEmployeeName_UsesDefaultName()
    {
        var entity = new ComplianceAlert
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AlertType = AlertType.Expired,
            Severity = AlertSeverity.Critical,
            Message = "Expired"
        };

        var result = DtoMapper.ToAlertDto(entity);

        Assert.Equal("(name unavailable)", result.EmployeeName);
    }

    [Fact]
    public void ToAlertDto_ResolvedAlert_HasResolvedDate()
    {
        var resolvedDate = DateTime.UtcNow.AddHours(-2);
        var entity = new ComplianceAlert
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            AlertType = AlertType.ExpirationWarning,
            Severity = AlertSeverity.Medium,
            Message = "Resolved alert",
            IsResolved = true,
            ResolvedDate = resolvedDate
        };

        var result = DtoMapper.ToAlertDto(entity, "Resolved Employee");

        Assert.True(result.IsResolved);
        Assert.Equal(resolvedDate, result.ResolvedDate);
    }
}
