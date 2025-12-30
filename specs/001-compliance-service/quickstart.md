# Quick Start: Compliance Service

**Date**: 2025-12-28
**Branch**: 001-compliance-service

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for Testcontainers)
- PostgreSQL client (optional, for manual DB inspection)
- Git
- IDE: Visual Studio 2025, VS Code, or Rider

---

## Local Development Setup

### 1. Clone Repository

```bash
git clone https://github.com/MALIEV-Co-Ltd/Maliev.ComplianceService.git
cd Maliev.ComplianceService
git checkout 001-compliance-service
```

### 2. Configure NuGet Package Source

The project requires `Maliev.Aspire.ServiceDefaults` from GitHub Packages.

**Create or update `nuget.config`** at repository root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/MALIEV-Co-Ltd/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="%NUGET_USERNAME%" />
      <add key="ClearTextPassword" value="%NUGET_PASSWORD%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

**Set environment variables**:

```bash
# Windows (PowerShell)
$env:NUGET_USERNAME="your-github-username"
$env:NUGET_PASSWORD="your-github-pat"  # PAT with read:packages scope

# macOS/Linux
export NUGET_USERNAME="your-github-username"
export NUGET_PASSWORD="your-github-pat"
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Configure Local Settings

**Create `Maliev.ComplianceService.Api/appsettings.Development.json`**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.AspNetCore.Watch.BrowserRefresh": "None",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.AspNetCore.Watch": "Warning",
      "System": "Warning"
    }
  },
  "ConnectionStrings": {
    "ComplianceDbContext": "Host=localhost;Port=5432;Database=compliance;Username=postgres;Password=postgres"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "ExternalServices": {
    "EmployeeService": {
      "BaseUrl": "http://localhost:8081"
    },
    "NotificationService": {
      "BaseUrl": "http://localhost:8082"
    },
    "IAMService": {
      "BaseUrl": "http://localhost:8083"
    }
  }
}
```

### 5. Run Infrastructure with Docker

**Create `docker-compose.dev.yml`** at repository root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:18
    environment:
      POSTGRES_DB: compliance
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  redis:
    image: redis:7
    ports:
      - "6379:6379"

volumes:
  postgres-data:
```

**Start infrastructure**:

```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Verify services are running**:

```bash
docker ps  # Should show 3 containers
```

### 6. Apply Database Migrations

```bash
cd Maliev.ComplianceService.Api
dotnet ef database update
```

**Verify migration**:

```bash
psql -h localhost -U postgres -d compliance -c "\dt"
# Should show: work_authorizations, compliance_alerts tables
```

### 7. Run the Service

```bash
dotnet run
```

**Verify service is running**:

```bash
curl http://localhost:8080/compliance/liveness
# Expected: HTTP 200 OK

curl http://localhost:8080/compliance/readiness
# Expected: HTTP 200 OK (all dependencies healthy)
```

### 8. Access API Documentation

Open browser to:
- **Scalar UI**: `http://localhost:8080/compliance/scalar/v1`
- **OpenAPI Spec**: `http://localhost:8080/compliance/openapi/v1.json`

---

## Running Tests

### Unit Tests

```bash
dotnet test --filter "Category=Unit"
```

### Integration Tests (with Testcontainers)

**Note**: Docker must be running

```bash
dotnet test --filter "Category=Integration"
```

**What happens**:
1. Testcontainers starts PostgreSQL, RabbitMQ, Redis containers
2. Tests run against real infrastructure
3. Containers are automatically cleaned up after tests

### All Tests

```bash
dotnet test
```

---

## Development Workflow

### 1. Create New Work Authorization

```bash
curl -X POST http://localhost:8080/compliance/v1/employees/{employeeId}/work-authorization \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {jwt-token}" \
  -d '{
    "authorizationType": "WorkVisa",
    "documentNumber": "EAC1234567890",
    "issueDate": "2024-01-15T00:00:00Z",
    "expirationDate": "2027-01-14T00:00:00Z",
    "issuingAuthority": "USCIS",
    "sponsorshipStatus": "Sponsored",
    "rightToWorkDocumentId": "uuid",
    "notes": "H-1B visa valid for 3 years"
  }'
```

### 2. Get Expiring Authorizations

```bash
curl -X GET "http://localhost:8080/compliance/v1/work-authorization/expiring?daysUntilExpiration=90" \
  -H "Authorization: Bearer {jwt-token}"
```

### 3. View Compliance Report

```bash
curl -X GET http://localhost:8080/compliance/v1/reports/compliance \
  -H "Authorization: Bearer {jwt-token}"
```

### 4. Trigger Background Jobs Manually

