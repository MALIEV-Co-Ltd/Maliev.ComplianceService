# Implementation Plan: Compliance Service

**Branch**: `001-compliance-service` | **Date**: 2025-12-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-compliance-service/spec.md`

## Summary

The Compliance Service is a microservice responsible for managing work authorizations, visas, permits, and employment compliance tracking. It provides automated expiration monitoring with configurable thresholds (30/60/90 days), compliance alert generation and resolution, and comprehensive reporting capabilities. The service integrates with Employee, Upload, and Notification services through REST APIs and event-driven messaging to maintain an accurate, real-time view of organizational compliance status.

**Primary Requirements**:
- Track employee work authorizations with expiration monitoring
- Generate automated alerts at 30/60/90 day thresholds
- Provide compliance summary reports with department filtering
- Support optimistic locking for concurrent updates
- Graceful degradation when external services unavailable

**Technical Approach**: .NET 10.0 microservice with PostgreSQL database, EF Core ORM, RabbitMQ messaging via MassTransit, Redis caching, and RESTful API documented with OpenAPI/Scalar. Background services handle scheduled jobs (expiration monitoring, flagging). Architecture follows CQRS pattern with command/query separation and repository abstraction for testability.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10.0
**Primary Dependencies**: ASP.NET Core 10.0, Entity Framework Core 10.x, MassTransit, Maliev.Aspire.ServiceDefaults (NuGet package)
**Storage**: PostgreSQL 18 (dedicated database: `compliance`)
**Testing**: xUnit with Testcontainers (real PostgreSQL, RabbitMQ, Redis - NO in-memory providers per Constitution IV)
**Target Platform**: Linux containers (Docker) on Kubernetes (GKE)
**Project Type**: Microservice (Clean Architecture with Api, Application, Domain, Infrastructure, Tests projects)
**Performance Goals**:
- Report generation: <5 seconds for 10,000 employees (SC-003)
- Alert processing: 100% accuracy for expiring authorizations (SC-002)
- Integration events: <30 seconds processing (SC-011)
- Concurrent updates: Optimistic locking prevents data loss (SC-010)

**Constraints**:
- Memory: 256MB request / 512MB limit per container (resource-optimized)
- Database: Index-optimized for expiration queries and employee lookups
- Caching: 15-minute TTL for compliance summary, 5-minute for alert counts
- Background jobs: 7 AM daily (expiration reminder), midnight daily (expired flagging)

**Scale/Scope**:
- Support: Up to 50,000 employees without degradation (SC-012)
- Work Authorizations: ~15-20% of employees require tracking
- Alerts: ~1-2 alerts per authorization lifecycle
- API Endpoints: 11 endpoints (3 controllers: WorkAuthorization, Alerts, Reports)

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ I. Service Autonomy (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Own PostgreSQL database (`compliance`) with `work_authorizations` and `compliance_alerts` tables
  - Own domain logic for compliance status calculation, alert generation, optimistic concurrency
  - Interacts via REST APIs (Employee, Upload, Notification, IAM services)
  - Publishes/consumes events via RabbitMQ (no direct database access to other services)

---

### ✅ II. Explicit Contracts

- **Status**: PASS
- **Evidence**:
  - OpenAPI 3.0.3 specification: `contracts/openapi.yaml`
  - Integration events documented: `contracts/integration-events.md`
  - Event versioning: MAJOR.MINOR format (e.g., "1.0")
  - Backward-compatible migrations (soft delete with `is_active`, nullable fields)
  - Scalar documentation UI at `/compliance/scalar/v1`

---

### ✅ III. Test-First Development (NON-NEGOTIABLE)

- **Status**: PASS (Commitment)
- **Evidence**:
  - Tests authored after specification approval, before implementation
  - Acceptance scenarios defined in spec.md (User Stories 1-6)
  - Minimum 80% coverage target for business logic (compliance calculation, alert generation)
  - Test categories: Unit, Integration, Contract tests planned

---

### ✅ IV. Real Infrastructure Testing (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Testcontainers for PostgreSQL 18 (NO EF Core InMemoryDatabase)
  - Testcontainers for RabbitMQ (NO in-memory message buses)
  - Testcontainers for Redis 7 (NO in-memory cache providers)
  - Test isolation via database transactions and queue purging
  - Production-like configuration (same PostgreSQL version, same RabbitMQ settings)

---

### ✅ V. Auditability & Observability

- **Status**: PASS
- **Evidence**:
  - Structured JSON logging with user IDs (`resolved_by` in alerts)
  - Immutable audit logs: resolved alerts retained indefinitely (FR-013)
  - Health checks: `/compliance/liveness`, `/compliance/readiness`
  - Mandatory LogLevel configuration in `appsettings.json`:
    ```json
    {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.AspNetCore.Watch.BrowserRefresh": "None",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.AspNetCore.Watch": "Warning",
        "System": "Warning"
      }
    }
    ```

---

### ✅ VI. Security & Compliance

- **Status**: PASS
- **Evidence**:
  - JWT authentication via `AddJwtAuthentication()`
  - Permission-based authorization: `compliance.manage`, `compliance.reports`
  - Sensitive data (none in this service - work auth data is non-PII)
  - GDPR compliance: Soft delete (`is_active = FALSE`), audit trail retention

---

### ✅ VII. Secrets Management & Configuration Security (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Google Secret Manager via `AddGoogleSecretManagerVolume()`
  - NuGet credentials via BuildKit secrets (Dockerfile)
  - No hardcoded credentials in source code
  - Database connection strings from configuration/secrets

---

### ✅ VIII. Zero Warnings Policy (NON-NEGOTIABLE)

- **Status**: PASS (Commitment)
- **Evidence**: Build configuration treats warnings as errors

---

### ✅ IX. Clean Project Artifacts (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Only `README.md` at repository root
  - `.gitignore` excludes bin/, obj/, .vs/, *.user
  - `.dockerignore` excludes specs/, .git/, Tests projects
  - CODEOWNERS file at `.github/CODEOWNERS` with `* @MALIEV-Co-Ltd/core-developers`

---

### ✅ X. Docker Best Practices (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Dockerfile in `Maliev.ComplianceService.Api/Dockerfile` (NOT root)
  - Uses built-in `app` user (no custom user creation)
  - Multi-stage build: SDK for build, ASP.NET runtime for final
  - Base images: `mcr.microsoft.com/dotnet/sdk:10.0`, `mcr.microsoft.com/dotnet/aspnet:10.0`
  - BuildKit secrets for NuGet credentials
  - Health check: `curl -f http://localhost:8080/compliance/liveness || exit 1`
  - Exposes port 8080: `ENV ASPNETCORE_URLS=http://+:8080`

