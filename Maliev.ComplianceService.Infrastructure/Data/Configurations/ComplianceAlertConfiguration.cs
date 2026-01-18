using Maliev.ComplianceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.ComplianceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ComplianceAlert entity
/// </summary>
public class ComplianceAlertConfiguration : IEntityTypeConfiguration<ComplianceAlert>
{
    /// <summary>
    /// Configures the compliance alert entity.
    /// </summary>
    /// <param name="builder">The entity builder.</param>
    public void Configure(EntityTypeBuilder<ComplianceAlert> builder)
    {
        // Table name
        builder.ToTable("compliance_alerts");

        // Primary key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties
        builder.Property(a => a.WorkAuthorizationId)
            .HasColumnName("work_authorization_id")
            .IsRequired();

        builder.Property(a => a.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(a => a.AlertType)
            .HasColumnName("alert_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.Severity)
            .HasColumnName("severity")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(a => a.IsResolved)
            .HasColumnName("is_resolved")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.ResolvedDate)
            .HasColumnName("resolved_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.ResolvedBy)
            .HasColumnName("resolved_by");

        builder.Property(a => a.ResolutionNotes)
            .HasColumnName("resolution_notes");

        builder.Property(a => a.CreatedDate)
            .HasColumnName("created_date")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.WorkAuthorizationId)
            .HasDatabaseName("idx_alerts_auth");

        builder.HasIndex(a => a.EmployeeId)
            .HasDatabaseName("idx_alerts_employee");

        builder.HasIndex(a => a.IsResolved)
            .HasDatabaseName("idx_alerts_unresolved")
            .HasFilter("is_resolved = FALSE");

        // Foreign key relationship configured in WorkAuthorizationConfiguration
    }
}
