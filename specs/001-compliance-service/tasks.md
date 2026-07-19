# Tasks: Compliance Service

**Input**: Design documents from `/specs/001-compliance-service/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: This project follows Test-First Development (Constitution III). Test tasks are included and should FAIL before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

**Clean Architecture Structure** (from plan.md):
- **Api**: `Maliev.ComplianceService.Api/`
- **Application**: `Maliev.ComplianceService.Application/`
- **Domain**: `Maliev.ComplianceService.Domain/`
- **Infrastructure**: `Maliev.ComplianceService.Infrastructure/`
- **Tests**: `Maliev.ComplianceService.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file `Maliev.ComplianceService.sln` at repository root
- [X] T002 [P] Create Api project `Maliev.ComplianceService.Api/Maliev.ComplianceService.Api.csproj` with .NET 10.0 SDK
- [X] T003 [P] Create Application project `Maliev.ComplianceService.Application/Maliev.ComplianceService.Application.csproj`
- [X] T004 [P] Create Domain project `Maliev.ComplianceService.Domain/Maliev.ComplianceService.Domain.csproj`
- [X] T005 [P] Create Infrastructure project `Maliev.ComplianceService.Infrastructure/Maliev.ComplianceService.Infrastructure.csproj`
- [X] T006 [P] Create Tests project `Maliev.ComplianceService.Tests/Maliev.ComplianceService.Tests.csproj` with xUnit and Testcontainers
- [X] T007 Create `nuget.config` at repository root with GitHub Packages source for Maliev.Aspire.ServiceDefaults
- [X] T008 [P] Create `.gitignore` at repository root excluding bin/, obj/, .vs/, *.user
- [X] T009 [P] Create `.dockerignore` excluding specs/, .git/, Tests projects, bin/, obj/
- [X] T010 [P] Create `.github/CODEOWNERS` with `* @MALIEV-Co-Ltd/core-developers`
- [X] T011 Add project references: Api → Application+Infrastructure, Application → Domain, Infrastructure → Domain+Application

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Layer (Entities, Enums, Events)

- [X] T012 [P] Create `AuthorizationType` enum in `Maliev.ComplianceService.Domain/Enums/AuthorizationType.cs` (8 values: Citizen, PermanentResident, WorkVisa, StudentVisa, TemporaryWorker, Refugee, Asylee, Other)
- [X] T013 [P] Create `SponsorshipStatus` enum in `Maliev.ComplianceService.Domain/Enums/SponsorshipStatus.cs` (5 values: NotRequired, Sponsored, TransferPending, RenewalPending, Expired)
- [X] T014 [P] Create `ComplianceStatus` enum in `Maliev.ComplianceService.Domain/Enums/ComplianceStatus.cs` (5 values: Compliant, ExpiringSoon, Expired, PendingVerification, NonCompliant)
- [X] T015 [P] Create `AlertType` enum in `Maliev.ComplianceService.Domain/Enums/AlertType.cs` (5 values: ExpirationWarning, Expired, DocumentMissing, VerificationRequired, RenewalReminder)
- [X] T016 [P] Create `AlertSeverity` enum in `Maliev.ComplianceService.Domain/Enums/AlertSeverity.cs` (4 values: Low, Medium, High, Critical)
- [X] T017 Create `WorkAuthorization` entity in `Maliev.ComplianceService.Domain/Entities/WorkAuthorization.cs` with all properties and RowVersion for optimistic locking
- [X] T018 Create `ComplianceAlert` entity in `Maliev.ComplianceService.Domain/Entities/ComplianceAlert.cs` with navigation property to WorkAuthorization
- [X] T019 [P] Create `CompliancePermissions` class in `Maliev.ComplianceService.Domain/Authorization/CompliancePermissions.cs` with Manage and Reports constants

### Infrastructure Layer (Database, Repositories)

