# Feature Specification: Compliance Service

**Feature Branch**: `001-compliance-service`
**Created**: 2025-12-28
**Status**: Draft
**Input**: User description: "Compliance Service for managing work authorizations, visas, permits, and employment compliance tracking"

## Clarifications

### Session 2025-12-28

- Q: The specification states alerts should be created for authorizations expiring in "exactly 30, 60, and 90 days" (FR-009). How should "exactly" be interpreted for the daily job running at 7 AM? → A: Day window - Alert if remaining days fall within 30±0, 60±0, 90±0 at time of service run (standard interpretation - alerts generated once in the target window)
- Q: The spec mentions resolving compliance alerts (FR-013, User Story 6), but doesn't specify retention policy for resolved alerts. Should resolved alerts be retained, and if so, for how long? → A: Retain indefinitely - All resolved alerts kept permanently for audit trail and historical compliance reporting
- Q: Multiple features require employee names from the Employee Service (FR-014, User Story 2, User Story 4). What should happen when the Employee Service is unavailable or an employee lookup fails? → A: Degrade gracefully - Display employee ID with indicator "(name unavailable)" and allow operation to complete with partial data
- Q: FR-018 states the system should publish AccessRevocationRequiredEvent "after grace period" and A-006 mentions a 30-day grace period. When exactly should this event be triggered? → A: 30 days after expiration - Event published when authorization has been expired for 30 days with no renewal
- Q: SC-010 states "System handles concurrent work authorization updates from multiple HR administrators without data conflicts." How should concurrent update conflicts be prevented or resolved? → A: Optimistic locking - Allow concurrent edits but detect conflicts on save using version/timestamp; reject stale updates

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record Work Authorization (Priority: P1)

As an HR administrator, I need to record employee work authorization details so that the organization maintains accurate compliance records and can monitor expiration dates.

**Why this priority**: This is the foundational capability that enables all other compliance tracking features. Without the ability to record work authorizations, no other functionality can operate.

**Independent Test**: Can be fully tested by creating an employee work authorization record with complete details (document number, dates, type) and verifying the record is stored and retrievable. Delivers immediate value by centralizing work authorization data.

**Acceptance Scenarios**:

1. **Given** I am an HR administrator viewing an employee profile, **When** I submit work authorization details including type, document number, issue date, expiration date, and issuing authority, **Then** the system creates a new work authorization record with compliance status automatically calculated
2. **Given** I attempt to create a work authorization with an issue date after the expiration date, **When** I submit the form, **Then** the system rejects the submission with error "INVALID_DATE_RANGE"
3. **Given** an employee already has an active work authorization of the same type, **When** I try to create another active authorization of that type, **Then** the system rejects with error "DUPLICATE_AUTHORIZATION"
4. **Given** I record a work authorization for a non-citizen without providing a right-to-work document, **When** I submit, **Then** the system requires the document reference before allowing submission

---

### User Story 2 - View Expiring Authorizations (Priority: P1)

As an HR administrator, I need to see which work authorizations are approaching expiration so that I can proactively work with employees to renew their documents and maintain compliance.

**Why this priority**: Proactive monitoring prevents compliance violations and ensures business continuity. This is critical for avoiding legal issues and employee disruptions.

**Independent Test**: Can be fully tested by querying the system for authorizations expiring within a specified timeframe (e.g., 90 days) and verifying the results are accurate and include key employee information. Delivers value by enabling proactive compliance management.

**Acceptance Scenarios**:

1. **Given** there are work authorizations in the system with various expiration dates, **When** I request authorizations expiring within 90 days, **Then** the system returns all authorizations with expiration dates between today and 90 days from now
2. **Given** I view the expiring authorizations list, **When** the data is displayed, **Then** each entry includes employee name, authorization type, document number, expiration date, days until expiration, and sponsorship status
3. **Given** an authorization expires in 45 days, **When** I view the list, **Then** the entry shows "45" in the days until expiration field

---

### User Story 3 - Receive Compliance Alerts (Priority: P1)

As an HR administrator, I need to receive automated alerts when work authorizations are expiring or have expired so that I can take timely action without manually checking dates.

**Why this priority**: Automated alerting ensures nothing falls through the cracks and reduces manual monitoring burden. Critical for maintaining continuous compliance.

**Independent Test**: Can be fully tested by creating authorization records with various expiration dates and verifying that alerts are automatically generated at the appropriate thresholds (90/60/30 days, and upon expiration). Delivers value through automated compliance monitoring.

**Acceptance Scenarios**:

