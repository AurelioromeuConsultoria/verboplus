using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToKidsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResponsaveisCriancas_CriancaPessoaId_ResponsavelPessoaId",
                table: "ResponsaveisCriancas");

            migrationBuilder.DropIndex(
                name: "IX_KidsTurmas_SalaId_Nome",
                table: "KidsTurmas");

            migrationBuilder.DropIndex(
                name: "IX_KidsSalas_Nome",
                table: "KidsSalas");

            migrationBuilder.DropIndex(
                name: "IX_KidsOcorrencias_CriancaPessoaId_DataCriacao",
                table: "KidsOcorrencias");

            migrationBuilder.DropIndex(
                name: "IX_KidsOcorrencias_Status_DataCriacao",
                table: "KidsOcorrencias");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_CriancaPessoaId_Status",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_Status",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsDeviceTokens_PessoaId_FcmToken",
                table: "KidsDeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_CodigoSessao",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_CriancaPessoaId_Status",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TokenRetirada",
                table: "KidsCheckins");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ResponsaveisCriancas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsTurmas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsSalas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsOcorrencias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsNotificacoes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsDeviceTokens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KidsCheckins",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CriancasDetalhes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE \"CriancasDetalhes\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsSalas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsTurmas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"ResponsaveisCriancas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsCheckins\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsNotificacoes\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsOcorrencias\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"KidsDeviceTokens\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsaveisCriancas_TenantId_CriancaPessoaId_ResponsavelPe~",
                table: "ResponsaveisCriancas",
                columns: new[] { "TenantId", "CriancaPessoaId", "ResponsavelPessoaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsTurmas_TenantId_SalaId_Nome",
                table: "KidsTurmas",
                columns: new[] { "TenantId", "SalaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsSalas_TenantId_Nome",
                table: "KidsSalas",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_TenantId_CriancaPessoaId_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "TenantId", "CriancaPessoaId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_TenantId_Status_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "TenantId", "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_TenantId_CriancaPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "TenantId", "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_TenantId_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes",
                columns: new[] { "TenantId", "ResponsavelPessoaId", "LidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_TenantId_ResponsavelPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "TenantId", "ResponsavelPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsDeviceTokens_TenantId_PessoaId_FcmToken",
                table: "KidsDeviceTokens",
                columns: new[] { "TenantId", "PessoaId", "FcmToken" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TenantId_CodigoSessao",
                table: "KidsCheckins",
                columns: new[] { "TenantId", "CodigoSessao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TenantId_CriancaPessoaId_Status",
                table: "KidsCheckins",
                columns: new[] { "TenantId", "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TenantId_PinRetirada",
                table: "KidsCheckins",
                columns: new[] { "TenantId", "PinRetirada" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TenantId_TokenRetirada",
                table: "KidsCheckins",
                columns: new[] { "TenantId", "TokenRetirada" });

            migrationBuilder.CreateIndex(
                name: "IX_CriancasDetalhes_TenantId_PessoaId",
                table: "CriancasDetalhes",
                columns: new[] { "TenantId", "PessoaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CriancasDetalhes_Tenants_TenantId",
                table: "CriancasDetalhes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsCheckins_Tenants_TenantId",
                table: "KidsCheckins",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsDeviceTokens_Tenants_TenantId",
                table: "KidsDeviceTokens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsNotificacoes_Tenants_TenantId",
                table: "KidsNotificacoes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsOcorrencias_Tenants_TenantId",
                table: "KidsOcorrencias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsSalas_Tenants_TenantId",
                table: "KidsSalas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KidsTurmas_Tenants_TenantId",
                table: "KidsTurmas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResponsaveisCriancas_Tenants_TenantId",
                table: "ResponsaveisCriancas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriancasDetalhes_Tenants_TenantId",
                table: "CriancasDetalhes");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsCheckins_Tenants_TenantId",
                table: "KidsCheckins");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsDeviceTokens_Tenants_TenantId",
                table: "KidsDeviceTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsNotificacoes_Tenants_TenantId",
                table: "KidsNotificacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsOcorrencias_Tenants_TenantId",
                table: "KidsOcorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsSalas_Tenants_TenantId",
                table: "KidsSalas");

            migrationBuilder.DropForeignKey(
                name: "FK_KidsTurmas_Tenants_TenantId",
                table: "KidsTurmas");

            migrationBuilder.DropForeignKey(
                name: "FK_ResponsaveisCriancas_Tenants_TenantId",
                table: "ResponsaveisCriancas");

            migrationBuilder.DropIndex(
                name: "IX_ResponsaveisCriancas_TenantId_CriancaPessoaId_ResponsavelPe~",
                table: "ResponsaveisCriancas");

            migrationBuilder.DropIndex(
                name: "IX_KidsTurmas_TenantId_SalaId_Nome",
                table: "KidsTurmas");

            migrationBuilder.DropIndex(
                name: "IX_KidsSalas_TenantId_Nome",
                table: "KidsSalas");

            migrationBuilder.DropIndex(
                name: "IX_KidsOcorrencias_TenantId_CriancaPessoaId_DataCriacao",
                table: "KidsOcorrencias");

            migrationBuilder.DropIndex(
                name: "IX_KidsOcorrencias_TenantId_Status_DataCriacao",
                table: "KidsOcorrencias");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_TenantId_CriancaPessoaId_Status",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_TenantId_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_TenantId_ResponsavelPessoaId_Status",
                table: "KidsNotificacoes");

            migrationBuilder.DropIndex(
                name: "IX_KidsDeviceTokens_TenantId_PessoaId_FcmToken",
                table: "KidsDeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TenantId_CodigoSessao",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TenantId_CriancaPessoaId_Status",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TenantId_PinRetirada",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TenantId_TokenRetirada",
                table: "KidsCheckins");

            migrationBuilder.DropIndex(
                name: "IX_CriancasDetalhes_TenantId_PessoaId",
                table: "CriancasDetalhes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ResponsaveisCriancas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsTurmas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsSalas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsOcorrencias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsNotificacoes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsDeviceTokens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CriancasDetalhes");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsaveisCriancas_CriancaPessoaId_ResponsavelPessoaId",
                table: "ResponsaveisCriancas",
                columns: new[] { "CriancaPessoaId", "ResponsavelPessoaId" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsTurmas_SalaId_Nome",
                table: "KidsTurmas",
                columns: new[] { "SalaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsSalas_Nome",
                table: "KidsSalas",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_CriancaPessoaId_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "CriancaPessoaId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_Status_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_CriancaPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes",
                columns: new[] { "ResponsavelPessoaId", "LidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "ResponsavelPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsDeviceTokens_PessoaId_FcmToken",
                table: "KidsDeviceTokens",
                columns: new[] { "PessoaId", "FcmToken" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_CodigoSessao",
                table: "KidsCheckins",
                column: "CodigoSessao");

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_CriancaPessoaId_Status",
                table: "KidsCheckins",
                columns: new[] { "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TokenRetirada",
                table: "KidsCheckins",
                column: "TokenRetirada");
        }
    }
}
