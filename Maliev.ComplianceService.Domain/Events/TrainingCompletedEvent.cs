namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when an employee completes a training course or certification.
/// </summary>
public record TrainingCompletedEvent(
    Guid EmployeeId, 
    string CourseName, 
    DateTime CompletionDate, 
    DateTime? CertificationExpiration, 
    Guid? CertificateId, 
    DateTime Timestamp, 
    string Version = "1.0");
