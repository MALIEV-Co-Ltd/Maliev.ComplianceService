using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when a work authorization has passed its expiration date.
/// </summary>
/// <param name="AuthorizationId">The unique identifier of the work authorization.</param>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="AuthorizationType">The type of authorization.</param>
/// <param name="ExpirationDate">The date when the authorization expired.</param>
/// <param name="ExpiredDays">The number of days since expiration occurred.</param>
/// <param name="Timestamp">The timestamp when the event occurred.</param>
/// <param name="Version">The version of the event schema.</param>
public record WorkAuthorizationExpiredEvent(
    Guid AuthorizationId,
    Guid EmployeeId,
    string AuthorizationType,
    DateTime ExpirationDate,
    int ExpiredDays,
    DateTime Timestamp,
    string Version = "1.0");
