using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdminAndTenantBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformAdmin",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CorPrimaria",
                table: "Tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorSecundaria",
                table: "Tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "Tenants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRootTenant",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NomeExibicao",
                table: "Tenants",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CorPrimaria", "CorSecundaria", "FaviconUrl", "IsRootTenant", "NomeExibicao" },
                values: new object[] { "#111827", "#374151", null, true, "Mang Guarulhos" });

            migrationBuilder.Sql("""
                UPDATE "Usuarios"
                SET "IsPlatformAdmin" = TRUE
                WHERE "TenantId" = 1
                  AND "TipoUsuario" IN (1, 3);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPlatformAdmin",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CorPrimaria",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CorSecundaria",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsRootTenant",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "NomeExibicao",
                table: "Tenants");
        }
    }
}