1. **Given** a work authorization expires in exactly 90 days, **When** the daily alert service runs, **Then** a Medium severity alert is created with type "ExpirationWarning"
2. **Given** a work authorization expires in exactly 60 days, **When** the daily alert service runs, **Then** a High severity alert is created
3. **Given** a work authorization expires in exactly 30 days, **When** the daily alert service runs, **Then** a Critical severity alert is created
4. **Given** a work authorization has passed its expiration date, **When** the midnight service runs, **Then** an alert with severity "Critical" and type "Expired" is created
5. **Given** I receive a compliance alert, **When** I view the alert, **Then** it includes employee information, authorization details, alert type, severity, and a descriptive message

---

### User Story 4 - View Compliance Reports (Priority: P2)

As an HR manager, I need to view compliance summary reports showing the overall compliance status across the organization so that I can understand compliance health and report to leadership.

**Why this priority**: Reporting provides organizational visibility and supports strategic decision-making. While important, it's secondary to the core tracking and alerting capabilities.

**Independent Test**: Can be fully tested by requesting a compliance report and verifying it contains accurate statistics about total employees, compliance rates, authorizations by type, and active alerts. Delivers value through executive visibility into compliance status.

**Acceptance Scenarios**:

1. **Given** the organization has employees with various work authorization statuses, **When** I request a compliance report, **Then** the system returns totals for: total employees, employees requiring authorization, compliant count, expiring soon count, expired count, and overall compliance rate
2. **Given** the compliance report is generated, **When** I view the data, **Then** it includes a breakdown by authorization type showing counts of authorizations, how many are expiring soon, and how many are expired
3. **Given** there are active compliance alerts, **When** I view the report, **Then** it includes a list of current alerts with employee name, alert type, severity, and message
4. **Given** I want to see compliance for a specific department, **When** I request the report with a department filter, **Then** the system returns data for only that department

---

### User Story 5 - Update Work Authorization (Priority: P2)

As an HR administrator, I need to update existing work authorization records when employees renew or transfer their authorizations so that records remain current and accurate.

**Why this priority**: Updates are essential for maintaining data accuracy over time, but initial record creation and monitoring are more fundamental.

**Independent Test**: Can be fully tested by modifying an existing work authorization record (e.g., updating expiration date, changing sponsorship status) and verifying the changes are saved with modification timestamp. Delivers value by supporting the full authorization lifecycle.

**Acceptance Scenarios**:

1. **Given** I have an existing work authorization record, **When** I update the expiration date to a later date, **Then** the system saves the change, updates the compliance status if needed, and records the modification date
2. **Given** an employee transfers visa sponsorship, **When** I update the sponsorship status to "TransferPending", **Then** the record reflects the new status
3. **Given** I attempt to update a non-existent authorization, **When** I submit the update, **Then** the system returns error "AUTHORIZATION_NOT_FOUND"

---

### User Story 6 - Resolve Compliance Alerts (Priority: P3)

As an HR administrator, I need to mark compliance alerts as resolved once I've taken action so that my active alert list remains manageable and shows only items requiring attention.

**Why this priority**: While important for workflow management, alert resolution is a supporting function that doesn't impact core compliance tracking.

**Independent Test**: Can be fully tested by marking an active alert as resolved with notes and verifying it no longer appears in the active alerts list. Delivers value by improving alert management workflow.

**Acceptance Scenarios**:

1. **Given** I have an active compliance alert, **When** I mark it as resolved with resolution notes, **Then** the alert is marked resolved with the current date and user ID, and no longer appears in active alerts
2. **Given** I view resolved alerts, **When** I look at the details, **Then** I can see who resolved it, when, and the resolution notes

---

### Edge Cases

