# Integration Events: Compliance Service

**Date**: 2025-12-28
**Branch**: 001-compliance-service

## Events Published

### WorkAuthorizationExpiringEvent

**Exchange**: `compliance.events`
**Routing Key**: `work-authorization.expiring`
**Purpose**: Notify when authorization reaches expiration thresholds (30/60/90 days)

**Message Schema**:
```json
{
  "authorizationId": "uuid",
  "employeeId": "uuid",
  "authorizationType": "WorkVisa",
  "expirationDate": "2025-04-15T00:00:00Z",
  "daysUntilExpiration": 30,
  "timestamp": "2025-03-16T07:00:00Z",
  "version": "1.0"
}
```

**Field Descriptions**:
- `authorizationId`: UUID of the work authorization
- `employeeId`: UUID of the employee
- `authorizationType`: Type of authorization (enum string)
- `expirationDate`: When the authorization expires
- `daysUntilExpiration`: Days remaining (30, 60, or 90)
- `timestamp`: When event was published
- `version`: Event schema version

**Published By**: `WorkAuthorizationExpirationReminderService` (daily at 7 AM)

**Consumed By**: Notification Service (sends emails/SMS to HR and employee)

**Frequency**: Once per threshold per authorization (3 events total per authorization)

**Example C# Record**:
```csharp
public record WorkAuthorizationExpiringEvent(
    Guid AuthorizationId,
    Guid EmployeeId,
    AuthorizationType AuthorizationType,
    DateTime ExpirationDate,
    int DaysUntilExpiration,
    DateTime Timestamp,
    string Version = "1.0"
);
```

---

### WorkAuthorizationExpiredEvent

**Exchange**: `compliance.events`
**Routing Key**: `work-authorization.expired`
**Purpose**: Notify when authorization has passed expiration date

**Message Schema**:
```json
{
  "authorizationId": "uuid",
  "employeeId": "uuid",
  "authorizationType": "WorkVisa",
  "expirationDate": "2025-01-01T00:00:00Z",
  "expiredDays": 5,
  "timestamp": "2025-01-06T00:00:00Z",
  "version": "1.0"
}
```

**Field Descriptions**:
- `authorizationId`: UUID of the work authorization
- `employeeId`: UUID of the employee
- `authorizationType`: Type of authorization (enum string)
- `expirationDate`: When the authorization expired
- `expiredDays`: How many days past expiration
- `timestamp`: When event was published
- `version`: Event schema version

**Published By**: `ExpiredWorkAuthorizationFlaggingService` (daily at midnight)

**Consumed By**: Notification Service (critical alert), IAM Service (potential access review)

**Frequency**: Once when first detected as expired

**Example C# Record**:
```csharp
public record WorkAuthorizationExpiredEvent(
    Guid AuthorizationId,
    Guid EmployeeId,
    AuthorizationType AuthorizationType,
    DateTime ExpirationDate,
    int ExpiredDays,
    DateTime Timestamp,
    string Version = "1.0"
);
```

---

### AccessRevocationRequiredEvent

**Exchange**: `compliance.events`
**Routing Key**: `access-revocation.required`
**Purpose**: Request system access revocation for employee with expired authorization (30 days past expiration)

**Message Schema**:
```json
{
  "employeeId": "uuid",
  "effectiveDate": "2025-02-01T00:00:00Z",
  "reason": "Work authorization expired on 2025-01-01 with no renewal after 30-day grace period",
  "authorizationId": "uuid",
  "expiredSince": "2025-01-01T00:00:00Z",
  "timestamp": "2025-02-01T00:00:00Z",
  "version": "1.0"
}
```

**Field Descriptions**:
- `employeeId`: UUID of the employee
- `effectiveDate`: When access should be revoked
- `reason`: Human-readable explanation
- `authorizationId`: Work authorization that triggered revocation
- `expiredSince`: Original expiration date
- `timestamp`: When event was published
- `version`: Event schema version

**Published By**: `ExpiredWorkAuthorizationFlaggingService` (30 days after expiration)

**Consumed By**: IAM Service (revoke system access), Notification Service (final notice)

**Frequency**: Once per authorization, 30 days after expiration

**Grace Period**: 30 days from expiration date (per FR-018 and A-006)