- [X] T020 Create `ComplianceDbContext` in `Maliev.ComplianceService.Infrastructure/Data/ComplianceDbContext.cs` with DbSets for WorkAuthorizations and ComplianceAlerts
- [X] T021 [P] Create `WorkAuthorizationConfiguration` in `Maliev.ComplianceService.Infrastructure/Data/Configurations/WorkAuthorizationConfiguration.cs` with table mapping, indexes, and unique constraint
- [X] T022 [P] Create `ComplianceAlertConfiguration` in `Maliev.ComplianceService.Infrastructure/Data/Configurations/ComplianceAlertConfiguration.cs` with table mapping and indexes
- [X] T023 Create initial EF Core migration in `Maliev.ComplianceService.Infrastructure/Migrations/` using `dotnet ef migrations add InitialCreate`
- [X] T024 Create `IWorkAuthorizationRepository` interface in `Maliev.ComplianceService.Application/Interfaces/IWorkAuthorizationRepository.cs`
- [X] T025 Create `IComplianceAlertRepository` interface in `Maliev.ComplianceService.Application/Interfaces/IComplianceAlertRepository.cs`
- [X] T026 Implement `WorkAuthorizationRepository` in `Maliev.ComplianceService.Infrastructure/Repositories/WorkAuthorizationRepository.cs` with all CRUD and query methods
- [X] T027 Implement `ComplianceAlertRepository` in `Maliev.ComplianceService.Infrastructure/Repositories/ComplianceAlertRepository.cs` with all CRUD and query methods

### API Layer (Program.cs, Middleware, Configuration)

- [X] T028 Create `Program.cs` in `Maliev.ComplianceService.Api/Program.cs` with ServiceDefaults integration, database configuration, MassTransit, Redis, authentication, authorization, rate limiting
- [X] T029 Create `appsettings.json` in `Maliev.ComplianceService.Api/appsettings.json` with LogLevel configuration per Constitution V
- [X] T030 [P] Create `appsettings.Development.json` in `Maliev.ComplianceService.Api/appsettings.Development.json` with local connection strings
- [X] T031 [P] Create `ExceptionHandlingMiddleware` in `Maliev.ComplianceService.Api/Middlewares/ExceptionHandlingMiddleware.cs` for global error handling

### Docker & Deployment

- [X] T032 Create `Dockerfile` in `Maliev.ComplianceService.Api/Dockerfile` following Constitution X standards (multi-stage, app user, BuildKit secrets, health check)
- [X] T033 [P] Create `docker-compose.dev.yml` at repository root for local development (PostgreSQL, RabbitMQ, Redis)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Record Work Authorization (Priority: P1) 🎯 MVP

**Goal**: HR administrators can record employee work authorization details with automatic compliance status calculation

**Independent Test**: Create a work authorization record with complete details and verify it's stored with calculated compliance status

### Tests for User Story 1

- [X] T034 [P] [US1] Unit test for RecordWorkAuthorizationCommandHandler in `Maliev.ComplianceService.Tests/Unit/Commands/RecordWorkAuthorizationCommandHandlerTests.cs` testing validation, duplicate check, status calculation
- [X] T035 [P] [US1] Integration test for POST /employees/{employeeId}/work-authorization in `Maliev.ComplianceService.Tests/Integration/Controllers/WorkAuthorizationControllerTests.cs` with Testcontainers (PostgreSQL)
- [X] T036 [P] [US1] Validation test for INVALID_DATE_RANGE error in `Maliev.ComplianceService.Tests/Unit/Commands/RecordWorkAuthorizationValidatorTests.cs`
- [X] T037 [P] [US1] Validation test for DUPLICATE_AUTHORIZATION error using repository mock in unit tests

### Implementation for User Story 1

