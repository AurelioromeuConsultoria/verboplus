using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToSchedulingArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnviosCampanhaAniversario_PessoaId_AnoReferencia",
                table: "EnviosCampanhaAniversario");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MensagensAgendadas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EnviosCampanhaAniversario",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE \"MensagensAgendadas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"EnviosCampanhaAniversario\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_TenantId_Status_DataEnvio",
                table: "MensagensAgendadas",
                columns: new[] { "TenantId", "Status", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_TenantId_VisitanteId_DataEnvio",
                table: "MensagensAgendadas",
                columns: new[] { "TenantId", "VisitanteId", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCampanhaAniversario_TenantId_PessoaId_AnoReferencia",
                table: "EnviosCampanhaAniversario",
                columns: new[] { "TenantId", "PessoaId", "AnoReferencia" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EnviosCampanhaAniversario_Tenants_TenantId",
                table: "EnviosCampanhaAniversario",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensAgendadas_Tenants_TenantId",
                table: "MensagensAgendadas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnviosCampanhaAniversario_Tenants_TenantId",
                table: "EnviosCampanhaAniversario");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensAgendadas_Tenants_TenantId",
                table: "MensagensAgendadas");

            migrationBuilder.DropIndex(
                name: "IX_MensagensAgendadas_TenantId_Status_DataEnvio",
                table: "MensagensAgendadas");

            migrationBuilder.DropIndex(
                name: "IX_MensagensAgendadas_TenantId_VisitanteId_DataEnvio",
                table: "MensagensAgendadas");

            migrationBuilder.DropIndex(
                name: "IX_EnviosCampanhaAniversario_TenantId_PessoaId_AnoReferencia",
                table: "EnviosCampanhaAniversario");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MensagensAgendadas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EnviosCampanhaAniversario");

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCampanhaAniversario_PessoaId_AnoReferencia",
                table: "EnviosCampanhaAniversario",
                columns: new[] { "PessoaId", "AnoReferencia" },
                unique: true);
        }
    }
}
