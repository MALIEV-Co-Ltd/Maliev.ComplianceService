using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.Interfaces;

/// <summary>
/// Repository interface for WorkAuthorization entity operations
/// </summary>
public interface IWorkAuthorizationRepository
{
    /// <summary>
    /// Gets a work authorization by ID
    /// </summary>
    Task<WorkAuthorization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all work authorizations for a specific employee
    /// </summary>
    Task<IEnumerable<WorkAuthorization>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets work authorizations expiring within specified days
    /// </summary>
    Task<IEnumerable<WorkAuthorization>> GetExpiringWithinDaysAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired work authorizations
    /// </summary>
    Task<IEnumerable<WorkAuthorization>> GetExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets work authorizations by compliance status
    /// </summary>
    Task<IEnumerable<WorkAuthorization>> GetByComplianceStatusAsync(ComplianceStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an active authorization exists for employee and type
    /// </summary>
    Task<bool> HasActiveAuthorizationAsync(Guid employeeId, AuthorizationType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new work authorization
    /// </summary>
    Task<WorkAuthorization> AddAsync(WorkAuthorization authorization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing work authorization
    /// </summary>
    Task<WorkAuthorization> UpdateAsync(WorkAuthorization authorization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates all work authorizations for an employee (soft delete)
    /// </summary>
    Task DeactivateWorkAuthorizationsAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