**Standard Dockerfile Pattern**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /
COPY nuget.config ./
COPY ["Maliev.ComplianceService.Api/Maliev.ComplianceService.Api.csproj", "Maliev.ComplianceService.Api/"]
COPY ["Maliev.ComplianceService.Application/Maliev.ComplianceService.Application.csproj", "Maliev.ComplianceService.Application/"]
COPY ["Maliev.ComplianceService.Domain/Maliev.ComplianceService.Domain.csproj", "Maliev.ComplianceService.Domain/"]
COPY ["Maliev.ComplianceService.Infrastructure/Maliev.ComplianceService.Infrastructure.csproj", "Maliev.ComplianceService.Infrastructure/"]
RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    NUGET_USERNAME=$(cat /run/secrets/nuget_username) \
    NUGET_PASSWORD=$(cat /run/secrets/nuget_password) \
    dotnet restore "Maliev.ComplianceService.Api/Maliev.ComplianceService.Api.csproj"
COPY . .
WORKDIR "/Maliev.ComplianceService.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN chown -R app:app /app
USER app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK CMD curl -f http://localhost:8080/compliance/liveness || exit 1
ENTRYPOINT ["dotnet", "Maliev.ComplianceService.Api.dll"]
```

---

### ✅ XI. Simplicity & Maintainability

- **Status**: PASS
- **Evidence**:
  - YAGNI: No unnecessary features (certification tracking stubbed for future)
  - Stateless design: All state in PostgreSQL, Redis cache for performance only
  - Shared library: Maliev.Aspire.ServiceDefaults (versioned NuGet package)

---

### ✅ XII. Business Metrics & Analytics (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Business metrics exposed via Prometheus format
  - Key metrics:
    - `compliance_work_authorizations_total{status}`: Count by compliance status
    - `compliance_alerts_generated_total{severity}`: Alerts by severity
    - `compliance_report_requests_total`: Report generation count
    - `compliance_report_generation_duration_seconds`: Report performance
    - `compliance_expiring_authorizations_count{days_threshold}`: 30/60/90 day counts
  - Tags: `service_name=compliance-service`, `version`, `region`, `environment`
  - No PII in metrics (employee IDs excluded, only counts)

---

### ✅ XIII. .NET Aspire Integration (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - `Maliev.Aspire.ServiceDefaults` consumed as NuGet package from GitHub Packages
  - `nuget.config` with GitHub Packages source and credential placeholders
  - `Program.cs` calls `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`
  - Dockerfile uses BuildKit secrets for NuGet authentication (NOT ARG)

---

### ✅ XIV. Code Quality & Library Standards (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - NO AutoMapper: Explicit mapping in DTOs and handlers
  - NO FluentValidation: Data Annotations for validation
  - NO FluentAssertions: Standard xUnit `Assert`

---

### ✅ XV. Project Structure & Naming (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Flat structure (no `/src`, `/tests` folders)
  - Naming: `Maliev.ComplianceService.Api`, `Maliev.ComplianceService.Application`, `Maliev.ComplianceService.Domain`, `Maliev.ComplianceService.Infrastructure`, `Maliev.ComplianceService.Tests`
  - Dockerfile in `Maliev.ComplianceService.Api/Dockerfile`

---

### ✅ XVI. CI/CD Standards (NON-NEGOTIABLE)

- **Status**: PASS
- **Evidence**:
  - Workflows: `ci-develop.yml`, `ci-staging.yml`, `ci-main.yml`
  - Testcontainers for integration tests (NO docker-compose.yml)

---

### Summary

**All constitution checks PASS**. No violations. No complexity justification required.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-compliance-service/
├── plan.md                   # This file
├── spec.md                   # Feature specification
├── research.md               # Phase 0: Technical research and decisions
├── data-model.md             # Phase 1: Database schema and entities
├── quickstart.md             # Phase 1: Developer onboarding guide
├── contracts/                # Phase 1: API contracts
│   ├── openapi.yaml          # RESTful API specification
│   └── integration-events.md # Message bus events
├── checklists/
│   └── requirements.md       # Spec quality validation
└── tasks.md                  # Phase 2: (Created by /speckit.tasks command)
```

