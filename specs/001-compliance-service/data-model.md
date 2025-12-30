# Data Model: Compliance Service

**Date**: 2025-12-28
**Branch**: 001-compliance-service

## Entity Relationship Diagram

```
┌─────────────────────────┐         ┌──────────────────────────┐
│   WorkAuthorization     │1      *│    ComplianceAlert       │
│─────────────────────────│◄────────│──────────────────────────│
│ Id (PK)                 │         │ Id (PK)                  │
│ EmployeeId              │         │ WorkAuthorizationId (FK) │
│ AuthorizationType       │         │ EmployeeId               │
│ DocumentNumber          │         │ AlertType                │
│ IssueDate               │         │ Severity                 │
│ ExpirationDate          │         │ Message                  │
│ IssuingAuthority        │         │ IsResolved               │
│ SponsorshipStatus       │         │ ResolvedDate             │
│ RightToWorkDocumentId   │         │ ResolvedBy               │
│ Notes                   │         │ ResolutionNotes          │
│ IsActive                │         │ CreatedDate              │
│ ComplianceStatus        │         └──────────────────────────┘
│ CreatedDate             │
│ ModifiedDate            │
│ RowVersion              │
└─────────────────────────┘
```

---

## Entities

### WorkAuthorization

**Purpose**: Represents an employee's legal authorization to work

**Table**: `work_authorizations`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY, DEFAULT gen_random_uuid() | Unique identifier |
| `employee_id` | UUID | NOT NULL, INDEX | Reference to employee in Employee Service |
| `authorization_type` | INTEGER | NOT NULL | Enum: AuthorizationType |
| `document_number` | VARCHAR(100) | NOT NULL | Official document number (e.g., EAC1234567890) |
| `issue_date` | TIMESTAMP WITH TIME ZONE | NOT NULL | When authorization was issued |
| `expiration_date` | TIMESTAMP WITH TIME ZONE | NULLABLE | When authorization expires (null for permanent residents) |
| `issuing_authority` | VARCHAR(200) | NULLABLE | Authority that issued authorization (e.g., USCIS) |
| `sponsorship_status` | INTEGER | NULLABLE | Enum: SponsorshipStatus |
| `right_to_work_document_id` | UUID | NULLABLE | Reference to document in Upload Service |
| `notes` | TEXT | NULLABLE | Additional notes |
| `is_active` | BOOLEAN | NOT NULL, DEFAULT TRUE, INDEX | Active/inactive status |
| `compliance_status` | INTEGER | NOT NULL, DEFAULT 0, INDEX | Enum: ComplianceStatus |
| `created_date` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Audit: creation timestamp |
| `modified_date` | TIMESTAMP WITH TIME ZONE | NULLABLE | Audit: last modification timestamp |
| `row_version` | BYTEA | NOT NULL | Optimistic concurrency token (EF Core Timestamp) |

**Indexes**:
- `idx_work_auth_employee`: ON (employee_id) - Fast employee lookup
- `idx_work_auth_expiration`: ON (expiration_date) - Background job queries
- `idx_work_auth_status`: ON (compliance_status) - Report generation
- `idx_work_auth_active`: ON (is_active) WHERE is_active = TRUE - Partial index for active records

**Unique Constraint**:
- `uq_active_auth_per_type`: UNIQUE (employee_id, authorization_type) WHERE is_active = TRUE
  - Enforces FR-003: Only one active authorization per type per employee

**Validation Rules** (from FR-001 to FR-005):
- `issue_date` must be <= `expiration_date` (FR-004)
- `right_to_work_document_id` required when `authorization_type` != Citizen (FR-005)
- `document_number` format validated per `issuing_authority` standards (FR-024)
- `compliance_status` auto-calculated from `expiration_date` (FR-002)

