# Agent Guidelines for Maliev.ComplianceService

This repository contains the `Maliev.ComplianceService`, a .NET 10 Web API built with Clean Architecture principles.

## 1. Build & Verification

### Build
- **Build Solution:** `dotnet build Maliev.ComplianceService.slnx`
- **Build Specific Project:** `dotnet build Maliev.ComplianceService.Api`

### Tests
The solution uses **xUnit** for testing.

- **Run All Tests:** `dotnet test Maliev.ComplianceService.slnx --verbosity normal`
- **Run with Code Coverage:** `dotnet test Maliev.ComplianceService.slnx --collect:"XPlat Code Coverage"`
- **Run Single Test:**
  ```bash
  dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
  ```
  *Example:*
  `dotnet test --filter "FullyQualifiedName~Maliev.ComplianceService.Tests.Unit.Commands.MyCommandTests.Handle_ValidRequest_Success"`

### Linting & Formatting
- **Format Code:** `dotnet format Maliev.ComplianceService.slnx`
- **Verify Formatting:** `dotnet format Maliev.ComplianceService.slnx --verify-no-changes`

## 2. Code Style & Conventions

### Architecture
- **Pattern:** Clean Architecture (Api, Application, Domain, Infrastructure).
- **CQRS:** Use **MediatR** for Commands and Queries.
  - **Commands:** Mutate state. Return `Unit` or created resource DTO.
  - **Queries:** Read-only. Return DTOs.
- **Validation:** Use `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned.

### Syntax & Features
- **Target Framework:** .NET 10
- **Namespaces:** File-scoped (`namespace Maliev.ComplianceService.Domain.Entities;`)
- **Nullability:** Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Records:** Use `public record` for DTOs, Commands, and Queries.
- **Entities:** Standard `class` with getters/setters. Use `[Required]`, `[MaxLength]` annotations.

### C# Naming & Formatting
- **Classes/Methods/Properties:** `PascalCase`
- **Private fields:** `_camelCase` (underscore prefix)
- **Parameters/locals:** `camelCase`
- **Async methods:** Suffix with `Async` (e.g., `AuthenticateAsync`)
- **Interfaces:** Prefix with `I` (e.g., `IAuthenticationService`)
- **Permissions:** GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `compliance.audits.create`, `compliance.rules.update`
  - Invalid: `compliance.audit.create` (singular), `compliance.create` (missing resource)
- **XML docs:** Required on ALL public methods and properties
- **Imports:** System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces:** Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation:** 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI:** Constructor injection with `private readonly` fields
- **Controllers:** `[ApiController]`, `[ApiVersion("1")]`, `[Route("compliance/v{version:apiVersion}")]`
- **Logging:** `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {FileId}", fileId)`
- **Error handling:** Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **Manual mapping:** Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation:** `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

## 3. Agent Operational Rules

- **Testing:**
  - ALWAYS run tests after modifying code to ensure no regressions.
  - If adding a feature, add a corresponding unit test in `Maliev.ComplianceService.Tests`.
  - Use `dotnet test` output to identify failures.

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

#### Testing Rules
- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests
- Every MassTransit consumer MUST have a consumer test using `services.AddMassTransitTestHarness()`

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

- **File Operations:**
  - Use **absolute paths** for all file reads/writes.
  - Verify file existence before reading.

- **Dependencies:**
  - Check `Directory.Build.props` for version overrides before adding packages.
  - Prefer existing libraries over adding new ones.

- **Documentation:**
  - Maintain XML comments (`/// <summary>`) on public members, especially in Domain and Application layers.

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/compliance/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

## Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("compliance.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/compliance`
- **Scalar docs**: Configured at `/compliance/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

## Git & Version Control — Mandatory Rules

### 🚨 CRITICAL: Always Commit Code Changes (Non-Negotiable)
- **You MUST commit your changes to the local repository after completing any meaningful unit of work.**
- **Never accumulate uncommitted changes.** Do not wait until end of session or until something breaks.
- **Commit early and often** — if a change is meaningful (even a small fix or refactor), commit it.
- **You do NOT need to push to remote** — local commits are sufficient to protect against accidental loss.
- **If you are unsure whether to commit, commit anyway.** Extra commits are harmless; lost work is irreversible.
- This rule applies even if you are just "testing" or "exploring" — use git branches to isolate experimental work and commit those changes too.

### 🚨 CRITICAL: Never Use `git checkout` to Restore Broken Files
- **NEVER use `git checkout` to restore or recover files.** This operation discards uncommitted changes permanently and will result in data loss.
- **To undo/recover from broken files: first commit your current changes, then use `git revert` or `git reset --soft` to safely undo.**

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.ComplianceService.Infrastructure --startup-project Maliev.ComplianceService.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
