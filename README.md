# Maliev Compliance Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.ComplianceService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Dedicated microservice for tracking employee work authorizations and ensuring regulatory compliance.

**Role in MALIEV Architecture**: Acts as the authoritative monitor for legal work status. It tracks visas, work permits, and residency documents, ensuring the organization remains compliant with local labor laws through proactive monitoring and alerting.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (Compliance status caching)
- **Messaging**: RabbitMQ via MassTransit
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **Work Authorization Tracking**: Specialized tracking for visas, work permits, and residency certificates.
- **Expiration Intelligence**: Automated monitoring of document expiry dates with configurable alert thresholds.
- **Proactive Compliance Alerts**: System-generated tasks and notifications for approaching document expirations.
- **Compliance Reporting**: Organization-wide visibility into work authorization status and upcoming risks.
- **Document Metadata Management**: Detailed recording of document issuers, numbers, and legal scopes.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.ComplianceService.git
cd Maliev.ComplianceService
```

2. **Spin up Infrastructure**
```bash
docker run --name compliance-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name compliance-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__ComplianceDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.ComplianceService.Api
dotnet run --project Maliev.ComplianceService.Api
```

The service will be available at `http://localhost:5000/compliance`. Access the interactive documentation at `http://localhost:5000/compliance/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/compliance/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/employees/{id}/work-authorization` | Get employee compliance records |
| POST | `/employees/{id}/work-authorization` | Record new work permit/visa |
| GET | `/work-authorization/expiring` | List records approaching expiration |
| GET | `/reports/compliance` | Generate organization status report |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /compliance/liveness`
- **Readiness**: `GET /compliance/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /compliance/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-compliance-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.