**State Transitions** (ComplianceStatus):
```
PendingVerification (3) → Initial creation
     ↓
Compliant (0) → expiration_date > NOW() + 90 days OR expiration_date IS NULL
     ↓
ExpiringSoon (1) → expiration_date BETWEEN NOW() AND NOW() + 90 days
     ↓
Expired (2) → expiration_date < NOW()
     ↓
NonCompliant (4) → Expired + 30 days (triggers AccessRevocationRequiredEvent)
```

---

### ComplianceAlert

**Purpose**: Represents a notification about a compliance issue

**Table**: `compliance_alerts`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY, DEFAULT gen_random_uuid() | Unique identifier |
| `work_authorization_id` | UUID | NOT NULL, FOREIGN KEY, INDEX | References work_authorizations(id) |
| `employee_id` | UUID | NOT NULL, INDEX | Denormalized for fast employee lookups |
| `alert_type` | INTEGER | NOT NULL | Enum: AlertType |
| `severity` | INTEGER | NOT NULL | Enum: AlertSeverity |
| `message` | TEXT | NOT NULL | Human-readable alert message |
| `is_resolved` | BOOLEAN | NOT NULL, DEFAULT FALSE, INDEX | Resolution status |
| `resolved_date` | TIMESTAMP WITH TIME ZONE | NULLABLE | When alert was resolved |
| `resolved_by` | UUID | NULLABLE | User ID who resolved the alert |
| `resolution_notes` | TEXT | NULLABLE | Notes from resolver |
| `created_date` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Alert creation timestamp |

**Indexes**:
- `idx_alerts_auth`: ON (work_authorization_id) - Authorization's alerts
- `idx_alerts_employee`: ON (employee_id) - Employee's alerts
- `idx_alerts_unresolved`: ON (is_resolved) WHERE is_resolved = FALSE - Active alerts only

**Retention Policy** (from FR-013):
- Resolved alerts retained indefinitely for audit trail
- No automatic deletion
- Retrieval available via FR-013a filters

**Alert Message Templates**:
- ExpirationWarning (90 days): "Work authorization {type} expires in 90 days on {date}"
- ExpirationWarning (60 days): "Work authorization {type} expires in 60 days on {date}"
- ExpirationWarning (30 days): "Work authorization {type} expires in 30 days on {date}"
- Expired: "Work authorization {type} expired on {date}"
- DocumentMissing: "Right-to-work document missing for {type} authorization"
- VerificationRequired: "Work authorization requires verification"
- RenewalReminder: "Renewal required for {type} expiring {date}"

---

## Enumerations

### AuthorizationType

```csharp
public enum AuthorizationType
{
    Citizen = 0,
    PermanentResident = 1,
    WorkVisa = 2,
    StudentVisa = 3,
    TemporaryWorker = 4,
    Refugee = 5,
    Asylee = 6,
    Other = 7
}
```

**Usage**: Determines compliance requirements and document needs
- Citizen: No expiration tracking
- Others: Expiration monitoring enabled

---

### SponsorshipStatus

```csharp
public enum SponsorshipStatus
{
    NotRequired = 0,      // Citizen or permanent resident
    Sponsored = 1,         // Currently sponsored by organization
    TransferPending = 2,   // Sponsorship transfer in progress
    RenewalPending = 3,    // Renewal application submitted
    Expired = 4            // Sponsorship expired
}
```

**Usage**: Affects renewal processes and responsibilities

---

### ComplianceStatus

```csharp
public enum ComplianceStatus
{
    Compliant = 0,             // > 90 days or no expiration
    ExpiringSoon = 1,          // 30-90 days until expiration
    Expired = 2,               // Past expiration date
    PendingVerification = 3,   // Newly created, awaiting verification
    NonCompliant = 4           // Expired beyond grace period
}
```

**Calculation Logic** (FR-002):
```csharp
public static ComplianceStatus CalculateStatus(DateTime? expirationDate)
{
    if (!expirationDate.HasValue)
        return ComplianceStatus.Compliant; // No expiration (e.g., citizen)

    var daysUntilExpiration = (expirationDate.Value - DateTime.UtcNow).Days;

    if (daysUntilExpiration < 0)
        return ComplianceStatus.Expired;
    if (daysUntilExpiration <= 90)
        return ComplianceStatus.ExpiringSoon;

    return ComplianceStatus.Compliant;
}
```

