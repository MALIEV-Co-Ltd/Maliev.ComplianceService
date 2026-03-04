using Maliev.ComplianceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.ComplianceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for WorkAuthorization entity
/// </summary>
public class WorkAuthorizationConfiguration : IEntityTypeConfiguration<WorkAuthorization>
{
    /// <summary>
    /// Configures the work authorization entity.
    /// </summary>
    /// <param name="builder">The entity builder.</param>
    public void Configure(EntityTypeBuilder<WorkAuthorization> builder)
    {
        // Table name
        builder.ToTable("work_authorizations");

        // Primary key
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties
        builder.Property(w => w.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(w => w.AuthorizationType)
            .HasColumnName("authorization_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.IssueDate)
            .HasColumnName("issue_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(w => w.ExpirationDate)
            .HasColumnName("expiration_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(w => w.IssuingAuthority)
            .HasColumnName("issuing_authority")
            .HasMaxLength(200);

        builder.Property(w => w.SponsorshipStatus)
            .HasColumnName("sponsorship_status")
            .HasConversion<int?>();

        builder.Property(w => w.RightToWorkDocumentId)
            .HasColumnName("right_to_work_document_id");

        builder.Property(w => w.Notes)
            .HasColumnName("notes");

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(w => w.ComplianceStatus)
            .HasColumnName("compliance_status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.LastExpirationAlertThreshold)
            .HasColumnName("last_expiration_alert_threshold");

        builder.Property(w => w.AccessRevocationSentDate)
            .HasColumnName("access_revocation_sent_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(w => w.CreatedDate)
            .HasColumnName("created_date")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(w => w.ModifiedDate)
            .HasColumnName("modified_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(w => w.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(w => w.EmployeeId)
            .HasDatabaseName("idx_work_auth_employee");

        builder.HasIndex(w => w.ExpirationDate)
            .HasDatabaseName("idx_work_auth_expiration");

        builder.HasIndex(w => w.ComplianceStatus)
            .HasDatabaseName("idx_work_auth_status");

        builder.HasIndex(w => w.IsActive)
            .HasDatabaseName("idx_work_auth_active")
            .HasFilter("is_active = TRUE");

        // Unique constraint: only one active authorization per type per employee
        builder.HasIndex(w => new { w.EmployeeId, w.AuthorizationType })
            .HasDatabaseName("uq_active_auth_per_type")
            .IsUnique()
            .HasFilter("is_active = TRUE");

        // Relationships
        builder.HasMany(w => w.ComplianceAlerts)
            .WithOne(a => a.WorkAuthorization)
            .HasForeignKey(a => a.WorkAuthorizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
