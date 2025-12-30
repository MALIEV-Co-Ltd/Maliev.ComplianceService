# Research: Compliance Service

**Date**: 2025-12-28
**Branch**: 001-compliance-service

## Technical Stack Decisions

### Runtime & Framework

**Decision**: .NET 10.0 with ASP.NET Core 10.0

**Rationale**:
- Latest LTS version providing modern C# 13 features
- Native support for minimal APIs and optimized performance
- Strong integration with Entity Framework Core for PostgreSQL
- Built-in dependency injection and middleware pipeline
- Excellent observability through OpenTelemetry integration

**Alternatives Considered**:
- .NET 8 (LTS): Rejected because project targets .NET 10 as specified
- Node.js: Rejected due to organization's standardization on .NET

---

### Database

**Decision**: PostgreSQL 18

**Rationale**:
- Advanced JSON support for flexible alert message storage
- Excellent performance for time-based queries (expiration monitoring)
- ACID compliance critical for compliance tracking
- Robust indexing for employee_id and expiration_date lookups
- Strong community support and reliability

**Alternatives Considered**:
- SQL Server: Rejected in favor of open-source PostgreSQL
- MongoDB: Rejected due to relational nature of work authorizations

---

### ORM

**Decision**: Entity Framework Core 10.x

**Rationale**:
- Native .NET integration with excellent tooling
- Code-first migrations for schema evolution
- Strong typing and compile-time query validation
- Built-in change tracking for optimistic concurrency
- AsNoTracking() support for read-heavy queries

**Alternatives Considered**:
- Dapper: Rejected due to need for change tracking and migrations
- Raw ADO.NET: Rejected due to development velocity requirements

---

### Messaging

**Decision**: RabbitMQ with MassTransit via ServiceDefaults

**Rationale**:
- Proven reliability for event-driven architecture
- MassTransit provides excellent abstractions and retry policies
- ServiceDefaults integration ensures consistent configuration
- Support for message patterns (publish/subscribe, request/response)
- Built-in dead-letter queue handling

**Alternatives Considered**:
- Azure Service Bus: Rejected due to cloud-agnostic requirements
- Kafka: Rejected as over-engineering for current message volumes

---

### Caching

**Decision**: Redis 7.x

**Rationale**:
- Fast in-memory caching for compliance report summaries
- Distributed cache support for multi-instance deployments
- TTL support for cache expiration strategies
- Low latency (<1ms) for hot-path queries
- ServiceDefaults integration for consistent configuration

**Alternatives Considered**:
- In-memory cache: Rejected due to multi-instance deployment requirements
- Memcached: Rejected in favor of Redis's richer feature set

---

### Validation

**Decision**: Data Annotations (.NET native)

**Rationale**:
- Built-in framework support with no additional dependencies
- Clear, declarative validation rules at model level
- Automatic model state validation in ASP.NET Core
- Consistent with constitution prohibition of FluentValidation
- Sufficient for compliance service validation needs

**Alternatives Considered**:
- FluentValidation: PROHIBITED by constitution (XIV)
- Custom validation: Rejected as unnecessary when Data Annotations suffice

---

## Architecture Patterns

### Command Query Responsibility Segregation (CQRS)

**Decision**: Lightweight CQRS with Commands and Queries

**Rationale**:
- Clear separation of write (Commands) and read (Queries) operations
- Commands: RecordWorkAuthorization, UpdateWorkAuthorization, ResolveAlert
- Queries: GetWorkAuthorization, GetExpiringAuthorizations, GetComplianceReport
- Enables independent optimization of read and write paths
- Avoids complexity of full event sourcing

**Implementation**:
- Commands in `Application/Commands/` with dedicated handlers
- Queries in `Application/Queries/` with dedicated handlers
- Shared DTOs in `Application/DTOs/`

---

### Repository Pattern

**Decision**: Simple repository interfaces for data access

**Rationale**:
- Abstracts EF Core implementation details from business logic
- Enables testability with Testcontainers (real PostgreSQL)
- `IWorkAuthorizationRepository` and `IComplianceAlertRepository`
- Keeps controllers and handlers clean
- Aligns with domain-driven design principles

**Alternatives Considered**:
- Direct DbContext injection: Rejected to maintain abstraction and testability
- Generic Repository: Rejected as over-engineering for this service

---

### Background Services

**Decision**: .NET Hosted Services for scheduled jobs

**Rationale**:
- Native framework support (`IHostedService`)
- `WorkAuthorizationExpirationReminderService`: Runs daily at 7 AM
- `ExpiredWorkAuthorizationFlaggingService`: Runs midnight daily
- Scoped service injection for database access
- Integrated with application lifecycle

