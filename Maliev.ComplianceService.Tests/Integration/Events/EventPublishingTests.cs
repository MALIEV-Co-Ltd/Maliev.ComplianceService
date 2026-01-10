using Maliev.ComplianceService.Domain.Events;
using Maliev.ComplianceService.Tests.Fixtures;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Events;

[Collection("IntegrationTests")]
public class EventPublishingTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public EventPublishingTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Service_CanPublishEvents()
    {
        // Arrange
        var harness = _fixture.Services.GetRequiredService<ITestHarness>();

        // Act
        await harness.Bus.Publish(new WorkAuthorizationExpiringEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Maliev.ComplianceService.Domain.Enums.AuthorizationType.WorkVisa,
            DateTime.UtcNow.AddDays(30),
            30,
            DateTime.UtcNow
        ));

        // Assert
        Assert.True(await harness.Published.Any<WorkAuthorizationExpiringEvent>());
    }
}
