using Maliev.ComplianceService.Application.Commands.ResolveAlert;
using Maliev.ComplianceService.Application.DTOs;
using Xunit;

namespace Maliev.ComplianceService.Tests.Unit.Commands;

public class ResolveAlertValidatorTests
{
    private readonly ResolveAlertValidator _validator;

    public ResolveAlertValidatorTests()
    {
        _validator = new ResolveAlertValidator();
    }

    [Fact]
    public void Validate_ValidNotes_ReturnsTrue()
    {
        var request = new ResolveAlertRequest
        {
            ResolutionNotes = "Issue resolved by updating work authorization"
        };
        var command = new ResolveAlertCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.True(result);
    }

    [Fact]
    public void Validate_ShortNotes_ReturnsFalse()
    {
        var request = new ResolveAlertRequest
        {
            ResolutionNotes = "Short"
        };
        var command = new ResolveAlertCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.False(result);
    }

    [Fact]
    public void Validate_NullNotes_ReturnsFalse()
    {
        var request = new ResolveAlertRequest
        {
            ResolutionNotes = null!
        };
        var command = new ResolveAlertCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.False(result);
    }

    [Fact]
    public void Validate_EmptyNotes_ReturnsFalse()
    {
        var request = new ResolveAlertRequest
        {
            ResolutionNotes = string.Empty
        };
        var command = new ResolveAlertCommand(Guid.NewGuid(), request);

        var result = _validator.Validate(command);

        Assert.False(result);
    }
}
