using Maliev.ComplianceService.Domain.Entities;
using Maliev.ComplianceService.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Maliev.ComplianceService.Infrastructure.Data;

/// <summary>
/// Database context for the Compliance Service
/// </summary>
public class ComplianceDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComplianceDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the work authorization records.
    /// </summary>
    public DbSet<WorkAuthorization> WorkAuthorizations => Set<WorkAuthorization>();

    /// <summary>
    /// Gets or sets the compliance alert records.
    /// </summary>
    public DbSet<ComplianceAlert> ComplianceAlerts => Set<ComplianceAlert>();

    /// <summary>
    /// Configures the database model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new WorkAuthorizationConfiguration());
        modelBuilder.ApplyConfiguration(new ComplianceAlertConfiguration());
    }
}