- What happens when an employee has multiple work authorizations of different types? (System should track all independently)
- How does the system handle work authorizations without expiration dates (e.g., permanent residents)? (ExpirationDate is nullable; no alerts generated for null expirations)
- What happens if authorization expires but employee is on leave? (Alert still generated; compliance team decides action)
- How are duplicate alerts prevented when the daily service runs multiple times? (Service checks for existing open alerts before creating new ones)
- What happens when an employee is terminated but has a valid work authorization? (Authorization remains in system but marked inactive upon processing EmployeeTerminatedEvent)
- How does the system handle authorizations expiring on today's date? (Treated as expired; midnight service will flag them)
- What if an HR admin tries to access work authorization for an employee that doesn't exist? (Return error "EMPLOYEE_NOT_FOUND")
- What happens when Employee Service is unavailable during report generation or list displays? (System displays employee ID with indicator "(name unavailable)" and continues operation with partial data)
- What happens if an expired authorization is renewed during the 30-day grace period? (AccessRevocationRequiredEvent is not published; authorization status updates to Compliant or ExpiringSoon based on new expiration date)
- What happens when two HR administrators try to update the same work authorization simultaneously? (Optimistic locking detects conflict; second save attempt receives "CONCURRENT_MODIFICATION" error and user must refresh and retry)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authorized users to create work authorization records containing employee ID, authorization type, document number, issue date, optional expiration date, issuing authority, sponsorship status, right-to-work document reference, and notes
- **FR-002**: System MUST automatically calculate and set compliance status based on expiration date: Compliant (>90 days), ExpiringSoon (30-90 days), Expired (past expiration), or PendingVerification (upon initial creation)
- **FR-003**: System MUST enforce unique active work authorizations per authorization type per employee (only one active WorkVisa, one active PermanentResident, etc. per employee)
- **FR-004**: System MUST validate that issue date is not after expiration date before saving work authorization records
- **FR-005**: System MUST require right-to-work document reference for all non-citizen authorization types (any type except Citizen)
- **FR-006**: System MUST allow retrieval of all work authorizations for a specific employee
- **FR-007**: System MUST allow retrieval of work authorizations expiring within a specified number of days (with default of 90 days)
- **FR-008**: System MUST allow updating existing work authorization records including all editable fields, record modification timestamp, and use optimistic locking (version or timestamp-based) to detect concurrent modifications
- **FR-009**: System MUST run a daily service at 7 AM to identify authorizations where days until expiration equals 30, 60, or 90 days (calculated as: expiration_date - current_date) and create corresponding alerts; each alert generated once per threshold
- **FR-010**: System MUST run a daily service at midnight to identify expired authorizations, update their compliance status to Expired, and create critical alerts
- **FR-011**: System MUST create compliance alerts with employee ID, work authorization ID, alert type, severity level, descriptive message, and creation timestamp
- **FR-012**: System MUST allow retrieval of active (unresolved) compliance alerts
- **FR-013**: System MUST allow authorized users to resolve compliance alerts by providing resolution notes; resolved alerts are retained indefinitely for audit trail and compliance reporting
- **FR-013a**: System MUST allow retrieval of resolved compliance alerts with filters by date range, employee, alert type, and resolver for audit and historical compliance reporting
- **FR-014**: System MUST generate compliance summary reports showing total employees, authorization counts, compliance rates, breakdown by authorization type, and active alerts
- **FR-015**: System MUST support filtering compliance reports by department ID
- **FR-016**: System MUST publish WorkAuthorizationExpiringEvent when authorization reaches expiration thresholds (30/60/90 days)
- **FR-017**: System MUST publish WorkAuthorizationExpiredEvent when authorization passes expiration date
- **FR-018**: System MUST publish AccessRevocationRequiredEvent when authorization has been expired for 30 days with no renewal, indicating system access should be revoked for the employee
- **FR-019**: System MUST consume EmployeeCreatedEvent to maintain employee context for work authorizations
- **FR-020**: System MUST consume EmployeeTerminatedEvent to mark work authorizations as inactive for terminated employees
- **FR-021**: System MUST consume TrainingCompletedEvent from Career Service when certifications with expiration dates are involved (for potential future certification tracking)
- **FR-022**: System MUST enforce permission "compliance.manage" for creating, updating, and viewing work authorizations and alerts
- **FR-023**: System MUST enforce permission "compliance.reports" for viewing compliance reports
- **FR-024**: System MUST return error code "INVALID_DOCUMENT_NUMBER" when document number format is invalid
- **FR-025**: System MUST return error code "INVALID_DATE_RANGE" when issue date is after expiration date
- **FR-026**: System MUST return error code "DUPLICATE_AUTHORIZATION" when attempting to create a duplicate active authorization
- **FR-027**: System MUST return error code "NOT_AUTHORIZED" when user lacks required permissions
- **FR-028**: System MUST return error code "AUTHORIZATION_NOT_FOUND" when requested work authorization does not exist
- **FR-029**: System MUST return error code "EMPLOYEE_NOT_FOUND" when referenced employee does not exist
- **FR-030**: System MUST track creation and modification timestamps for all work authorization records
- **FR-031**: System MUST gracefully degrade when Employee Service is unavailable or employee lookup fails by displaying employee ID with indicator "(name unavailable)" in reports and lists, allowing operations to complete with partial data
- **FR-032**: System MUST return error code "CONCURRENT_MODIFICATION" when an update attempt fails due to optimistic locking conflict (record was modified by another user since it was retrieved)

### Key Entities

- **WorkAuthorization**: Represents an employee's legal authorization to work, including authorization type (citizen, visa, permit, etc.), document details (number, issuing authority, dates), sponsorship information, compliance status, and references to supporting documents. Maintains full audit trail with creation and modification dates.

- **ComplianceAlert**: Represents a notification about a compliance issue requiring attention, linked to specific work authorization and employee. Captures alert type (expiration warning, expired, missing document), severity level (low to critical), message content, resolution status, and resolution details (who resolved, when, notes).

- **AuthorizationType**: Enumeration defining types of work authorizations (Citizen, PermanentResident, WorkVisa, StudentVisa, TemporaryWorker, Refugee, Asylee, Other) that determine compliance requirements and document needs.