- [X] T038 [P] [US1] Create `RecordWorkAuthorizationRequest` DTO in `Maliev.ComplianceService.Application/DTOs/RecordWorkAuthorizationRequest.cs` with Data Annotations validation
- [X] T039 [P] [US1] Create `WorkAuthorizationResponse` DTO in `Maliev.ComplianceService.Application/DTOs/WorkAuthorizationResponse.cs`
- [X] T040 [P] [US1] Create `DtoMapper` class in `Maliev.ComplianceService.Application/Mappers/DtoMapper.cs` with explicit mapping methods (NO AutoMapper per Constitution XIV)
- [X] T041 [US1] Create `RecordWorkAuthorizationCommand` in `Maliev.ComplianceService.Application/Commands/RecordWorkAuthorization/RecordWorkAuthorizationCommand.cs`
- [X] T042 [US1] Create `RecordWorkAuthorizationCommandHandler` in `Maliev.ComplianceService.Application/Commands/RecordWorkAuthorization/RecordWorkAuthorizationCommandHandler.cs` implementing validation, duplicate check, compliance status calculation, and persistence
- [X] T043 [P] [US1] Create `RecordWorkAuthorizationValidator` in `Maliev.ComplianceService.Application/Commands/RecordWorkAuthorization/RecordWorkAuthorizationValidator.cs` with date range and document requirement validation
- [X] T044 [US1] Create `WorkAuthorizationController` in `Maliev.ComplianceService.Api/Controllers/WorkAuthorizationController.cs` with POST endpoint
- [X] T045 [US1] Add command handler to DI container in Program.cs
- [X] T046 [US1] Add validation error mapping to return proper error codes (INVALID_DATE_RANGE, DUPLICATE_AUTHORIZATION, EMPLOYEE_NOT_FOUND)

**Checkpoint**: User Story 1 should be fully functional - HR can record work authorizations and see calculated compliance status

---

## Phase 4: User Story 2 - View Expiring Authorizations (Priority: P1)

**Goal**: HR administrators can see which work authorizations are approaching expiration within a specified timeframe

**Independent Test**: Query for authorizations expiring within 90 days and verify results include employee name, document number, days until expiration

### Tests for User Story 2

- [X] T047 [P] [US2] Unit test for GetExpiringAuthorizationsQueryHandler in `Maliev.ComplianceService.Tests/Unit/Queries/GetExpiringAuthorizationsQueryHandlerTests.cs`
- [X] T048 [P] [US2] Integration test for GET /work-authorization/expiring in `Maliev.ComplianceService.Tests/Integration/Controllers/WorkAuthorizationControllerTests.cs` verifying date filtering and employee name retrieval

### Implementation for User Story 2

- [X] T049 [P] [US2] Create `ExpiringAuthorizationResponse` DTO in `Maliev.ComplianceService.Application/DTOs/ExpiringAuthorizationResponse.cs` extending WorkAuthorizationResponse
- [X] T050 [P] [US2] Create `IEmployeeService` interface in `Maliev.ComplianceService.Application/Interfaces/IEmployeeService.cs` for employee name lookup
- [X] T051 [US2] Create `EmployeeServiceClient` in `Maliev.ComplianceService.Infrastructure/Services/EmployeeServiceClient.cs` with graceful degradation (FR-031: return "(name unavailable)" on failure)
- [X] T052 [US2] Create `GetExpiringAuthorizationsQuery` in `Maliev.ComplianceService.Application/Queries/GetExpiringAuthorizations/GetExpiringAuthorizationsQuery.cs`
- [X] T053 [US2] Create `GetExpiringAuthorizationsQueryHandler` in `Maliev.ComplianceService.Application/Queries/GetExpiringAuthorizations/GetExpiringAuthorizationsQueryHandler.cs` with efficient date filtering and employee name resolution
- [X] T054 [US2] Add GET /work-authorization/expiring endpoint to WorkAuthorizationController with daysUntilExpiration parameter (default 90)
- [X] T055 [US2] Add query handler and EmployeeServiceClient to DI container in Program.cs with HttpClient configuration for Employee Service

**Checkpoint**: User Story 2 should be fully functional - HR can view expiring authorizations with employee names

---

## Phase 5: User Story 3 - Receive Compliance Alerts (Priority: P1)

**Goal**: HR administrators receive automated alerts when work authorizations are expiring or have expired (30/60/90 days)

**Independent Test**: Create authorizations with various expiration dates, run background services, verify alerts are generated with correct severity levels

### Tests for User Story 3

