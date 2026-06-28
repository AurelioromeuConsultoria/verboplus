using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToSchedulingMediaAndPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolicitacoesTrocasEscalas_EscalaItemId_Status",
                table: "SolicitacoesTrocasEscalas");

            migrationBuilder.DropIndex(
                name: "IX_IndisponibilidadesVoluntarios_VoluntarioId_Data",
                table: "IndisponibilidadesVoluntarios");

            migrationBuilder.DropIndex(
                name: "IX_EscalasModelos_EventoId_EquipeId",
                table: "EscalasModelos");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_EscalaId_EquipeId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_EscalaId_VoluntarioId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_EventoOcorrenciaId_EquipeId",
                table: "Escalas");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SolicitacoesTrocasEscalas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "IndisponibilidadesVoluntarios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GaleriasFotosItens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GaleriasFotos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EscalasModelosItens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EscalasModelos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EscalasItens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Escalas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EnqueteVotos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Enquetes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EnqueteOpcoes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CategoriasMidias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"UPDATE ""Escalas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EscalasItens"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EscalasModelos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EscalasModelosItens"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""IndisponibilidadesVoluntarios"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""SolicitacoesTrocasEscalas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""CategoriasMidias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""GaleriasFotos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""GaleriasFotosItens"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Enquetes"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EnqueteOpcoes"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EnqueteVotos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_TenantId_EscalaItemId_Status",
                table: "SolicitacoesTrocasEscalas",
                columns: new[] { "TenantId", "EscalaItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IndisponibilidadesVoluntarios_TenantId_VoluntarioId_Data",
                table: "IndisponibilidadesVoluntarios",
                columns: new[] { "TenantId", "VoluntarioId", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GaleriasFotosItens_TenantId_GaleriaFotoId_NomeArquivo",
                table: "GaleriasFotosItens",
                columns: new[] { "TenantId", "GaleriaFotoId", "NomeArquivo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GaleriasFotos_TenantId_Nome_Data",
                table: "GaleriasFotos",
                columns: new[] { "TenantId", "Nome", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelosItens_TenantId_EscalaModeloId_Ordem",
                table: "EscalasModelosItens",
                columns: new[] { "TenantId", "EscalaModeloId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelos_TenantId_EventoId_EquipeId",
                table: "EscalasModelos",
                columns: new[] { "TenantId", "EventoId", "EquipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_EquipeId",
                table: "EscalasItens",
                columns: new[] { "TenantId", "EscalaId", "EquipeId" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_VoluntarioId",
                table: "EscalasItens",
                columns: new[] { "TenantId", "EscalaId", "VoluntarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_TenantId_EventoOcorrenciaId_EquipeId",
                table: "Escalas",
                columns: new[] { "TenantId", "EventoOcorrenciaId", "EquipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteVotos_TenantId_EnqueteId_UsuarioId",
                table: "EnqueteVotos",
                columns: new[] { "TenantId", "EnqueteId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enquetes_TenantId_DataCriacao",
                table: "Enquetes",
                columns: new[] { "TenantId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteOpcoes_TenantId_EnqueteId_Ordem",
                table: "EnqueteOpcoes",
                columns: new[] { "TenantId", "EnqueteId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasMidias_TenantId_Nome",
                table: "CategoriasMidias",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasMidias_Tenants_TenantId",
                table: "CategoriasMidias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnqueteOpcoes_Tenants_TenantId",
                table: "EnqueteOpcoes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enquetes_Tenants_TenantId",
                table: "Enquetes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnqueteVotos_Tenants_TenantId",
                table: "EnqueteVotos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_Tenants_TenantId",
                table: "Escalas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasItens_Tenants_TenantId",
                table: "EscalasItens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasModelos_Tenants_TenantId",
                table: "EscalasModelos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasModelosItens_Tenants_TenantId",
                table: "EscalasModelosItens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GaleriasFotos_Tenants_TenantId",
                table: "GaleriasFotos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GaleriasFotosItens_Tenants_TenantId",
                table: "GaleriasFotosItens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndisponibilidadesVoluntarios_Tenants_TenantId",
                table: "IndisponibilidadesVoluntarios",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacoesTrocasEscalas_Tenants_TenantId",
                table: "SolicitacoesTrocasEscalas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasMidias_Tenants_TenantId",
                table: "CategoriasMidias");

            migrationBuilder.DropForeignKey(
                name: "FK_EnqueteOpcoes_Tenants_TenantId",
                table: "EnqueteOpcoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Enquetes_Tenants_TenantId",
                table: "Enquetes");

            migrationBuilder.DropForeignKey(
                name: "FK_EnqueteVotos_Tenants_TenantId",
                table: "EnqueteVotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_Tenants_TenantId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasItens_Tenants_TenantId",
                table: "EscalasItens");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasModelos_Tenants_TenantId",
                table: "EscalasModelos");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasModelosItens_Tenants_TenantId",
                table: "EscalasModelosItens");

            migrationBuilder.DropForeignKey(
                name: "FK_GaleriasFotos_Tenants_TenantId",
                table: "GaleriasFotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GaleriasFotosItens_Tenants_TenantId",
                table: "GaleriasFotosItens");

            migrationBuilder.DropForeignKey(
                name: "FK_IndisponibilidadesVoluntarios_Tenants_TenantId",
                table: "IndisponibilidadesVoluntarios");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacoesTrocasEscalas_Tenants_TenantId",
                table: "SolicitacoesTrocasEscalas");

            migrationBuilder.DropIndex(
                name: "IX_SolicitacoesTrocasEscalas_TenantId_EscalaItemId_Status",
                table: "SolicitacoesTrocasEscalas");

            migrationBuilder.DropIndex(
                name: "IX_IndisponibilidadesVoluntarios_TenantId_VoluntarioId_Data",
                table: "IndisponibilidadesVoluntarios");

            migrationBuilder.DropIndex(
                name: "IX_GaleriasFotosItens_TenantId_GaleriaFotoId_NomeArquivo",
                table: "GaleriasFotosItens");

            migrationBuilder.DropIndex(
                name: "IX_GaleriasFotos_TenantId_Nome_Data",
                table: "GaleriasFotos");

            migrationBuilder.DropIndex(
                name: "IX_EscalasModelosItens_TenantId_EscalaModeloId_Ordem",
                table: "EscalasModelosItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasModelos_TenantId_EventoId_EquipeId",
                table: "EscalasModelos");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_EquipeId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_VoluntarioId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_TenantId_EventoOcorrenciaId_EquipeId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_EnqueteVotos_TenantId_EnqueteId_UsuarioId",
                table: "EnqueteVotos");

            migrationBuilder.DropIndex(
                name: "IX_Enquetes_TenantId_DataCriacao",
                table: "Enquetes");

            migrationBuilder.DropIndex(
                name: "IX_EnqueteOpcoes_TenantId_EnqueteId_Ordem",
                table: "EnqueteOpcoes");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasMidias_TenantId_Nome",
                table: "CategoriasMidias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SolicitacoesTrocasEscalas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IndisponibilidadesVoluntarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GaleriasFotosItens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GaleriasFotos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EscalasModelosItens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EscalasModelos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EnqueteVotos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Enquetes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EnqueteOpcoes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoriasMidias");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_EscalaItemId_Status",
                table: "SolicitacoesTrocasEscalas",
                columns: new[] { "EscalaItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IndisponibilidadesVoluntarios_VoluntarioId_Data",
                table: "IndisponibilidadesVoluntarios",
                columns: new[] { "VoluntarioId", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelos_EventoId_EquipeId",
                table: "EscalasModelos",
                columns: new[] { "EventoId", "EquipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_EscalaId_EquipeId",
                table: "EscalasItens",
                columns: new[] { "EscalaId", "EquipeId" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_EscalaId_VoluntarioId",
                table: "EscalasItens",
                columns: new[] { "EscalaId", "VoluntarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EventoOcorrenciaId_EquipeId",
                table: "Escalas",
                columns: new[] { "EventoOcorrenciaId", "EquipeId" },
                unique: true);
        }
    }
}
