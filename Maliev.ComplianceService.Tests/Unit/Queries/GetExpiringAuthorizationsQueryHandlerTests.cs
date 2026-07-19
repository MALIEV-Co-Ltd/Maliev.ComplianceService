using Maliev.ComplianceService.Application.Queries.GetExpiringAuthorizations;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Queries;

public class GetExpiringAuthorizationsQueryHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly GetExpiringAuthorizationsQueryHandler _handler;

    public GetExpiringAuthorizationsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _handler = new GetExpiringAuthorizationsQueryHandler(_repositoryMock.Object, _employeeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsExpiringAuthorizations()
    {
        // Arrange
        var days = 90;
        var query = new GetExpiringAuthorizationsQuery(days);
        var authorizations = new List<WorkAuthorization>
        {
            new WorkAuthorization
            {
                Id = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                DocumentNumber = "DOC1",
                ExpirationDate = DateTime.UtcNow.AddDays(10),
                ComplianceStatus = ComplianceStatus.ExpiringSoon
            }
        };

        _repositoryMock.Setup(r => r.GetExpiringWithinDaysAsync(days, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorizations);

        _employeeServiceMock.Setup(s => s.GetEmployeeNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("John Doe");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var item = result.First();
        Assert.Equal("John Doe", item.EmployeeName);
        Assert.Equal("DOC1", item.DocumentNumber);
    }
}
