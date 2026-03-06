using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.ComplianceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddXminConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "work_authorizations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "work_authorizations");
        }
    }
}