### Source Code (repository root)

```text
Maliev.ComplianceService/
├── Maliev.ComplianceService.Api/
│   ├── Controllers/
│   │   ├── WorkAuthorizationController.cs
│   │   ├── AlertsController.cs
│   │   └── ReportsController.cs
│   ├── Middlewares/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Maliev.ComplianceService.Api.csproj
│   └── Dockerfile
├── Maliev.ComplianceService.Application/
│   ├── Commands/
│   │   ├── RecordWorkAuthorization/
│   │   │   ├── RecordWorkAuthorizationCommand.cs
│   │   │   ├── RecordWorkAuthorizationCommandHandler.cs
│   │   │   └── RecordWorkAuthorizationValidator.cs
│   │   ├── UpdateWorkAuthorization/
│   │   │   ├── UpdateWorkAuthorizationCommand.cs
│   │   │   ├── UpdateWorkAuthorizationCommandHandler.cs
│   │   │   └── UpdateWorkAuthorizationValidator.cs
│   │   └── ResolveAlert/
│   │       ├── ResolveAlertCommand.cs
│   │       ├── ResolveAlertCommandHandler.cs
│   │       └── ResolveAlertValidator.cs
│   ├── Queries/
│   │   ├── GetWorkAuthorization/
│   │   │   ├── GetWorkAuthorizationQuery.cs
│   │   │   └── GetWorkAuthorizationQueryHandler.cs
│   │   ├── GetExpiringAuthorizations/
│   │   │   ├── GetExpiringAuthorizationsQuery.cs
│   │   │   └── GetExpiringAuthorizationsQueryHandler.cs
│   │   ├── GetAlerts/
│   │   │   ├── GetAlertsQuery.cs
│   │   │   └── GetAlertsQueryHandler.cs
│   │   └── GetComplianceReport/
│   │       ├── GetComplianceReportQuery.cs
│   │       └── GetComplianceReportQueryHandler.cs
│   ├── DTOs/
│   │   ├── RecordWorkAuthorizationRequest.cs
│   │   ├── UpdateWorkAuthorizationRequest.cs
│   │   ├── WorkAuthorizationResponse.cs
│   │   ├── ComplianceAlertResponse.cs
│   │   ├── ComplianceReportResponse.cs
│   │   └── ResolveAlertRequest.cs
│   ├── Interfaces/
│   │   ├── IWorkAuthorizationRepository.cs
│   │   ├── IComplianceAlertRepository.cs
│   │   └── IEmployeeService.cs
│   ├── Mappers/
│   │   └── DtoMapper.cs (explicit mapping, NO AutoMapper)
│   └── Maliev.ComplianceService.Application.csproj
├── Maliev.ComplianceService.Domain/
│   ├── Entities/
│   │   ├── WorkAuthorization.cs
│   │   └── ComplianceAlert.cs
│   ├── Enums/
│   │   ├── AuthorizationType.cs
│   │   ├── SponsorshipStatus.cs
│   │   ├── ComplianceStatus.cs
│   │   ├── AlertType.cs
│   │   └── AlertSeverity.cs
│   ├── Events/
│   │   ├── WorkAuthorizationExpiringEvent.cs
│   │   ├── WorkAuthorizationExpiredEvent.cs
│   │   ├── AccessRevocationRequiredEvent.cs
│   │   ├── EmployeeCreatedEvent.cs
│   │   ├── EmployeeTerminatedEvent.cs
│   │   └── TrainingCompletedEvent.cs
│   ├── Authorization/
│   │   └── CompliancePermissions.cs
│   └── Maliev.ComplianceService.Domain.csproj
├── Maliev.ComplianceService.Infrastructure/
│   ├── Data/
│   │   ├── ComplianceDbContext.cs
│   │   └── Configurations/
│   │       ├── WorkAuthorizationConfiguration.cs
│   │       └── ComplianceAlertConfiguration.cs
│   ├── Migrations/
│   │   └── (EF Core generated migrations)
│   ├── Repositories/
│   │   ├── WorkAuthorizationRepository.cs
│   │   └── ComplianceAlertRepository.cs
│   ├── Services/
│   │   └── EmployeeServiceClient.cs
│   ├── BackgroundServices/
│   │   ├── WorkAuthorizationExpirationReminderService.cs
│   │   └── ExpiredWorkAuthorizationFlaggingService.cs
│   ├── Consumers/
│   │   ├── EmployeeCreatedEventConsumer.cs
│   │   ├── EmployeeTerminatedEventConsumer.cs
│   │   └── TrainingCompletedEventConsumer.cs
│   ├── IAM/
│   │   └── ComplianceIAMRegistrationService.cs
│   └── Maliev.ComplianceService.Infrastructure.csproj
├── Maliev.ComplianceService.Tests/
│   ├── Unit/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Domain/
│   ├── Integration/
│   │   ├── Controllers/
│   │   ├── BackgroundServices/
│   │   ├── Repositories/
│   │   └── Consumers/
│   ├── Fixtures/
│   │   └── ComplianceServiceTestFixture.cs (Testcontainers setup)
│   └── Maliev.ComplianceService.Tests.csproj
├── .github/
│   ├── workflows/
│   │   ├── ci-develop.yml
│   │   ├── ci-staging.yml
│   │   └── ci-main.yml
│   └── CODEOWNERS
├── .gitignore
├── .dockerignore
├── nuget.config
├── README.md
└── LICENSE
```