- [X] T056 [P] [US3] Unit test for WorkAuthorizationExpirationReminderService in `Maliev.ComplianceService.Tests/Unit/BackgroundServices/WorkAuthorizationExpirationReminderServiceTests.cs` testing 30/60/90 day thresholds
- [X] T057 [P] [US3] Unit test for ExpiredWorkAuthorizationFlaggingService in `Maliev.ComplianceService.Tests/Unit/BackgroundServices/ExpiredWorkAuthorizationFlaggingServiceTests.cs` testing expiration detection
- [X] T058 [P] [US3] Integration test for alert generation in `Maliev.ComplianceService.Tests/Integration/BackgroundServices/AlertGenerationTests.cs` with Testcontainers
- [X] T059 [P] [US3] Integration test for GET /alerts endpoint in `Maliev.ComplianceService.Tests/Integration/Controllers/AlertsControllerTests.cs`

### Implementation for User Story 3

- [X] T060 [P] [US3] Create `ComplianceAlertResponse` DTO in `Maliev.ComplianceService.Application/DTOs/ComplianceAlertResponse.cs`
- [X] T061 [P] [US3] Create `WorkAuthorizationExpiringEvent` record in `Maliev.ComplianceService.Domain/Events/WorkAuthorizationExpiringEvent.cs` (per contracts/integration-events.md) with properties: AuthorizationId, EmployeeId, AuthorizationType, ExpirationDate, DaysUntilExpiration, Timestamp, Version
- [X] T062 [P] [US3] Create `WorkAuthorizationExpiredEvent` record in `Maliev.ComplianceService.Domain/Events/WorkAuthorizationExpiredEvent.cs` (per contracts/integration-events.md) with properties: AuthorizationId, EmployeeId, AuthorizationType, ExpirationDate, ExpiredDays, Timestamp, Version
- [X] T063 [US3] Create `WorkAuthorizationExpirationReminderService` in `Maliev.ComplianceService.Infrastructure/BackgroundServices/WorkAuthorizationExpirationReminderService.cs` running daily at 7 AM, checking for 30/60/90 day expirations, creating alerts, publishing events
- [X] T064 [US3] Create `ExpiredWorkAuthorizationFlaggingService` in `Maliev.ComplianceService.Infrastructure/BackgroundServices/ExpiredWorkAuthorizationFlaggingService.cs` running daily at midnight, detecting expired authorizations, creating critical alerts, publishing events
- [X] T065 [US3] Create `GetAlertsQuery` in `Maliev.ComplianceService.Application/Queries/GetAlerts/GetAlertsQuery.cs` with filtering parameters
- [X] T066 [US3] Create `GetAlertsQueryHandler` in `Maliev.ComplianceService.Application/Queries/GetAlerts/GetAlertsQueryHandler.cs` supporting resolved, severity, employeeId, date range filters
- [X] T067 [US3] Create `AlertsController` in `Maliev.ComplianceService.Api/Controllers/AlertsController.cs` with GET /alerts endpoint
- [X] T068 [US3] Add background services, query handler, and AlertsController to DI container in Program.cs
- [X] T069 [US3] Add MassTransit publisher configuration for WorkAuthorizationExpiringEvent and WorkAuthorizationExpiredEvent in Program.cs

**Checkpoint**: User Story 3 should be fully functional - alerts are automatically generated at correct thresholds and retrievable via API

---

## Phase 6: User Story 4 - View Compliance Reports (Priority: P2)

**Goal**: HR managers can view compliance summary reports showing overall compliance status across the organization

**Independent Test**: Request compliance report and verify it contains accurate statistics, breakdown by authorization type, and active alerts

### Tests for User Story 4

- [X] T070 [P] [US4] Unit test for GetComplianceReportQueryHandler in `Maliev.ComplianceService.Tests/Unit/Queries/GetComplianceReportQueryHandlerTests.cs` testing calculation logic
- [X] T071 [P] [US4] Integration test for GET /reports/compliance in `Maliev.ComplianceService.Tests/Integration/Controllers/ReportsControllerTests.cs` with department filtering
- [X] T072 [P] [US4] Performance test for report generation in `Maliev.ComplianceService.Tests/Integration/Performance/ReportPerformanceTests.cs` verifying <5 seconds for 10K employees (SC-003)

