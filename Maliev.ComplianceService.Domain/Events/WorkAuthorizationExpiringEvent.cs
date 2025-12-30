using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Domain.Events;

/// <summary>
/// Integration event published when a work authorization is approaching its expiration date.
/// </summary>
/// <param name="AuthorizationId">The unique identifier of the work authorization.</param>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="AuthorizationType">The type of authorization.</param>
/// <param name="ExpirationDate">The date when the authorization expires.</param>
/// <param name="DaysUntilExpiration">The number of days remaining until expiration.</param>
/// <param name="Timestamp">The timestamp when the event occurred.</param>
/// <param name="Version">The version of the event schema.</param>
public record WorkAuthorizationExpiringEvent(
    Guid AuthorizationId, 
    Guid EmployeeId, 
    AuthorizationType AuthorizationType, 
    DateTime ExpirationDate, 
    int DaysUntilExpiration, 
    DateTime Timestamp, 
    string Version = "1.0");