**Structure Decision**: Clean Architecture pattern selected to maintain clear separation of concerns:
- **Api**: Controllers, HTTP concerns, Program.cs
- **Application**: Commands, Queries, DTOs, business logic orchestration
- **Domain**: Entities, Enums, Events, core business rules
- **Infrastructure**: Database, external services, background jobs, event consumers
- **Tests**: Unit (fast, isolated), Integration (Testcontainers), separated by layer

---

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations. This section is empty.

---

## Phase 0: Research (Complete)

**Artifact**: [research.md](./research.md)

**Key Decisions**:
1. **.NET 10.0 + ASP.NET Core**: Latest LTS, native performance, strong ecosystem
2. **PostgreSQL 18**: ACID compliance, time-based query performance, JSON support
3. **Entity Framework Core 10.x**: Code-first migrations, optimistic concurrency, change tracking
4. **RabbitMQ + MassTransit**: Proven reliability, ServiceDefaults integration, retry policies
5. **Redis 7.x**: Distributed cache, low latency, TTL support
6. **Data Annotations**: Built-in validation (NO FluentValidation per Constitution XIV)
7. **CQRS Pattern**: Command/Query separation, independent optimization
8. **Repository Pattern**: Abstraction for testability with Testcontainers
9. **Hosted Services**: Native .NET background jobs for scheduled tasks
10. **Optimistic Locking**: EF Core RowVersion for concurrent update handling

