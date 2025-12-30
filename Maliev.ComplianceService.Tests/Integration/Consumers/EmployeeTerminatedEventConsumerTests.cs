using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.MessagingContracts.Generated;
using Maliev.ComplianceService.Infrastructure.Consumers;
using Maliev.ComplianceService.Tests.Fixtures;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.ComplianceService.Tests.Integration.Consumers;

[Collection("IntegrationTests")]
public class EmployeeTerminatedEventConsumerTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public EmployeeTerminatedEventConsumerTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Consume_EmployeeTerminatedEvent_DeactivatesAuthorizations()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        using var scope = _fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkAuthorizationRepository>();
        
        await repository.AddAsync(new WorkAuthorization
        {
            EmployeeId = employeeId,
            AuthorizationType = AuthorizationType.Citizen,
            DocumentNumber = "TERM-001",
            IssueDate = DateTime.UtcNow.AddYears(-1),
            IsActive = true
        });

        // Use a real harness if possible, but here we can just invoke the consumer or use the background bus
        var harness = _fixture.Services.GetRequiredService<ITestHarness>();

        // Act
        var payload = new EmployeeTerminatedEventPayload(
            EmployeeId: employeeId,
            TerminationDate: DateTimeOffset.UtcNow,
            TerminationReason: "Test termination",
            EligibleForRehire: false
        );

        var @event = new EmployeeTerminatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "EmployeeTerminated",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "EmployeeService",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: payload
        );

        await harness.Bus.Publish(@event);

        // Assert
        Assert.True(await harness.Published.Any<EmployeeTerminatedEvent>());
        
        // Give some time for consumer to process
        await Task.Delay(1000);

        var authorizations = await repository.GetByEmployeeIdAsync(employeeId);
        Assert.Empty(authorizations); // repository.GetByEmployeeIdAsync filters by IsActive=true
    }
}
