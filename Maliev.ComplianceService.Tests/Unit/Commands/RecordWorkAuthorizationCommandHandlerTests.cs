using Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class RecordWorkAuthorizationCommandHandlerTests
{
    private readonly Mock<IWorkAuthorizationRepository> _repositoryMock;
    private readonly RecordWorkAuthorizationCommandHandler _handler;

    public RecordWorkAuthorizationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWorkAuthorizationRepository>();
        _handler = new RecordWorkAuthorizationCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesWorkAuthorization()
    {
        // Arrange
        var request = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DOC123",
            IssueDate = DateTime.UtcNow.AddDays(-10),
            ExpirationDate = DateTime.UtcNow.AddDays(100),
            IssuingAuthority = "USCIS",
            SponsorshipStatus = SponsorshipStatus.Sponsored,
            RightToWorkDocumentId = Guid.NewGuid(),
            Notes = "Test notes"
        };
        var command = new RecordWorkAuthorizationCommand(Guid.NewGuid(), request);

        _repositoryMock.Setup(r => r.HasActiveAuthorizationAsync(command.EmployeeId, request.AuthorizationType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<WorkAuthorization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkAuthorization w, CancellationToken c) => w);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.DocumentNumber, result.DocumentNumber);
        Assert.Equal(ComplianceStatus.Compliant, result.ComplianceStatus);
        
        _repositoryMock.Verify(r => r.AddAsync(It.Is<WorkAuthorization>(w => 
            w.EmployeeId == command.EmployeeId &&
            w.DocumentNumber == request.DocumentNumber &&
            w.ComplianceStatus == ComplianceStatus.Compliant
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateAuthorization_ThrowsException()
    {
        // Arrange
        var request = new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "DUP123",
            IssueDate = DateTime.UtcNow.AddDays(-10),
            ExpirationDate = DateTime.UtcNow.AddDays(100),
            RightToWorkDocumentId = Guid.NewGuid()
        };
        var command = new RecordWorkAuthorizationCommand(Guid.NewGuid(), request);

        _repositoryMock.Setup(r => r.HasActiveAuthorizationAsync(command.EmployeeId, request.AuthorizationType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
