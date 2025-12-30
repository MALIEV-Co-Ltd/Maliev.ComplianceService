using Microsoft.EntityFrameworkCore;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Infrastructure.Data;

namespace Maliev.ComplianceService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ComplianceAlert entity
/// </summary>
public class ComplianceAlertRepository : IComplianceAlertRepository
{
    private readonly ComplianceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplianceAlertRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ComplianceAlertRepository(ComplianceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<ComplianceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ComplianceAlerts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ComplianceAlert>> GetAlertsAsync(
        bool? isResolved = null,
        AlertSeverity? severity = null,
        Guid? employeeId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        AlertType? alertType = null,
        Guid? resolvedBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ComplianceAlerts.AsNoTracking();

        if (isResolved.HasValue)
            query = query.Where(a => a.IsResolved == isResolved.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedDate <= toDate.Value);

        if (alertType.HasValue)
            query = query.Where(a => a.AlertType == alertType.Value);

        if (resolvedBy.HasValue)
            query = query.Where(a => a.ResolvedBy == resolvedBy.Value);

        return await query
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ComplianceAlert>> GetUnresolvedByAuthorizationIdAsync(Guid authorizationId, CancellationToken cancellationToken = default)
    {
        return await _context.ComplianceAlerts
            .AsNoTracking()
            .Where(a => a.WorkAuthorizationId == authorizationId && !a.IsResolved)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ComplianceAlert>> GetUnresolvedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ComplianceAlerts
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && !a.IsResolved)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> HasUnresolvedAlertAsync(Guid authorizationId, AlertType alertType, CancellationToken cancellationToken = default)
    {
        return await _context.ComplianceAlerts
            .AnyAsync(a => a.WorkAuthorizationId == authorizationId
                && a.AlertType == alertType
                && !a.IsResolved, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ComplianceAlert> CreateAsync(ComplianceAlert alert, CancellationToken cancellationToken = default)
    {
        _context.ComplianceAlerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return alert;
    }

    /// <inheritdoc/>
    public async Task<ComplianceAlert> UpdateAsync(ComplianceAlert alert, CancellationToken cancellationToken = default)
    {
        _context.ComplianceAlerts.Update(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return alert;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
