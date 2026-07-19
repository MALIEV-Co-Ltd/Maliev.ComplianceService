using Maliev.ComplianceService.Application.Queries.GetEmployeeWorkAuthorizations;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Queries;

public class GetEmployeeWorkAuthorizationsQueryHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly GetEmployeeWorkAuthorizationsQueryHandler _handler;

    public GetEmployeeWorkAuthorizationsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _handler = new GetEmployeeWorkAuthorizationsQueryHandler(_repositoryMock.Object, _employeeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidEmployee_ReturnsAuthorizations()
    {
        var employeeId = Guid.NewGuid();
        var authorizations = new List<WorkAuthorization>
        {
            new WorkAuthorization
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                AuthorizationType = AuthorizationType.WorkVisa,
                DocumentNumber = "DOC-EMP-001",
                ComplianceStatus = ComplianceStatus.Compliant
            },
            new WorkAuthorization
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                AuthorizationType = AuthorizationType.Citizen,
                DocumentNumber = "DOC-EMP-002",
                ComplianceStatus = ComplianceStatus.Compliant
            }
        };

        _repositoryMock.Setup(r => r.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorizations);

        _employeeServiceMock.Setup(s => s.GetEmployeeNameAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test Employee");

        var query = new GetEmployeeWorkAuthorizationsQuery(employeeId);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_NoAuthorizations_ReturnsEmptyList()
    {
        var employeeId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkAuthorization>());

        var query = new GetEmployeeWorkAuthorizationsQuery(employeeId);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Empty(result);
    }
}
