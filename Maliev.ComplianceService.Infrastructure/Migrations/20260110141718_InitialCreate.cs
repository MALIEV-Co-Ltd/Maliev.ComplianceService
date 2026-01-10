using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.ComplianceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "work_authorizations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                authorization_type = table.Column<int>(type: "integer", nullable: false),
                document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                issuing_authority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                sponsorship_status = table.Column<int>(type: "integer", nullable: true),
                right_to_work_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                notes = table.Column<string>(type: "text", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                last_expiration_alert_threshold = table.Column<int>(type: "integer", nullable: true),
                access_revocation_sent_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                compliance_status = table.Column<int>(type: "integer", nullable: false),
                created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                row_version = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_work_authorizations", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "compliance_alerts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                work_authorization_id = table.Column<Guid>(type: "uuid", nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                alert_type = table.Column<int>(type: "integer", nullable: false),
                severity = table.Column<int>(type: "integer", nullable: false),
                message = table.Column<string>(type: "text", nullable: false),
                is_resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                resolved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                resolution_notes = table.Column<string>(type: "text", nullable: true),
                created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_compliance_alerts", x => x.id);
                table.ForeignKey(
                    name: "FK_compliance_alerts_work_authorizations_work_authorization_id",
                    column: x => x.work_authorization_id,
                    principalTable: "work_authorizations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "idx_alerts_auth",
            table: "compliance_alerts",
            column: "work_authorization_id");

        migrationBuilder.CreateIndex(
            name: "idx_alerts_employee",
            table: "compliance_alerts",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "idx_alerts_unresolved",
            table: "compliance_alerts",
            column: "is_resolved",
            filter: "is_resolved = FALSE");

        migrationBuilder.CreateIndex(
            name: "idx_work_auth_active",
            table: "work_authorizations",
            column: "is_active",
            filter: "is_active = TRUE");

        migrationBuilder.CreateIndex(
            name: "idx_work_auth_employee",
            table: "work_authorizations",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "idx_work_auth_expiration",
            table: "work_authorizations",
            column: "expiration_date");

        migrationBuilder.CreateIndex(
            name: "idx_work_auth_status",
            table: "work_authorizations",
            column: "compliance_status");

        migrationBuilder.CreateIndex(
            name: "uq_active_auth_per_type",
            table: "work_authorizations",
            columns: new[] { "employee_id", "authorization_type" },
            unique: true,
            filter: "is_active = TRUE");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "compliance_alerts");

        migrationBuilder.DropTable(
            name: "work_authorizations");
    }
}