- **SponsorshipStatus**: Enumeration tracking the sponsorship state of work visas (NotRequired, Sponsored, TransferPending, RenewalPending, Expired) which affects renewal processes and responsibilities.

- **ComplianceStatus**: Enumeration indicating current compliance state (Compliant, ExpiringSoon, Expired, PendingVerification, NonCompliant) automatically calculated based on expiration dates and verification status.

- **AlertType**: Enumeration categorizing compliance alerts (ExpirationWarning, Expired, DocumentMissing, VerificationRequired, RenewalReminder) to enable proper routing and handling.

- **AlertSeverity**: Enumeration defining urgency levels (Low, Medium, High, Critical) that determine alert priority and escalation paths.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: HR administrators can record a complete work authorization in under 2 minutes
- **SC-002**: System identifies and alerts on 100% of work authorizations expiring within configured thresholds (30/60/90 days)
- **SC-003**: Compliance reports generate in under 5 seconds for organizations with up to 10,000 employees
- **SC-004**: Zero work authorizations expire without advance notification (at least 90-day warning)
- **SC-005**: HR administrators can view and act on expiring authorizations within 3 clicks from the main compliance dashboard
- **SC-006**: System maintains 99.9% accuracy in compliance status calculations compared to manual review
- **SC-007**: Reduce time spent on manual compliance tracking by 70% compared to spreadsheet-based processes
- **SC-008**: 95% of compliance alerts result in timely action (authorization renewed or employee offboarded) before expiration
- **SC-009**: Compliance report generation supports real-time reporting without impacting work authorization record creation performance
- **SC-010**: System prevents data loss from concurrent updates using optimistic locking; when conflicts occur, user receives clear error message and can retry update with current data
- **SC-011**: Integration events (published and consumed) process within 30 seconds of triggering action
- **SC-012**: Support tracking work authorizations for organizations with up to 50,000 employees without performance degradation

## Assumptions

- **A-001**: Employee Service is already operational and can provide EmployeeCreatedEvent and EmployeeTerminatedEvent
- **A-002**: Upload Service exists and can store right-to-work documents, returning document IDs for reference
- **A-003**: Event bus infrastructure is in place for publishing and consuming integration events
- **A-004**: Authorization and permission system is available to enforce "compliance.manage" and "compliance.reports" permissions
- **A-005**: Document number format validation will be specific to issuing authority standards (e.g., USCIS format for US work authorizations)
- **A-006**: Grace period for access revocation is 30 days after expiration date; AccessRevocationRequiredEvent is published only after authorization has been expired for 30 consecutive days without renewal
- **A-007**: Background services (expiration reminder and flagging) have reliable scheduling infrastructure available
- **A-008**: HR administrators have unique user IDs available for audit tracking (ResolvedBy field)
- **A-009**: Department information is available from Employee Service for department-filtered compliance reports
- **A-010**: Employee names are retrieved from Employee Service for display in reports and expiring authorization lists
- **A-011**: Time zone handling: All dates use UTC for storage and calculation; UI conversion to user time zones handled by presentation layer
- **A-012**: Notification delivery (email/SMS to HR and employees) is handled by a separate Notification Service consuming the expiration events
- **A-013**: Career Service integration for certification tracking is included for future extensibility but not required for initial release

## Dependencies

- **D-001**: Employee Service must provide employee lookup capability to validate employee IDs and retrieve employee names for reports
- **D-002**: Upload Service must be available to store and retrieve right-to-work documents
- **D-003**: Event Bus infrastructure for asynchronous event publishing and consumption
- **D-004**: Authorization/Permission service for enforcing compliance.manage and compliance.reports permissions
- **D-005**: Scheduling infrastructure for cron-based background services (7 AM daily, midnight daily)
- **D-006**: Notification Service (external) to consume WorkAuthorizationExpiringEvent and WorkAuthorizationExpiredEvent for user notifications

## Scope

### In Scope

- Recording and updating work authorization records
- Automated expiration monitoring with configurable thresholds (30/60/90 days)
- Compliance alert generation and resolution
- Compliance status reporting (organization-wide and department-level)
- Integration events for expiration notifications and employee lifecycle changes
- Permission-based access control
- Support for multiple authorization types and sponsorship statuses
- Background services for automated compliance monitoring

### Out of Scope

- Document upload functionality (delegated to Upload Service)
- Actual notification delivery to users (delegated to Notification Service)
- Employee management (delegated to Employee Service)
- Certification and training compliance tracking beyond initial event consumption structure
- Workflow automation for renewal processes (future enhancement)
- Integration with external government systems for document verification
- Multi-language support for compliance alerts and reports
- Custom reporting builder (only predefined compliance reports included)
- Historical compliance trend analysis and analytics
- Mobile application for compliance management