**Example C# Record**:
```csharp
public record AccessRevocationRequiredEvent(
    Guid EmployeeId,
    DateTime EffectiveDate,
    string Reason,
    Guid AuthorizationId,
    DateTime ExpiredSince,
    DateTime Timestamp,
    string Version = "1.0"
);
```

---

## Events Consumed

### EmployeeCreatedEvent

**Exchange**: `employee.events`
**Routing Key**: `employee.created`
**Purpose**: Maintain employee context for work authorizations

**Message Schema**:
```json
{
  "employeeId": "uuid",
  "employeeNumber": "EMP-12345",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "departmentId": "uuid",
  "startDate": "2025-01-15T00:00:00Z",
  "timestamp": "2025-01-15T10:00:00Z",
  "version": "1.0"
}
```

**Published By**: Employee Service

**Consumed By**: `EmployeeCreatedEventConsumer`

**Handler Behavior**:
- Store employee ID for future reference (no local employee table)
- Log employee creation for audit trail
- No action required (work authorizations added separately via API)

**Example C# Record**:
```csharp
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    Guid DepartmentId,
    DateTime StartDate,
    DateTime Timestamp,
    string Version = "1.0"
);
```

---

### EmployeeTerminatedEvent

**Exchange**: `employee.events`
**Routing Key**: `employee.terminated`
**Purpose**: Mark work authorizations inactive for terminated employees

**Message Schema**:
```json
{
  "employeeId": "uuid",
  "terminationDate": "2025-12-31T00:00:00Z",
  "reason": "Voluntary",
  "timestamp": "2025-12-31T17:00:00Z",
  "version": "1.0"
}
```

**Published By**: Employee Service

**Consumed By**: `EmployeeTerminatedEventConsumer`

**Handler Behavior**:
1. Query all active work authorizations for `employeeId`
2. Set `is_active = FALSE` for all authorizations
3. Log termination for audit trail
4. Do NOT delete records (audit requirements)

**Example C# Record**:
```csharp
public record EmployeeTerminatedEvent(
    Guid EmployeeId,
    DateTime TerminationDate,
    string Reason,
    DateTime Timestamp,
    string Version = "1.0"
);
```

**Implementation**:
```csharp
public class EmployeeTerminatedEventConsumer : IConsumer<EmployeeTerminatedEvent>
{
    public async Task Consume(ConsumeContext<EmployeeTerminatedEvent> context)
    {
        var employeeId = context.Message.EmployeeId;

        await _repository.DeactivateWorkAuthorizationsAsync(
            employeeId,
            context.CancellationToken
        );

        _logger.LogInformation(
            "Deactivated work authorizations for terminated employee {EmployeeId}",
            employeeId
        );
    }
}
```

---

### TrainingCompletedEvent

**Exchange**: `career.events`
**Routing Key**: `training.completed`
**Purpose**: Track certifications with expiration dates (future enhancement)

**Message Schema**:
```json
{
  "employeeId": "uuid",
  "courseName": "OSHA Safety Certification",
  "completionDate": "2025-06-15T00:00:00Z",
  "certificationExpiration": "2027-06-15T00:00:00Z",
  "certificateId": "uuid",
  "timestamp": "2025-06-15T14:00:00Z",
  "version": "1.0"
}
```

**Published By**: Career Service

**Consumed By**: `TrainingCompletedEventConsumer`

**Handler Behavior** (Current - FR-021):
- Log event for audit trail
- No action taken (certification tracking not in MVP)
- Infrastructure prepared for future enhancement

**Future Enhancement**:
- Create compliance tracking for certifications
- Generate alerts for certification expirations
- Include in compliance reports

**Example C# Record**:
```csharp
public record TrainingCompletedEvent(
    Guid EmployeeId,
    string CourseName,
    DateTime CompletionDate,
    DateTime? CertificationExpiration,
    Guid CertificateId,
    DateTime Timestamp,
    string Version = "1.0"
);
```

**Implementation (Stub)**:
```csharp
public class TrainingCompletedEventConsumer : IConsumer<TrainingCompletedEvent>
{
    public async Task Consume(ConsumeContext<TrainingCompletedEvent> context)
    {
        _logger.LogInformation(
            "Training completed event received for employee {EmployeeId}: {CourseName}",
            context.Message.EmployeeId,
            context.Message.CourseName
        );

        // Future: Create certification compliance tracking
        await Task.CompletedTask;
    }
}
```