```bash
# Expiration Reminder Service (normally runs at 7 AM)
curl -X POST http://localhost:8080/compliance/admin/trigger-expiration-reminder \
  -H "Authorization: Bearer {admin-jwt-token}"

# Expired Flagging Service (normally runs at midnight)
curl -X POST http://localhost:8080/compliance/admin/trigger-expired-flagging \
  -H "Authorization: Bearer {admin-jwt-token}"
```

---

## Debugging

### View Database State

```bash
# Connect to PostgreSQL
psql -h localhost -U postgres -d compliance

# Query work authorizations
SELECT id, employee_id, authorization_type, compliance_status, expiration_date
FROM work_authorizations
WHERE is_active = TRUE
ORDER BY expiration_date;

# Query alerts
SELECT id, employee_id, alert_type, severity, is_resolved
FROM compliance_alerts
WHERE is_resolved = FALSE
ORDER BY created_date DESC;
```

### View RabbitMQ Messages

Open RabbitMQ Management UI:
- URL: `http://localhost:15672`
- Username: `guest`
- Password: `guest`

Navigate to **Queues** tab to inspect messages.

### View Redis Cache

```bash
# Connect to Redis CLI
redis-cli

# View all keys
KEYS *

# Get cached compliance summary
GET compliance:summary

# Clear cache
FLUSHDB
```

### View Logs

Logs are output to console in structured JSON format:

```bash
dotnet run | jq '.'  # Pretty-print JSON logs (requires jq)
```

---

## Common Issues

### Issue: "Unable to connect to PostgreSQL"

**Solution**:
```bash
docker ps  # Verify postgres container is running
docker logs <postgres-container-id>  # Check for errors
```

### Issue: "NuGet restore failed for Maliev.Aspire.ServiceDefaults"

**Solution**:
1. Verify GitHub PAT has `read:packages` scope
2. Check environment variables are set:
   ```bash
   echo $NUGET_USERNAME
   echo $NUGET_PASSWORD
   ```
3. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

### Issue: "Testcontainers timeout"

**Solution**:
1. Verify Docker is running
2. Increase timeout in test configuration
3. Check Docker has sufficient resources (4GB RAM minimum)

### Issue: "Optimistic concurrency exception"

**Solution**:
This is expected when two updates conflict. To test:
1. GET work authorization (note `rowVersion`)
2. Update in UI or another API call
3. Try to update with old `rowVersion`
4. Expect `CONCURRENT_MODIFICATION` error
5. Refresh data and retry update

---

## Database Seeding (Development Only)

**Create `Maliev.ComplianceService.Api/Data/DevDataSeeder.cs`**:

```csharp
public static class DevDataSeeder
{
    public static async Task SeedAsync(ComplianceDbContext context)
    {
        if (await context.WorkAuthorizations.AnyAsync())
            return; // Already seeded

        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();

        var authorizations = new[]
        {
            new WorkAuthorization
            {
                EmployeeId = employeeId1,
                AuthorizationType = AuthorizationType.WorkVisa,
                DocumentNumber = "EAC1111111111",
                IssueDate = DateTime.UtcNow.AddYears(-2),
                ExpirationDate = DateTime.UtcNow.AddDays(45), // Expiring soon
                IssuingAuthority = "USCIS",
                SponsorshipStatus = SponsorshipStatus.RenewalPending,
                IsActive = true,
                ComplianceStatus = ComplianceStatus.ExpiringSoon
            },
            new WorkAuthorization
            {
                EmployeeId = employeeId2,
                AuthorizationType = AuthorizationType.PermanentResident,
                DocumentNumber = "I-551-222222",
                IssueDate = DateTime.UtcNow.AddYears(-5),
                ExpirationDate = null, // No expiration
                IssuingAuthority = "USCIS",
                SponsorshipStatus = SponsorshipStatus.NotRequired,
                IsActive = true,
                ComplianceStatus = ComplianceStatus.Compliant
            }
        };

        context.WorkAuthorizations.AddRange(authorizations);
        await context.SaveChangesAsync();
    }
}
```

**Call in `Program.cs` (Development only)**:

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
    await DevDataSeeder.SeedAsync(context);
}
```

---

## Next Steps

1. **Read specification**: `specs/001-compliance-service/spec.md`
2. **Review data model**: `specs/001-compliance-service/data-model.md`
3. **Study API contracts**: `specs/001-compliance-service/contracts/openapi.yaml`
4. **Run integration events examples**: See `contracts/integration-events.md`
5. **Start implementing**: Begin with Phase 2 tasks in `tasks.md`

---

## Support

- **Documentation**: `specs/001-compliance-service/`
- **Issues**: GitHub Issues
- **Team Chat**: Slack #compliance-service channel
