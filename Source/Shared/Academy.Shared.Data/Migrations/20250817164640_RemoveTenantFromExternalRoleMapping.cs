using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTenantFromExternalRoleMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalRoleMappings_Tenants_TenantId",
                table: "ExternalRoleMappings");

            migrationBuilder.DropIndex(
                name: "IX_ExternalRoleMappings_TenantId",
                table: "ExternalRoleMappings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExternalRoleMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "ExternalRoleMappings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRoleMappings_TenantId",
                table: "ExternalRoleMappings",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalRoleMappings_Tenants_TenantId",
                table: "ExternalRoleMappings",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
