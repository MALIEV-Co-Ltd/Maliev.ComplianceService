# Compliance Service

Dedicated microservice for tracking employee work authorizations and ensuring regulatory compliance for Maliev Co. Ltd.

## Overview

The Compliance Service maintains essential legal documentation and monitoring:

- **Work Authorization** - Recording and tracking visas, work permits, and residency status.
- **Expiration Monitoring** - Automated alerts for documents approaching their expiry dates.
- **Compliance Alerts** - Generating system notifications for unresolved compliance issues.
- **Reporting** - Organization-wide compliance status and upcoming expiration reports.

## Architecture

- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL 18 with Entity Framework Core
- **Messaging**: RabbitMQ via MassTransit

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 18
- Docker (optional, for Redis and RabbitMQ)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/MALIEV-Co-Ltd/Maliev.ComplianceService.git
   ```

2. **Run database migrations**
   ```bash
   dotnet ef database update --project Maliev.ComplianceService.Infrastructure --startup-project Maliev.ComplianceService.Api
   ```

3. **Run the service**
   ```bash
   dotnet run --project Maliev.ComplianceService.Api
   ```

   The service will be available at `http://localhost:5205`.

## API Endpoints

### Work Authorization

```
GET  /employees/{employeeId}/work-authorization - Get employee authorizations
POST /employees/{employeeId}/work-authorization - Record new authorization
GET  /work-authorization/{authId} - Get specific record details
PUT  /work-authorization/{authId} - Update authorization
```

### Alerts & Reports

```
GET /work-authorization/expiring - List records approaching expiration
GET /reports/compliance - Generate organization-wide status report
```

## Integration Events Consumed

- `EmployeeCreatedIntegrationEvent` - Establishes compliance context for new hires.
- `EmployeeTerminatedIntegrationEvent` - Deactivates work authorizations for departing employees.

## License

Copyright © 2025 Maliev Co. Ltd. All rights reserved.