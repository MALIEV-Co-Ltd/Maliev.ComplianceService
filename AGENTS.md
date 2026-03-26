# Agent Guidelines for Maliev.ComplianceService

This repository contains the `Maliev.ComplianceService`, a .NET 10 Web API built with Clean Architecture principles.

## 1. Build & Verification

### Build
- **Build Solution:** `dotnet build`
- **Build Specific Project:** `dotnet build Maliev.ComplianceService.Api`

### Tests
The solution uses **xUnit** for testing.

- **Run All Tests:** `dotnet test`
- **Run Specific Test Project:** `dotnet test Maliev.ComplianceService.Tests`
- **Run Single Test:**
  ```bash
  dotnet test --filter "FullyQualifiedName=Namespace.ClassName.MethodName"
  ```
  *Example:*
  `dotnet test --filter "FullyQualifiedName=Maliev.ComplianceService.Tests.Unit.Commands.MyCommandTests.Handle_ValidRequest_Success"`

### Linting & Formatting
- **Format Code:** `dotnet format`
- **Verify Formatting:** `dotnet format --verify-no-changes`

## 2. Code Style & Conventions

### Architecture
- **Pattern:** Clean Architecture (Api, Application, Domain, Infrastructure).
- **CQRS:** Use **MediatR** for Commands and Queries.
  - **Commands:** Mutate state. Return `Unit` or created resource DTO.
  - **Queries:** Read-only. Return DTOs.
- **Validation:** Use Data Annotations on Entities. Manual validation in Application layer.

### Syntax & Features
- **Target Framework:** .NET 10
- **Namespaces:** Use **file-scoped namespaces** (`namespace My.Namespace;`).
- **Nullability:** Enabled. Use `string?` for nullable strings.
- **Records:** Use `public record` for DTOs, Commands, and Queries.
- **Entities:** Standard `class` with getters/setters. Use `[Required]`, `[MaxLength]` annotations.

### Naming
- **Classes/Methods:** `PascalCase`
- **Variables/Params:** `camelCase`
- **Private Fields:** `_camelCase` (underscore prefix)
- **Interfaces:** `IPascalCase`
- **Async Methods:** Suffix with `Async` generally preferred for I/O operations (check surrounding code).

### Imports (Usings)
- Place `using` directives at the top of the file.
- Remove unused usings.
- `ImplicitUsings` are enabled, so standard system namespaces may not be needed.

### Error Handling
- Use structured exception handling.
- meaningful error messages in exceptions.

## 3. Agent Operational Rules

- **Testing:**
  - ALWAYS run tests after modifying code to ensure no regressions.
  - If adding a feature, add a corresponding unit test in `Maliev.ComplianceService.Tests`.
  - Use `dotnet test` output to identify failures.

- **File Operations:**
  - Use **absolute paths** for all file reads/writes.
  - Verify file existence before reading.

- **Dependencies:**
  - Check `Directory.Build.props` for version overrides before adding packages.
  - Prefer existing libraries over adding new ones.

- **Documentation:**
  - Maintain XML comments (`/// <summary>`) on public members, especially in Domain and Application layers.


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
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project Maliev.<Domain>Service.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
