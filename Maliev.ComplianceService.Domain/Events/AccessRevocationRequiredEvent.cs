namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when system access revocation is required due to expired work authorization.
/// </summary>
public record AccessRevocationRequiredEvent(
    Guid EmployeeId,
    DateTime EffectiveDate,
    string Reason,
    Guid AuthorizationId,
    int ExpiredSince,
    DateTime Timestamp,
    string Version = "1.0");