### Implementation for User Story 4

- [X] T073 [P] [US4] Create `ComplianceReportResponse` DTO in `Maliev.ComplianceService.Application/DTOs/ComplianceReportResponse.cs`
- [X] T074 [P] [US4] Create `AuthorizationTypeBreakdown` DTO in `Maliev.ComplianceService.Application/DTOs/AuthorizationTypeBreakdown.cs`
- [X] T075 [US4] Create `GetComplianceReportQuery` in `Maliev.ComplianceService.Application/Queries/GetComplianceReport/GetComplianceReportQuery.cs` with optional departmentId parameter
- [X] T076 [US4] Create `GetComplianceReportQueryHandler` in `Maliev.ComplianceService.Application/Queries/GetComplianceReport/GetComplianceReportQueryHandler.cs` with efficient aggregation queries and Redis caching (15-minute TTL)
- [X] T077 [US4] Create `ReportsController` in `Maliev.ComplianceService.Api/Controllers/ReportsController.cs` with GET /reports/compliance endpoint requiring compliance.reports permission
- [X] T078 [US4] Add Redis distributed cache configuration in Program.cs with "compliance:" instance name
- [X] T079 [US4] Add query handler and ReportsController to DI container in Program.cs

**Checkpoint**: User Story 4 should be fully functional - compliance reports generate quickly with accurate statistics

---

## Phase 7: User Story 5 - Update Work Authorization (Priority: P2)

**Goal**: HR administrators can update existing work authorization records with optimistic locking to prevent concurrent modification conflicts

**Independent Test**: Update an authorization's expiration date, verify changes are saved with modification timestamp; attempt concurrent update and verify CONCURRENT_MODIFICATION error

### Tests for User Story 5

- [X] T080 [P] [US5] Unit test for UpdateWorkAuthorizationCommandHandler in `Maliev.ComplianceService.Tests/Unit/Commands/UpdateWorkAuthorizationCommandHandlerTests.cs` testing optimistic locking
- [X] T081 [P] [US5] Integration test for PUT /work-authorization/{authId} in `Maliev.ComplianceService.Tests/Integration/Controllers/WorkAuthorizationControllerTests.cs` verifying concurrent modification detection
- [X] T082 [P] [US5] Integration test for compliance status recalculation after expiration date update in `Maliev.ComplianceService.Tests/Integration/Controllers/WorkAuthorizationControllerTests.cs`

### Implementation for User Story 5

- [X] T083 [P] [US5] Create `UpdateWorkAuthorizationRequest` DTO in `Maliev.ComplianceService.Application/DTOs/UpdateWorkAuthorizationRequest.cs` with RowVersion property
- [X] T084 [US5] Create `UpdateWorkAuthorizationCommand` in `Maliev.ComplianceService.Application/Commands/UpdateWorkAuthorization/UpdateWorkAuthorizationCommand.cs`
- [X] T085 [US5] Create `UpdateWorkAuthorizationCommandHandler` in `Maliev.ComplianceService.Application/Commands/UpdateWorkAuthorization/UpdateWorkAuthorizationCommandHandler.cs` implementing optimistic locking check, compliance status recalculation, and modification timestamp
- [X] T086 [P] [US5] Create `UpdateWorkAuthorizationValidator` in `Maliev.ComplianceService.Application/Commands/UpdateWorkAuthorization/UpdateWorkAuthorizationValidator.cs`
- [X] T087 [US5] Add PUT /work-authorization/{authId} endpoint to WorkAuthorizationController with DbUpdateConcurrencyException handling
- [X] T088 [US5] Add command handler to DI container in Program.cs
- [X] T089 [US5] Add CONCURRENT_MODIFICATION error code mapping in exception handling middleware

**Checkpoint**: User Story 5 should be fully functional - authorizations can be updated safely with concurrency protection

