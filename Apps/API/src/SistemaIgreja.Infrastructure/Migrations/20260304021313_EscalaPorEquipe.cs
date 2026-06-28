using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EscalaPorEquipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipeId",
                table: "Escalas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EquipeId",
                table: "Escalas",
                column: "EquipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_Equipes_EquipeId",
                table: "Escalas",
                column: "EquipeId",
                principalTable: "Equipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Dados: definir EquipeId nas escalas existentes (primeira equipe dos itens)
            migrationBuilder.Sql(@"
                UPDATE ""Escalas"" e
                SET ""EquipeId"" = (SELECT it.""EquipeId"" FROM ""EscalasItens"" it WHERE it.""EscalaId"" = e.""Id"" LIMIT 1)
                WHERE e.""EquipeId"" IS NULL;
            ");

            // Escalas sem itens: usar primeira equipe do sistema (evita null)
            migrationBuilder.Sql(@"
                UPDATE ""Escalas"" e
                SET ""EquipeId"" = (SELECT ""Id"" FROM ""Equipes"" LIMIT 1)
                WHERE e.""EquipeId"" IS NULL;
            ");

            // Remover escalas órfãs (sem itens e sem equipe válida)
            migrationBuilder.Sql(@"DELETE FROM ""Escalas"" WHERE ""EquipeId"" IS NULL;");

            // Criar novas escalas para as demais equipes que já têm itens na mesma ocorrência
            migrationBuilder.Sql(@"
                INSERT INTO ""Escalas"" (""EventoOcorrenciaId"", ""EquipeId"", ""Status"", ""Observacoes"", ""CriadoPorUsuarioId"", ""DataCriacao"", ""DataPublicacao"")
                SELECT DISTINCT ON (e.""EventoOcorrenciaId"", it.""EquipeId"")
                    e.""EventoOcorrenciaId"", it.""EquipeId"", e.""Status"", e.""Observacoes"", e.""CriadoPorUsuarioId"", e.""DataCriacao"", e.""DataPublicacao""
                FROM ""Escalas"" e
                INNER JOIN ""EscalasItens"" it ON it.""EscalaId"" = e.""Id""
                WHERE it.""EquipeId"" != e.""EquipeId""
                ORDER BY e.""EventoOcorrenciaId"", it.""EquipeId"";
            ");

            // Apontar itens para a escala correta (mesma ocorrência + mesma equipe)
            migrationBuilder.Sql(@"
                UPDATE ""EscalasItens"" it
                SET ""EscalaId"" = (
                    SELECT es.""Id"" FROM ""Escalas"" es
                    WHERE es.""EventoOcorrenciaId"" = (SELECT e2.""EventoOcorrenciaId"" FROM ""Escalas"" e2 WHERE e2.""Id"" = it.""EscalaId"")
                      AND es.""EquipeId"" = it.""EquipeId""
                    LIMIT 1
                )
                WHERE it.""EquipeId"" != (SELECT e3.""EquipeId"" FROM ""Escalas"" e3 WHERE e3.""Id"" = it.""EscalaId"");
            ");

            // Remover escalas vazias quando existir outra escala para o mesmo (EventoOcorrenciaId, EquipeId)
            migrationBuilder.Sql(@"
                DELETE FROM ""Escalas"" e
                WHERE NOT EXISTS (SELECT 1 FROM ""EscalasItens"" it WHERE it.""EscalaId"" = e.""Id"")
                  AND EXISTS (SELECT 1 FROM ""Escalas"" e2 WHERE e2.""EventoOcorrenciaId"" = e.""EventoOcorrenciaId"" AND e2.""EquipeId"" = e.""EquipeId"" AND e2.""Id"" != e.""Id"");
            ");

            migrationBuilder.AlterColumn<int>(
                name: "EquipeId",
                table: "Escalas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_Escalas_EventoOcorrenciaId",
                table: "Escalas");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EventoOcorrenciaId_EquipeId",
                table: "Escalas",
                columns: new[] { "EventoOcorrenciaId", "EquipeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Escalas_EventoOcorrenciaId_EquipeId",
                table: "Escalas");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EventoOcorrenciaId",
                table: "Escalas",
                column: "EventoOcorrenciaId",
                unique: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_Equipes_EquipeId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_EquipeId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "EquipeId",
                table: "Escalas");
        }
    }
}
