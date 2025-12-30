namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when a new employee is created.
/// </summary>
public record EmployeeCreatedEvent(
    Guid EmployeeId, 
    string EmployeeNumber, 
    string FirstName, 
    string LastName, 
    string Email, 
    Guid DepartmentId, 
    DateTime StartDate, 
    DateTime Timestamp, 
    string Version = "1.0");