---

### AlertType

```csharp
public enum AlertType
{
    ExpirationWarning = 0,     // 30/60/90 days warning
    Expired = 1,               // Authorization has expired
    DocumentMissing = 2,       // Right-to-work document not provided
    VerificationRequired = 3,  // Manual verification needed
    RenewalReminder = 4        // Time to renew
}
```

**Severity Mapping** (FR-009):
- 90 days → Medium
- 60 days → High
- 30 days → Critical
- Expired → Critical

---

### AlertSeverity

```csharp
public enum AlertSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
```

**Usage**: Determines alert priority and escalation

---

## Data Transfer Objects (DTOs)

### RecordWorkAuthorizationRequest

```csharp
public record RecordWorkAuthorizationRequest
{
    [Required]
    public AuthorizationType AuthorizationType { get; init; }

    [Required, StringLength(100)]
    public string DocumentNumber { get; init; }

    [Required]
    public DateTime IssueDate { get; init; }

    public DateTime? ExpirationDate { get; init; }

    [StringLength(200)]
    public string? IssuingAuthority { get; init; }

    public SponsorshipStatus? SponsorshipStatus { get; init; }

    public Guid? RightToWorkDocumentId { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }
}
```

**Validation**:
- IssueDate <= ExpirationDate (custom validator)
- RightToWorkDocumentId required if AuthorizationType != Citizen

---

### WorkAuthorizationResponse

```csharp
public record WorkAuthorizationResponse
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } // From Employee Service or "(name unavailable)"
    public AuthorizationType AuthorizationType { get; init; }
    public string DocumentNumber { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public int? DaysUntilExpiration { get; init; } // Calculated
    public string? IssuingAuthority { get; init; }
    public SponsorshipStatus? SponsorshipStatus { get; init; }
    public ComplianceStatus ComplianceStatus { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? ModifiedDate { get; init; }
}
```

---

### ComplianceAlertResponse

```csharp
public record ComplianceAlertResponse
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; }
    public AlertType AlertType { get; init; }
    public AlertSeverity Severity { get; init; }
    public string Message { get; init; }
    public bool IsResolved { get; init; }
    public DateTime? ResolvedDate { get; init; }
    public string? ResolvedByName { get; init; }
    public string? ResolutionNotes { get; init; }
    public DateTime CreatedDate { get; init; }
}
```

---

### ComplianceReportResponse

```csharp
public record ComplianceReportResponse
{
    public DateTime ReportDate { get; init; }
    public int TotalEmployees { get; init; }
    public int RequiresAuthorization { get; init; }
    public int Compliant { get; init; }
    public int ExpiringSoon { get; init; }
    public int Expired { get; init; }
    public decimal ComplianceRate { get; init; } // Percentage
    public List<AuthorizationTypeBreakdown> ByAuthorizationType { get; init; }
    public List<ComplianceAlertResponse> Alerts { get; init; }
}

public record AuthorizationTypeBreakdown
{
    public AuthorizationType Type { get; init; }
    public int Count { get; init; }
    public int ExpiringSoon { get; init; }
    public int Expired { get; init; }
}
```

---

## Database Migrations

### Initial Migration

