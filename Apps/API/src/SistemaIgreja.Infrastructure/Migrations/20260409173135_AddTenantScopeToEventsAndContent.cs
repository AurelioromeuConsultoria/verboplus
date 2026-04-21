using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToEventsAndContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InscricoesEventos_EventoId_WhatsApp",
                table: "InscricoesEventos");

            migrationBuilder.DropIndex(
                name: "IX_EventosRecorrencias_EventoId_Ativo",
                table: "EventosRecorrencias");

            migrationBuilder.DropIndex(
                name: "IX_EventosOcorrencias_EventoId_DataHoraInicio",
                table: "EventosOcorrencias");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Noticias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "InscricoesEventos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EventosRecorrencias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EventosOcorrencias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Eventos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DestaquesSite",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Contatos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CategoriasNoticias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"UPDATE ""Eventos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EventosRecorrencias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""EventosOcorrencias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""InscricoesEventos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""DestaquesSite"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""CategoriasNoticias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Noticias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Contatos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Noticias_TenantId",
                table: "Noticias",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesEventos_TenantId_EventoId_WhatsApp",
                table: "InscricoesEventos",
                columns: new[] { "TenantId", "EventoId", "WhatsApp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosRecorrencias_TenantId_EventoId_Ativo",
                table: "EventosRecorrencias",
                columns: new[] { "TenantId", "EventoId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosOcorrencias_TenantId_EventoId_DataHoraInicio",
                table: "EventosOcorrencias",
                columns: new[] { "TenantId", "EventoId", "DataHoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_TenantId_Titulo_DataInicio",
                table: "Eventos",
                columns: new[] { "TenantId", "Titulo", "DataInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_DestaquesSite_TenantId_Texto",
                table: "DestaquesSite",
                columns: new[] { "TenantId", "Texto" });

            migrationBuilder.CreateIndex(
                name: "IX_Contatos_TenantId",
                table: "Contatos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasNoticias_TenantId_Nome",
                table: "CategoriasNoticias",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasNoticias_Tenants_TenantId",
                table: "CategoriasNoticias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contatos_Tenants_TenantId",
                table: "Contatos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DestaquesSite_Tenants_TenantId",
                table: "DestaquesSite",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Eventos_Tenants_TenantId",
                table: "Eventos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosOcorrencias_Tenants_TenantId",
                table: "EventosOcorrencias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosRecorrencias_Tenants_TenantId",
                table: "EventosRecorrencias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InscricoesEventos_Tenants_TenantId",
                table: "InscricoesEventos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Noticias_Tenants_TenantId",
                table: "Noticias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasNoticias_Tenants_TenantId",
                table: "CategoriasNoticias");

            migrationBuilder.DropForeignKey(
                name: "FK_Contatos_Tenants_TenantId",
                table: "Contatos");

            migrationBuilder.DropForeignKey(
                name: "FK_DestaquesSite_Tenants_TenantId",
                table: "DestaquesSite");

            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_Tenants_TenantId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_EventosOcorrencias_Tenants_TenantId",
                table: "EventosOcorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_EventosRecorrencias_Tenants_TenantId",
                table: "EventosRecorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_InscricoesEventos_Tenants_TenantId",
                table: "InscricoesEventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Noticias_Tenants_TenantId",
                table: "Noticias");

            migrationBuilder.DropIndex(
                name: "IX_Noticias_TenantId",
                table: "Noticias");

            migrationBuilder.DropIndex(
                name: "IX_InscricoesEventos_TenantId_EventoId_WhatsApp",
                table: "InscricoesEventos");

            migrationBuilder.DropIndex(
                name: "IX_EventosRecorrencias_TenantId_EventoId_Ativo",
                table: "EventosRecorrencias");

            migrationBuilder.DropIndex(
                name: "IX_EventosOcorrencias_TenantId_EventoId_DataHoraInicio",
                table: "EventosOcorrencias");

            migrationBuilder.DropIndex(
                name: "IX_Eventos_TenantId_Titulo_DataInicio",
                table: "Eventos");

            migrationBuilder.DropIndex(
                name: "IX_DestaquesSite_TenantId_Texto",
                table: "DestaquesSite");

            migrationBuilder.DropIndex(
                name: "IX_Contatos_TenantId",
                table: "Contatos");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasNoticias_TenantId_Nome",
                table: "CategoriasNoticias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Noticias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InscricoesEventos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EventosRecorrencias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EventosOcorrencias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DestaquesSite");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Contatos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoriasNoticias");

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesEventos_EventoId_WhatsApp",
                table: "InscricoesEventos",
                columns: new[] { "EventoId", "WhatsApp" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosRecorrencias_EventoId_Ativo",
                table: "EventosRecorrencias",
                columns: new[] { "EventoId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosOcorrencias_EventoId_DataHoraInicio",
                table: "EventosOcorrencias",
                columns: new[] { "EventoId", "DataHoraInicio" });
        }
    }
}
