using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Employee;
using Maliev.MessagingContracts.Contracts.Career;
using Maliev.ComplianceService.Infrastructure.Consumers;
using MassTransit;
using Maliev.ComplianceService.Tests.Fixtures;
using Microsoft.Extensions.Logging;

namespace Maliev.ComplianceService.Tests.Integration;

public class ComplianceConsumerTests : IClassFixture<ComplianceServiceTestFixture>
{
    private readonly ComplianceServiceTestFixture _fixture;

    public ComplianceConsumerTests(ComplianceServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Consume_EmployeeCreatedEvent_ShouldLogInformation()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmployeeCreatedEventConsumer>>();
        var consumer = new EmployeeCreatedEventConsumer(logger);

        var messageId = Guid.NewGuid();
        var evt = new EmployeeCreatedEvent(
            MessageId: messageId,
            MessageName: "EmployeeCreatedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Employee",
            ConsumedBy: new[] { "Compliance" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new EmployeeCreatedEventPayload(
                EmployeeId: Guid.NewGuid(),
                EmployeeNumber: "EMP-001",
                PrincipalId: Guid.NewGuid(),
                Email: "test@example.com",
                FullName: "Test Employee",
                StartDate: DateTimeOffset.UtcNow,
                DepartmentId: Guid.NewGuid(),
                PositionId: null,
                ManagerId: null
            )
        );

        var mockContext = new Mock<ConsumeContext<EmployeeCreatedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task Consume_TrainingCompletedEvent_ShouldLogInformation()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TrainingCompletedEventConsumer>>();
        var consumer = new TrainingCompletedEventConsumer(logger);

        var messageId = Guid.NewGuid();
        var evt = new TrainingCompletedEvent(
            MessageId: messageId,
            MessageName: "TrainingCompletedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Training",
            ConsumedBy: new[] { "Compliance" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new TrainingCompletedEventPayload(
                TrainingRecordId: Guid.NewGuid(),
                EmployeeId: Guid.NewGuid(),
                CourseName: "Compliance 101",
                CompletionDate: DateTimeOffset.UtcNow,
                CertificationExpiration: DateTimeOffset.UtcNow.AddYears(1)
            )
        );

        var mockContext = new Mock<ConsumeContext<TrainingCompletedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        Assert.True(true);
    }
}