```sql
-- Create work_authorizations table
CREATE TABLE work_authorizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    authorization_type INTEGER NOT NULL,
    document_number VARCHAR(100) NOT NULL,
    issue_date TIMESTAMP WITH TIME ZONE NOT NULL,
    expiration_date TIMESTAMP WITH TIME ZONE,
    issuing_authority VARCHAR(200),
    sponsorship_status INTEGER,
    right_to_work_document_id UUID,
    notes TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    compliance_status INTEGER NOT NULL DEFAULT 0,
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    modified_date TIMESTAMP WITH TIME ZONE,
    row_version BYTEA NOT NULL DEFAULT '\x0000000000000001'
);

-- Create indexes
CREATE INDEX idx_work_auth_employee ON work_authorizations(employee_id);
CREATE INDEX idx_work_auth_expiration ON work_authorizations(expiration_date);
CREATE INDEX idx_work_auth_status ON work_authorizations(compliance_status);
CREATE INDEX idx_work_auth_active ON work_authorizations(is_active) WHERE is_active = TRUE;

-- Create unique constraint for active authorizations
CREATE UNIQUE INDEX uq_active_auth_per_type
    ON work_authorizations(employee_id, authorization_type)
    WHERE is_active = TRUE;

-- Create compliance_alerts table
CREATE TABLE compliance_alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_authorization_id UUID NOT NULL REFERENCES work_authorizations(id) ON DELETE CASCADE,
    employee_id UUID NOT NULL,
    alert_type INTEGER NOT NULL,
    severity INTEGER NOT NULL,
    message TEXT NOT NULL,
    is_resolved BOOLEAN NOT NULL DEFAULT FALSE,
    resolved_date TIMESTAMP WITH TIME ZONE,
    resolved_by UUID,
    resolution_notes TEXT,
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Create indexes for alerts
CREATE INDEX idx_alerts_auth ON compliance_alerts(work_authorization_id);
CREATE INDEX idx_alerts_employee ON compliance_alerts(employee_id);
CREATE INDEX idx_alerts_unresolved ON compliance_alerts(is_resolved) WHERE is_resolved = FALSE;
```

---

## Data Import from Employee Service

### Export Query

```sql
-- Export from Employee Service database
COPY (
    SELECT
        id,
        employee_id,
        authorization_type,
        document_number,
        issue_date,
        expiration_date,
        issuing_authority,
        sponsorship_status,
        right_to_work_document_id,
        notes,
        is_active,
        created_date,
        modified_date
    FROM work_authorizations
    ORDER BY employee_id, created_date
) TO '/tmp/work_authorizations.csv' WITH CSV HEADER;
```

### Import and Status Calculation

```sql
-- Import into Compliance Service database
COPY work_authorizations(
    id,
    employee_id,
    authorization_type,
    document_number,
    issue_date,
    expiration_date,
    issuing_authority,
    sponsorship_status,
    right_to_work_document_id,
    notes,
    is_active,
    created_date,
    modified_date
) FROM '/tmp/work_authorizations.csv' CSV HEADER;

-- Calculate initial compliance status for all records
UPDATE work_authorizations
SET compliance_status = CASE
    WHEN expiration_date IS NULL THEN 0  -- Compliant (no expiration)
    WHEN expiration_date < NOW() THEN 2  -- Expired
    WHEN expiration_date < NOW() + INTERVAL '90 days' THEN 1  -- ExpiringSoon
    ELSE 0  -- Compliant
END;

-- Initialize row_version for optimistic concurrency
UPDATE work_authorizations
SET row_version = '\x0000000000000001'
WHERE row_version IS NULL;
```

---

## Performance Considerations

### Query Patterns

**Most Common Queries** (optimize with indexes):
1. Get active authorizations for employee: `WHERE employee_id = ? AND is_active = TRUE`
2. Get expiring authorizations: `WHERE expiration_date BETWEEN ? AND ? AND is_active = TRUE`
3. Get unresolved alerts: `WHERE is_resolved = FALSE`
4. Compliance report by status: `WHERE compliance_status = ? AND is_active = TRUE`

### Expected Data Volumes

- **Work Authorizations**: ~15-20% of total employees (e.g., 10K out of 50K)
- **Compliance Alerts**: ~1-2 alerts per authorization over lifetime
- **Growth Rate**: Linear with employee count

### Retention

- **Work Authorizations**: Retain indefinitely (audit requirements)
- **Compliance Alerts**: Retain indefinitely (FR-013)
- **Inactive Authorizations**: Soft delete (is_active = FALSE)
