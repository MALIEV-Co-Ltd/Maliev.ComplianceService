using Maliev.ComplianceService.Application.Commands.UpdateWorkAuthorization;
using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Domain.Enums;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class UpdateWorkAuthorizationValidatorTests
{
    private readonly UpdateWorkAuthorizationValidator _validator;

    public UpdateWorkAuthorizationValidatorTests()
    {
        _validator = new UpdateWorkAuthorizationValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsTrue()
    {
        var request = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            Notes = "Updated notes",
            RowVersion = new byte[] { 1 }
        };
        var command = new UpdateWorkAuthorizationCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.True(result);
    }

    [Fact]
    public void Validate_MissingExpirationDate_ReturnsTrue()
    {
        var request = new UpdateWorkAuthorizationRequest
        {
            ExpirationDate = null,
            RowVersion = new byte[] { 1 }
        };
        var command = new UpdateWorkAuthorizationCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.True(result);
    }
}
