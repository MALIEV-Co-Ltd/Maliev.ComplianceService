using Maliev.ComplianceService.Application.Queries.GetWorkAuthorization;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Queries;

public class GetWorkAuthorizationQueryHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly GetWorkAuthorizationQueryHandler _handler;

    public GetWorkAuthorizationQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _employeeServiceMock = new Mock<IEmployeeService>();
        _handler = new GetWorkAuthorizationQueryHandler(_repositoryMock.Object, _employeeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingId_ReturnsAuthorization()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var entity = new WorkAuthorization
        {
            Id = id,
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DOC-001",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            ComplianceStatus = ComplianceStatus.Compliant
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _employeeServiceMock.Setup(s => s.GetEmployeeNameAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Jane Doe");

        var query = new GetWorkAuthorizationQuery(id);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Jane Doe", result.EmployeeName);
        Assert.Equal("DOC-001", result.DocumentNumber);
    }

    [Fact]
    public async Task Handle_NonExistingId_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkAuthorization?)null);

        var query = new GetWorkAuthorizationQuery(id);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
