using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToCoreOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Voluntarios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "HubCasas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Equipes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Cargos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE \"Cargos\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"Equipes\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"HubCasas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"Voluntarios\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_TenantId_PessoaId_EquipeId_CargoId",
                table: "Voluntarios",
                columns: new[] { "TenantId", "PessoaId", "EquipeId", "CargoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HubCasas_TenantId_Nome",
                table: "HubCasas",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipes_TenantId_Nome",
                table: "Equipes",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_TenantId_Nome",
                table: "Cargos",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cargos_Tenants_TenantId",
                table: "Cargos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipes_Tenants_TenantId",
                table: "Equipes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HubCasas_Tenants_TenantId",
                table: "HubCasas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voluntarios_Tenants_TenantId",
                table: "Voluntarios",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cargos_Tenants_TenantId",
                table: "Cargos");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipes_Tenants_TenantId",
                table: "Equipes");

            migrationBuilder.DropForeignKey(
                name: "FK_HubCasas_Tenants_TenantId",
                table: "HubCasas");

            migrationBuilder.DropForeignKey(
                name: "FK_Voluntarios_Tenants_TenantId",
                table: "Voluntarios");

            migrationBuilder.DropIndex(
                name: "IX_Voluntarios_TenantId_PessoaId_EquipeId_CargoId",
                table: "Voluntarios");

            migrationBuilder.DropIndex(
                name: "IX_HubCasas_TenantId_Nome",
                table: "HubCasas");

            migrationBuilder.DropIndex(
                name: "IX_Equipes_TenantId_Nome",
                table: "Equipes");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_TenantId_Nome",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Voluntarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HubCasas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Equipes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Cargos");
        }
    }
}