---

## Event Versioning Strategy

### Schema Evolution

**Version Format**: MAJOR.MINOR (e.g., "1.0", "2.0")

**Versioning Rules**:
- **MAJOR**: Breaking changes (field removed, type changed, required field added)
- **MINOR**: Backward-compatible changes (optional field added)

**Handling Version Mismatches**:
1. Consumer checks `version` field
2. If unknown version → log warning, attempt best-effort parse
3. If critical field missing → reject message, send to dead-letter queue
4. If extra fields present → ignore unknown fields

**Example Version Check**:
```csharp
public async Task Consume(ConsumeContext<WorkAuthorizationExpiringEvent> context)
{
    var evt = context.Message;

    if (evt.Version != "1.0")
    {
        _logger.LogWarning(
            "Unexpected event version {Version}, expected 1.0",
            evt.Version
        );
    }

    // Process event...
}
```

---

## Message Bus Configuration

### RabbitMQ Exchanges

**Exchange Name**: `compliance.events`
**Exchange Type**: Topic
**Durability**: Durable
**Auto-Delete**: False

### Queues

**Published Events**:
- Queue: `compliance.expiring-notifications` (consumed by Notification Service)
- Queue: `compliance.expired-notifications` (consumed by Notification Service)
- Queue: `compliance.access-revocation` (consumed by IAM Service)

**Consumed Events**:
- Queue: `compliance.employee-created` (from Employee Service)
- Queue: `compliance.employee-terminated` (from Employee Service)
- Queue: `compliance.training-completed` (from Career Service)

### Dead Letter Queue

**Queue**: `compliance.dead-letter`
**Purpose**: Failed message processing (version mismatch, validation errors)
**TTL**: 30 days
**Monitoring**: Daily review for repeated failures

---

## Retry Policies

### MassTransit Configuration

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<EmployeeCreatedEventConsumer>();
    x.AddConsumer<EmployeeTerminatedEventConsumer>();
    x.AddConsumer<TrainingCompletedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        });

        cfg.ConfigureEndpoints(context);
    });
});
```

**Retry Strategy**:
- Attempt 1: Immediate
- Attempt 2: +1 second
- Attempt 3: +3 seconds (1 + 2)
- Attempt 4: +5 seconds (1 + 2 + 2)
- After 4 attempts → Dead letter queue

---

## Testing Strategy

### Integration Event Tests

**Test Containers**: Real RabbitMQ instance

**Test Scenarios**:
1. **Publish Event**: Verify event published to correct exchange/routing key
2. **Consume Event**: Verify consumer processes message correctly
3. **Idempotency**: Verify duplicate events handled gracefully
4. **Version Tolerance**: Verify handling of unknown versions
5. **Dead Letter**: Verify failed messages route to DLQ

**Example Test**:
```csharp
[Fact]
public async Task EmployeeTerminatedEvent_DeactivatesWorkAuthorizations()
{
    // Arrange
    var employeeId = Guid.NewGuid();
    await CreateActiveWorkAuthorizationAsync(employeeId);

    var evt = new EmployeeTerminatedEvent(
        employeeId,
        DateTime.UtcNow,
        "Voluntary",
        DateTime.UtcNow,
        "1.0"
    );

    // Act
    await _bus.Publish(evt);
    await Task.Delay(500); // Allow consumer processing

    // Assert
    var authorizations = await _repository.GetByEmployeeIdAsync(employeeId);
    Assert.All(authorizations, auth => Assert.False(auth.IsActive));
}
```

---

## Monitoring & Observability

### Event Metrics

**Published Events**:
- `compliance_events_published_total{event_type}`
- `compliance_events_publish_duration_seconds{event_type}`

**Consumed Events**:
- `compliance_events_consumed_total{event_type}`
- `compliance_events_consume_duration_seconds{event_type}`
- `compliance_events_failed_total{event_type, reason}`

**Dead Letter Queue**:
- `compliance_dlq_messages_total`
- `compliance_dlq_oldest_message_age_seconds`

### Alerts

**Critical Alerts**:
- DLQ message count > 10
- Event processing failure rate > 5%
- Consumer lag > 1000 messages

**Warning Alerts**:
- Event processing duration > 5 seconds
- Retry attempts > 50% of total attempts
