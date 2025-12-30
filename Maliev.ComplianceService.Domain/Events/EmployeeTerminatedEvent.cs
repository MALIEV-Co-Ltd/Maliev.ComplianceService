namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when an employee is terminated.
/// </summary>
public record EmployeeTerminatedEvent(
    Guid EmployeeId, 
    DateTime TerminationDate, 
    string Reason, 
    DateTime Timestamp, 
    string Version = "1.0");
