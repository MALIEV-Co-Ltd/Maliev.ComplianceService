using Maliev.MessagingContracts.Contracts.Compliance;
using Maliev.MessagingContracts;
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
            MessageId: Guid.NewGuid(),
            MessageName: nameof(WorkAuthorizationExpiringEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new WorkAuthorizationExpiringEventPayload(
                AuthorizationId: Guid.NewGuid(),
                EmployeeId: Guid.NewGuid(),
                AuthorizationType: "WorkVisa",
                ExpirationDate: DateTimeOffset.UtcNow.AddDays(30),
                DaysUntilExpiration: 30
            )
        ));

        // Assert
        Assert.True(await harness.Published.Any<WorkAuthorizationExpiringEvent>());
    }
}