---

## Phase 8: User Story 6 - Resolve Compliance Alerts (Priority: P3)

**Goal**: HR administrators can mark compliance alerts as resolved with notes, retained indefinitely for audit trail

**Independent Test**: Mark an active alert as resolved with notes, verify it no longer appears in active alerts but is retrievable with filters

### Tests for User Story 6

- [X] T090 [P] [US6] Unit test for ResolveAlertCommandHandler in `Maliev.ComplianceService.Tests/Unit/Commands/ResolveAlertCommandHandlerTests.cs`
- [X] T091 [P] [US6] Integration test for PUT /alerts/{alertId}/resolve in `Maliev.ComplianceService.Tests/Integration/Controllers/AlertsControllerTests.cs` verifying resolved alerts are retained (FR-013)
- [X] T092 [P] [US6] Integration test for resolved alert retrieval with filters (FR-013a) in `Maliev.ComplianceService.Tests/Integration/Controllers/AlertsControllerTests.cs`

### Implementation for User Story 6

- [X] T093 [P] [US6] Create `ResolveAlertRequest` DTO in `Maliev.ComplianceService.Application/DTOs/ResolveAlertRequest.cs`
- [X] T094 [US6] Create `ResolveAlertCommand` in `Maliev.ComplianceService.Application/Commands/ResolveAlert/ResolveAlertCommand.cs`
- [X] T095 [US6] Create `ResolveAlertCommandHandler` in `Maliev.ComplianceService.Application/Commands/ResolveAlert/ResolveAlertCommandHandler.cs` setting resolved flag, date, user ID, and notes
- [X] T096 [P] [US6] Create `ResolveAlertValidator` in `Maliev.ComplianceService.Application/Commands/ResolveAlert/ResolveAlertValidator.cs` requiring resolution notes
- [X] T097 [US6] Add PUT /alerts/{alertId}/resolve endpoint to AlertsController
- [X] T098 [US6] Update GetAlertsQuery to support filtering by resolved status, date range, employee, alert type, and resolver (FR-013a)
- [X] T099 [US6] Add command handler to DI container in Program.cs

**Checkpoint**: User Story 6 should be fully functional - alerts can be resolved and historical alerts are retrievable

---

## Phase 9: Integration Events (Cross-Cutting)

**Purpose**: Event-driven integration with Employee, Career, and Notification services

### Event Consumers

- [X] T100 [P] Create `EmployeeCreatedEvent` record in `Maliev.ComplianceService.Domain/Events/EmployeeCreatedEvent.cs` (per contracts/integration-events.md) with properties: EmployeeId, EmployeeNumber, FirstName, LastName, Email, DepartmentId, StartDate, Timestamp, Version
- [X] T101 [P] Create `EmployeeTerminatedEvent` record in `Maliev.ComplianceService.Domain/Events/EmployeeTerminatedEvent.cs` (per contracts/integration-events.md) with properties: EmployeeId, TerminationDate, Reason, Timestamp, Version
- [X] T102 [P] Create `TrainingCompletedEvent` record in `Maliev.ComplianceService.Domain/Events/TrainingCompletedEvent.cs` (per contracts/integration-events.md) with properties: EmployeeId, CourseName, CompletionDate, CertificationExpiration?, CertificateId, Timestamp, Version
- [X] T103 [P] Create `EmployeeCreatedEventConsumer` in `Maliev.ComplianceService.Infrastructure/Consumers/EmployeeCreatedEventConsumer.cs` (logs event for audit trail)
- [X] T104 [P] Create `EmployeeTerminatedEventConsumer` in `Maliev.ComplianceService.Infrastructure/Consumers/EmployeeTerminatedEventConsumer.cs` (marks work authorizations inactive)
- [X] T105 [P] Create `TrainingCompletedEventConsumer` in `Maliev.ComplianceService.Infrastructure/Consumers/TrainingCompletedEventConsumer.cs` (stub for future certification tracking)
- [X] T106 Add event consumers to MassTransit configuration in Program.cs
- [X] T107 [P] Integration test for EmployeeTerminatedEventConsumer in `Maliev.ComplianceService.Tests/Integration/Consumers/EmployeeTerminatedEventConsumerTests.cs` with Testcontainers (RabbitMQ)

