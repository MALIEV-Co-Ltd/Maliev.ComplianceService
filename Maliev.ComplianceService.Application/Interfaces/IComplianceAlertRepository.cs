using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;

namespace Maliev.ComplianceService.Application.Interfaces;

/// <summary>
/// Repository interface for ComplianceAlert entity operations
/// </summary>
public interface IComplianceAlertRepository
{
    /// <summary>
    /// Gets a compliance alert by ID
    /// </summary>
    Task<ComplianceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all compliance alerts (with optional filters)
    /// </summary>
    Task<IEnumerable<ComplianceAlert>> GetAlertsAsync(
        bool? isResolved = null,
        AlertSeverity? severity = null,
        Guid? employeeId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        AlertType? alertType = null,
        Guid? resolvedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unresolved alerts for a specific work authorization
    /// </summary>
    Task<IEnumerable<ComplianceAlert>> GetUnresolvedByAuthorizationIdAsync(Guid authorizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unresolved alerts for a specific employee
    /// </summary>
    Task<IEnumerable<ComplianceAlert>> GetUnresolvedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an unresolved alert of a specific type exists for an authorization
    /// </summary>
    Task<bool> HasUnresolvedAlertAsync(Guid authorizationId, AlertType alertType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new compliance alert
    /// </summary>
    Task<ComplianceAlert> CreateAsync(ComplianceAlert alert, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing compliance alert
    /// </summary>
    Task<ComplianceAlert> UpdateAsync(ComplianceAlert alert, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