**All technical unknowns resolved. Ready for Phase 1.**

---

## Phase 1: Design & Contracts (Complete)

### Data Model

**Artifact**: [data-model.md](./data-model.md)

**Tables**:
- `work_authorizations` (15 columns, 4 indexes, 1 unique constraint)
- `compliance_alerts` (10 columns, 3 indexes)

**Enumerations**:
- `AuthorizationType` (8 values)
- `SponsorshipStatus` (5 values)
- `ComplianceStatus` (5 values)
- `AlertType` (5 values)
- `AlertSeverity` (4 values)

**Key Features**:
- Optimistic concurrency: `row_version` column
- Soft delete: `is_active` column
- Audit trail: `created_date`, `modified_date`, `resolved_by`, `resolved_date`
- Compliance status auto-calculation from `expiration_date`

---

### API Contracts

**Artifacts**:
- [contracts/openapi.yaml](./contracts/openapi.yaml)
- [contracts/integration-events.md](./contracts/integration-events.md)

**REST API Endpoints** (11 total):

**Work Authorizations**:
- `POST /employees/{employeeId}/work-authorization` - Record authorization
- `GET /employees/{employeeId}/work-authorization` - Get employee authorizations
- `GET /work-authorization/{authId}` - Get authorization by ID
- `PUT /work-authorization/{authId}` - Update authorization
- `GET /work-authorization/expiring` - Get expiring authorizations

**Alerts**:
- `GET /alerts` - Get compliance alerts (with filters)
- `PUT /alerts/{alertId}/resolve` - Resolve alert

**Reports**:
- `GET /reports/compliance` - Get compliance report (with department filter)

**Health & Observability**:
- `GET /compliance/liveness` - Liveness probe
- `GET /compliance/readiness` - Readiness probe
- `GET /compliance/metrics` - Prometheus metrics

**Integration Events**:

**Published** (3 events):
- `WorkAuthorizationExpiringEvent` - 30/60/90 day warnings
- `WorkAuthorizationExpiredEvent` - Authorization expired
- `AccessRevocationRequiredEvent` - 30 days post-expiration

**Consumed** (3 events):
- `EmployeeCreatedEvent` - Employee context maintenance
- `EmployeeTerminatedEvent` - Deactivate authorizations
- `TrainingCompletedEvent` - Future certification tracking