### Event Publishers (Already created in US3)

- [X] T108 [P] Create `AccessRevocationRequiredEvent` record in `Maliev.ComplianceService.Domain/Events/AccessRevocationRequiredEvent.cs` (per contracts/integration-events.md) with properties: EmployeeId, EffectiveDate, Reason, AuthorizationId, ExpiredSince, Timestamp, Version
- [X] T109 Update ExpiredWorkAuthorizationFlaggingService to publish AccessRevocationRequiredEvent 30 days after expiration (FR-018)
- [X] T110 [P] Integration test for event publishing in `Maliev.ComplianceService.Tests/Integration/Events/EventPublishingTests.cs` with Testcontainers (RabbitMQ)

---

## Phase 10: Additional Endpoints & Features

**Purpose**: Remaining CRUD endpoints for completeness

- [X] T111 [P] Add GET /employees/{employeeId}/work-authorization endpoint to WorkAuthorizationController (retrieve all authorizations for employee)
- [X] T112 [P] Add GET /work-authorization/{authId} endpoint to WorkAuthorizationController (retrieve single authorization by ID)
- [X] T113 [P] Create `GetWorkAuthorizationQuery` and handler in `Maliev.ComplianceService.Application/Queries/GetWorkAuthorization/`
- [X] T114 [P] Create `GetEmployeeWorkAuthorizationsQuery` and handler in `Maliev.ComplianceService.Application/Queries/GetEmployeeWorkAuthorizations/`
- [X] T115 [P] Integration tests for new GET endpoints in `Maliev.ComplianceService.Tests/Integration/Controllers/WorkAuthorizationControllerTests.cs`

---

## Phase 11: IAM & Observability

**Purpose**: Permission registration and business metrics

- [X] T116 [P] Create `ComplianceIAMRegistrationService` in `Maliev.ComplianceService.Infrastructure/IAM/ComplianceIAMRegistrationService.cs` to register compliance.manage and compliance.reports permissions with IAM service on startup
- [X] T117 [P] Add business metrics instrumentation in `Maliev.ComplianceService.Api/Program.cs` using AddServiceMeters("compliance-service")
- [X] T118 [P] Add Prometheus metrics for work authorizations by status, alerts by severity, report requests, expiring authorization counts (Constitution XII)
- [X] T119 [P] Add health check endpoints /compliance/liveness and /compliance/readiness in Program.cs via MapDefaultEndpoints
- [X] T120 [P] Add IAM registration service to DI container in Program.cs

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T121 [P] Create README.md at repository root with service description, prerequisites, quick start reference
- [X] T122 [P] Add XML documentation comments to all public APIs for Scalar documentation
- [X] T123 [P] Add API versioning configuration in Program.cs using AddDefaultApiVersioning
- [X] T124 [P] Add CORS configuration in Program.cs using AddDefaultCors
- [X] T125 [P] Add rate limiting configuration in Program.cs using AddStandardRateLimiting
- [X] T126 [P] Add structured logging with correlation IDs throughout all handlers and services
- [X] T127 [P] Add validation for document number format (FR-024) per issuing authority standards in RecordWorkAuthorizationValidator
- [X] T128 Code cleanup: Remove unused using statements, ensure zero warnings (Constitution VIII)
- [X] T129 Performance optimization: Review and optimize database queries with AsNoTracking where appropriate
- [X] T130 Security review: Ensure all endpoints have proper authorization attributes
- [X] T131 Run quickstart.md validation: Follow local development setup and verify all steps work
- [X] T132 [P] Create CI/CD workflows `.github/workflows/ci-develop.yml`, `ci-staging.yml`, `ci-main.yml`
- [X] T133 [P] Verify Dockerfile builds successfully with BuildKit secrets for NuGet credentials
- [X] T134 [P] Verify Testcontainers tests pass in CI environment