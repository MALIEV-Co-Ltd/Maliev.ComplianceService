using Maliev.ComplianceService.Application.Queries.GetAlerts;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Queries;

public class GetAlertsQueryHandlerTests
{
    private readonly Mock<IComplianceAlertRepository> _repositoryMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly GetAlertsQueryHandler _handler;

    public GetAlertsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IComplianceAlertRepository>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _handler = new GetAlertsQueryHandler(_repositoryMock.Object, _employeeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsAlerts()
    {
        var alerts = new List<ComplianceAlert>
        {
            new ComplianceAlert
            {
                Id = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                AlertType = AlertType.ExpirationWarning,
                Severity = AlertSeverity.High,
                Message = "Test alert",
                IsResolved = false,
                CreatedDate = DateTime.UtcNow
            }
        };

        _repositoryMock.Setup(r => r.GetAlertsAsync(
            It.IsAny<bool?>(),
            It.IsAny<AlertSeverity?>(),
            It.IsAny<Guid?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<AlertType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        _employeeServiceMock.Setup(s => s.GetEmployeeNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("John Doe");

        var query = new GetAlertsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("John Doe", result.First().EmployeeName);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesFiltersToRepository()
    {
        Guid? employeeId = Guid.NewGuid();
        var severity = AlertSeverity.Critical;
        var alertType = AlertType.Expired;
        var isResolved = false;
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        Guid? resolvedBy = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetAlertsAsync(
            isResolved,
            severity,
            employeeId,
            fromDate,
            toDate,
            alertType,
            resolvedBy,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ComplianceAlert>());

        var query = new GetAlertsQuery
        {
            IsResolved = isResolved,
            Severity = severity,
            EmployeeId = employeeId,
            FromDate = fromDate,
            ToDate = toDate,
            AlertType = alertType,
            ResolvedBy = resolvedBy
        };

        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(r => r.GetAlertsAsync(
            isResolved,
            severity,
            employeeId,
            fromDate,
            toDate,
            alertType,
            resolvedBy,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAlertsAsync(
            It.IsAny<bool?>(),
            It.IsAny<AlertSeverity?>(),
            It.IsAny<Guid?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<AlertType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ComplianceAlert>());

        var query = new GetAlertsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Empty(result);
    }
}
