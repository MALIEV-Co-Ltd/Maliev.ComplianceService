using Maliev.ComplianceService.Application.DTOs;
using Maliev.ComplianceService.Application.Interfaces;
using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Domain.Enums;
using Maliev.ComplianceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.ComplianceService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WorkAuthorization entity
/// </summary>
public class WorkAuthorizationRepository : IWorkAuthorizationRepository
{
    private readonly ComplianceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkAuthorizationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public WorkAuthorizationRepository(ComplianceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<WorkAuthorization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkAuthorizations
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkAuthorization>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkAuthorizations
            .AsNoTracking()
            .Where(w => w.EmployeeId == employeeId && w.IsActive)
            .OrderByDescending(w => w.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkAuthorization>> GetExpiringWithinDaysAsync(int days, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var futureDate = today.AddDays(days);

        return await _context.WorkAuthorizations
            .AsNoTracking()
            .Where(w => w.IsActive
                && w.ExpirationDate != null
                && w.ExpirationDate >= today
                && w.ExpirationDate <= futureDate)
            .OrderBy(w => w.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkAuthorization>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.WorkAuthorizations
            .AsNoTracking()
            .Where(w => w.IsActive
                && w.ExpirationDate != null
                && w.ExpirationDate < today)
            .OrderBy(w => w.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkAuthorization>> GetByComplianceStatusAsync(ComplianceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.WorkAuthorizations
            .AsNoTracking()
            .Where(w => w.IsActive && w.ComplianceStatus == status)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> HasActiveAuthorizationAsync(Guid employeeId, AuthorizationType type, CancellationToken cancellationToken = default)
    {
        return await _context.WorkAuthorizations
            .AnyAsync(w => w.EmployeeId == employeeId
                && w.AuthorizationType == type
                && w.IsActive, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<WorkAuthorization> AddAsync(WorkAuthorization authorization, CancellationToken cancellationToken = default)
    {
        _context.WorkAuthorizations.Add(authorization);
        await _context.SaveChangesAsync(cancellationToken);
        return authorization;
    }

    /// <inheritdoc/>
    public async Task<WorkAuthorization> UpdateAsync(WorkAuthorization authorization, CancellationToken cancellationToken = default)
    {
        authorization.ModifiedDate = DateTime.UtcNow;
        _context.WorkAuthorizations.Update(authorization);
        await _context.SaveChangesAsync(cancellationToken);
        return authorization;
    }

    /// <inheritdoc/>
    public async Task DeactivateWorkAuthorizationsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var authorizations = await _context.WorkAuthorizations
            .Where(w => w.EmployeeId == employeeId && w.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var auth in authorizations)
        {
            auth.IsActive = false;
            auth.ModifiedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IDictionary<ComplianceStatus, int>> GetComplianceStatsAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        // Note: Department filtering would normally require a join or a list of employee IDs.
        // For this implementation, we assume department filtering is handled by the caller 
        // providing a list of employee IDs if needed, or we just count all if departmentId is null.

        return await _context.WorkAuthorizations
            .Where(w => w.IsActive)
            .GroupBy(w => w.ComplianceStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AuthorizationTypeBreakdown>> GetTypeBreakdownAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        return await _context.WorkAuthorizations
            .Where(w => w.IsActive)
            .GroupBy(w => w.AuthorizationType)
            .Select(g => new AuthorizationTypeBreakdown
            {
                Type = g.Key,
                Count = g.Count(),
                ExpiringSoon = g.Count(x => x.ComplianceStatus == ComplianceStatus.ExpiringSoon),
                Expired = g.Count(x => x.ComplianceStatus == ComplianceStatus.Expired)
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