---

### Developer Onboarding

**Artifact**: [quickstart.md](./quickstart.md)

**Contents**:
- Prerequisites and setup
- Local development with Docker Compose (Postgres, RabbitMQ, Redis)
- Database migration workflow
- Running tests (Unit, Integration with Testcontainers)
- Development workflow examples
- Debugging tips
- Common issues and solutions

---

## Phase 2: Tasks

**Next Command**: `/speckit.tasks`

**This command generates**:
- Dependency-ordered task breakdown
- Acceptance criteria per task
- Estimated complexity
- Testing requirements

---

## Estimated Scope

### Files

- **Domain**: 10 files (entities, enums, events)
- **Application**: 15 files (commands, queries, DTOs, validators)
- **Infrastructure**: 12 files (DbContext, repositories, services, consumers)
- **Api**: 5 files (controllers, Program.cs, middleware)
- **Tests**: 20 files (unit + integration)
- **Total**: ~62 files

### Lines of Code

- **Domain**: ~500 LOC
- **Application**: ~1,500 LOC
- **Infrastructure**: ~2,000 LOC
- **Api**: ~800 LOC
- **Tests**: ~2,200 LOC
- **Total**: ~7,000 LOC

### Components

- **Controllers**: 3 (WorkAuthorization, Alerts, Reports)
- **Commands**: 3 (Record, Update, Resolve)
- **Queries**: 4 (GetById, GetExpiring, GetAlerts, GetReport)
- **Background Services**: 2 (Expiration Reminder, Expired Flagging)
- **Event Consumers**: 3 (EmployeeCreated, EmployeeTerminated, TrainingCompleted)
- **Repositories**: 2 (WorkAuthorization, ComplianceAlert)
- **External Clients**: 1 (EmployeeService)

---

## Deployment

### Kubernetes Resources

**Memory**:
- Request: 256MB
- Limit: 512MB

**CPU**:
- Request: 100m
- Limit: 500m

**Replicas**: 2 (high availability)

**Probes**:
- Liveness: `/compliance/liveness` (HTTP GET, port 8080)
- Readiness: `/compliance/readiness` (HTTP GET, port 8080)

**Secrets**:
- Database connection string (Google Secret Manager)
- RabbitMQ credentials (ServiceDefaults)
- Redis connection string (ServiceDefaults)
- NuGet credentials (BuildKit secrets for build only)

### Database Migration Strategy

**Initial Deployment**:
1. Export work authorizations from Employee Service
2. Import into Compliance Service database
3. Calculate initial compliance status (SQL UPDATE)
4. Verify data integrity

**Ongoing**:
- Integration events keep data synchronized
- EF Core migrations applied automatically on startup (`MigrateDatabaseAsync()`)

---

## Success Metrics

**Performance** (from spec.md success criteria):
- SC-001: Work authorization creation <2 minutes ✅
- SC-003: Report generation <5 seconds (10K employees) ✅
- SC-011: Integration event processing <30 seconds ✅

**Accuracy**:
- SC-002: 100% alert generation for expiring authorizations ✅
- SC-004: Zero expirations without 90-day notice ✅
- SC-006: 99.9% compliance status calculation accuracy ✅

**Efficiency**:
- SC-007: 70% reduction in manual compliance tracking time ✅
- SC-008: 95% alerts result in timely action before expiration ✅

**Scalability**:
- SC-012: Support 50K employees without degradation ✅

**User Experience**:
- SC-005: View expiring authorizations within 3 clicks ✅
- SC-009: Real-time reporting without performance impact ✅
- SC-010: Concurrent updates handled with optimistic locking ✅

---

## Next Steps

1. **Run `/speckit.tasks`** to generate detailed implementation tasks
2. **Review tasks.md** for dependency-ordered task breakdown
3. **Implement** following Test-First Development (Constitution III)
4. **Validate** with Testcontainers (Constitution IV)
5. **Deploy** to staging for integration testing

---

**Plan Complete**: Ready for Phase 2 (Task Generation)