**Implementation**:
```csharp
public class WorkAuthorizationExpirationReminderService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunAtScheduledTime(new TimeOnly(7, 0), stoppingToken);
        }
    }
}
```

---

## Integration Patterns

### Event-Driven Communication

**Decision**: MassTransit consumers for integration events

**Rationale**:
- `EmployeeCreatedEventConsumer`: Maintains employee context
- `EmployeeTerminatedEventConsumer`: Marks authorizations inactive
- `TrainingCompletedEventConsumer`: Future certification tracking
- Asynchronous processing prevents blocking
- Retry policies handle transient failures

---

### External Service Communication

**Decision**: Typed HttpClient with Polly resilience

**Rationale**:
- Employee Service: For name lookup (graceful degradation on failure)
- Notification Service: For alert delivery
- IAM Service: For permission registration
- Polly retry policies from ServiceDefaults
- Circuit breaker prevents cascade failures

**Graceful Degradation**:
- Employee name unavailable: Display "(name unavailable)" per FR-031
- Notification failure: Log error, continue operation
- IAM unavailable: Retry with exponential backoff

---

## Concurrency Control

### Optimistic Locking Strategy

**Decision**: EF Core concurrency tokens (RowVersion/Timestamp)

**Rationale**:
- Prevents lost updates from concurrent HR administrators
- Version/timestamp column in `work_authorizations` table
- EF Core automatically checks version on update
- Returns `CONCURRENT_MODIFICATION` error on conflict
- User retries with fresh data

**Implementation**:
```csharp
public class WorkAuthorization
{
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

---

## Performance Optimizations

### Database Indexing

**Decision**: Strategic indexes on high-query columns

**Rationale**:
- `idx_work_auth_employee`: Fast employee lookup
- `idx_work_auth_expiration`: Background job performance
- `idx_work_auth_status`: Compliance report queries
- `idx_work_auth_active`: Partial index for active records only
- `idx_alerts_unresolved`: Fast active alert retrieval

---

### Caching Strategy

**Decision**: Redis distributed cache for reports

**Rationale**:
- Compliance summary: 15-minute TTL (SC-003: <5s for 10K employees)
- Expiring authorizations: Real-time (no cache)
- Alert counts: 5-minute TTL
- Cache invalidation on write operations

---

### Query Optimization

**Decision**: AsNoTracking for read-only queries

**Rationale**:
- Reduces memory overhead for report generation
- Faster query execution (no change tracking)
- Applied to all queries, not commands
- Maintains tracking for updates (optimistic concurrency)

---

## Testing Strategy

### Real Infrastructure with Testcontainers

**Decision**: PostgreSQL, RabbitMQ, Redis containers for all tests

**Rationale**:
- Constitution IV: MANDATORY real infrastructure testing
- No EF Core InMemoryDatabase permitted
- Ensures production-like behavior
- Tests distributed locking, transactions, message serialization
- Container lifecycle managed per test class

**Implementation**:
```csharp
public class ComplianceServiceTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;
    private RabbitMqContainer _rabbitmq;
    private RedisContainer _redis;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder().Build();
        await _postgres.StartAsync();
        // Similar for RabbitMQ, Redis
    }
}
```

---

## Deployment Considerations

### Container Configuration

**Decision**: 256MB request / 512MB limit

**Rationale**:
- Lightweight service with minimal entity model
- EF Core + caching fit within limits
- Background services run in same container
- Auto-scaling based on CPU (>70%)

---

### Health Checks

**Decision**: Liveness and readiness probes

**Rationale**:
- Liveness: `/compliance/liveness` (service responding)
- Readiness: `/compliance/readiness` (DB + RabbitMQ + Redis healthy)
- Kubernetes restarts on liveness failure
- Traffic routing based on readiness

---

## Security & Compliance

### Permission Model

**Decision**: Fine-grained permissions via IAM service

**Rationale**:
- `compliance.manage`: Create, update, view authorizations and alerts
- `compliance.reports`: View compliance reports
- JWT bearer token authentication
- Claims-based authorization

---

### Audit Trail

**Decision**: Immutable alert history + timestamps

**Rationale**:
- Resolved alerts retained indefinitely (FR-013)
- `created_date` and `modified_date` on all records
- `resolved_by` tracks who resolved alerts
- Supports regulatory compliance and audit requirements

---

## Data Migration Strategy

**Decision**: One-time import from Employee Service

**Rationale**:
- Export existing work authorizations via SQL COPY
- Calculate initial compliance status in bulk
- Ongoing sync via integration events
- Idempotent import process

---

## Open Questions (None)

All technical decisions resolved. Ready for Phase 1 design.
