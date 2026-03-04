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
