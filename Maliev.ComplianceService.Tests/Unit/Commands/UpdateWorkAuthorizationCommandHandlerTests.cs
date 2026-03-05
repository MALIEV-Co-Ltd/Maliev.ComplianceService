using Maliev.ComplianceService.Application.Commands.UpdateWorkAuthorization;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class UpdateWorkAuthorizationCommandHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly UpdateWorkAuthorizationCommandHandler _handler;

    public UpdateWorkAuthorizationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _handler = new UpdateWorkAuthorizationCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesWorkAuthorization()
    {
        // Arrange
        var authId = Guid.NewGuid();
        var existingAuth = new WorkAuthorization { Id = authId, Xmin = 100 };
        var request = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(200),
            Xmin = 100
        };
        var command = new UpdateWorkAuthorizationCommand(authId, request);

        _repositoryMock.Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAuth);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WorkAuthorization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkAuthorization w, CancellationToken c) => w);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ComplianceStatus.Compliant, result.ComplianceStatus);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkAuthorization>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AuthorizationNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var authId = Guid.NewGuid();
        var request = new UpdateWorkAuthorizationRequest();
        var command = new UpdateWorkAuthorizationCommand(authId, request);

        _repositoryMock.Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkAuthorization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_StaleXmin_ThrowsConcurrencyException()
    {
        // Arrange
        var authId = Guid.NewGuid();
        var existingAuth = new WorkAuthorization { Id = authId, Xmin = 100 };
        var request = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(200),
            Xmin = 50
        };
        var command = new UpdateWorkAuthorizationCommand(authId, request);

        _repositoryMock.Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAuth);

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ZeroXminInEntity_AllowsUpdate()
    {
        // Arrange
        var authId = Guid.NewGuid();
        var existingAuth = new WorkAuthorization { Id = authId, Xmin = 0 };
        var request = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(200),
            Xmin = null
        };
        var command = new UpdateWorkAuthorizationCommand(authId, request);

        _repositoryMock.Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAuth);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WorkAuthorization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkAuthorization w, CancellationToken c) => w);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkAuthorization>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
