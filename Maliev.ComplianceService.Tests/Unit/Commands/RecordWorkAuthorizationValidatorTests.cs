using Maliev.ComplianceService.Application.Commands.RecordWorkAuthorization;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class RecordWorkAuthorizationValidatorTests
{
    private readonly RecordWorkAuthorizationValidator _validator;

    public RecordWorkAuthorizationValidatorTests()
    {
        _validator = new RecordWorkAuthorizationValidator();
    }

    [Fact]
    public void Validate_IssueDateAfterExpirationDate_ReturnsError()
    {
        // Arrange
        var command = new RecordWorkAuthorizationCommand(Guid.NewGuid(), new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "123",
            IssueDate = DateTime.UtcNow.AddDays(10),
            ExpirationDate = DateTime.UtcNow
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "INVALID_DATE_RANGE");
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var command = new RecordWorkAuthorizationCommand(Guid.NewGuid(), new RecordWorkAuthorizationRequest
        {
            AuthorizationType = AuthorizationType.WorkVisa,
            DocumentNumber = "123",
            IssueDate = DateTime.UtcNow.AddDays(-10),
            ExpirationDate = DateTime.UtcNow.AddDays(10),
            RightToWorkDocumentId = Guid.NewGuid()
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
