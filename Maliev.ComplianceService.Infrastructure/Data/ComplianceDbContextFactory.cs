using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.ComplianceService.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating ComplianceDbContext for EF Core migrations
/// </summary>
public class ComplianceDbContextFactory : IDesignTimeDbContextFactory<ComplianceDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="ComplianceDbContext"/> for design-time use.
    /// </summary>
    /// <param name="args">Arguments passed by the design-time tool.</param>
    /// <returns>A new instance of the database context.</returns>
    public ComplianceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ComplianceDbContext>();

        // Use a dummy connection string for design-time operations
        // The actual connection string will be configured in Program.cs
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=compliance;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")
        );

        return new ComplianceDbContext(optionsBuilder.Options);
    }
}
