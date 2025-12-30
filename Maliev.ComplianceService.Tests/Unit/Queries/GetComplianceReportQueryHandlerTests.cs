using Maliev.ComplianceService.Application.Queries.GetComplianceReport;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Queries;

public class GetComplianceReportQueryHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly Mock<IComplianceAlertRepository> _alertRepositoryMock;
    private readonly GetComplianceReportQueryHandler _handler;

    public GetComplianceReportQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _alertRepositoryMock = new Mock<IComplianceAlertRepository>();
        _handler = new GetComplianceReportQueryHandler(_repositoryMock.Object, _alertRepositoryMock.Object, null!);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsComplianceReport()
    {
        // Arrange
        var query = new GetComplianceReportQuery();
        
        _repositoryMock.Setup(r => r.GetByComplianceStatusAsync(ComplianceStatus.Compliant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Maliev.ComplianceService.Domain.Entities.WorkAuthorization> { new() });

        _alertRepositoryMock.Setup(r => r.GetAlertsAsync(false, null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Maliev.ComplianceService.Domain.Entities.ComplianceAlert>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Compliant);
    }
}
