using Maliev.ComplianceService.Application.Commands.ResolveAlert;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class ResolveAlertCommandHandlerTests
{
    private readonly Mock<IComplianceAlertRepository> _repositoryMock;
    private readonly ResolveAlertCommandHandler _handler;

    public ResolveAlertCommandHandlerTests()
    {
        _repositoryMock = new Mock<IComplianceAlertRepository>();
        _handler = new ResolveAlertCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ResolvesAlert()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var existingAlert = new ComplianceAlert { Id = alertId, IsResolved = false };
        var request = new ResolveAlertRequest
        {
            ResolvedBy = Guid.NewGuid(),
            ResolutionNotes = "Resolved for testing purposes."
        };
        var command = new ResolveAlertCommand(alertId, request);

        _repositoryMock.Setup(r => r.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAlert);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(existingAlert.IsResolved);
        Assert.Equal(request.ResolvedBy, existingAlert.ResolvedBy);
        Assert.Equal(request.ResolutionNotes, existingAlert.ResolutionNotes);
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<ComplianceAlert>(a => a.Id == alertId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
