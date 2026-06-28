using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTenantIdComunicacaoNotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "NotificacoesUsuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoSegmentos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoPreferencias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoEntregas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoCampanhas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoCampanhaCanais",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ComunicacaoAutomacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // ===== Backfill do TenantId nos registros existentes =====
            // Entidades com vínculo herdam o tenant do relacionamento; as sem
            // vínculo (campanhas/templates/segmentos/automações) recebem o tenant
            // inicial (1). Idempotente: só toca linhas com TenantId = 0.
            migrationBuilder.Sql(@"
                UPDATE ""NotificacoesUsuarios"" n SET ""TenantId"" = u.""TenantId""
                FROM ""Usuarios"" u WHERE u.""Id"" = n.""UsuarioId"" AND n.""TenantId"" = 0;

                UPDATE ""ComunicacaoCampanhas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""ComunicacaoTemplates"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""ComunicacaoSegmentos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""ComunicacaoAutomacoes"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;

                UPDATE ""ComunicacaoPreferencias"" pr SET ""TenantId"" = p.""TenantId""
                FROM ""Pessoas"" p WHERE p.""Id"" = pr.""PessoaId"" AND pr.""TenantId"" = 0;

                UPDATE ""ComunicacaoCampanhaCanais"" cc SET ""TenantId"" = c.""TenantId""
                FROM ""ComunicacaoCampanhas"" c WHERE c.""Id"" = cc.""ComunicacaoCampanhaId"" AND cc.""TenantId"" = 0;

                UPDATE ""ComunicacaoEntregas"" e SET ""TenantId"" = c.""TenantId""
                FROM ""ComunicacaoCampanhas"" c WHERE c.""Id"" = e.""ComunicacaoCampanhaId"" AND e.""TenantId"" = 0;
                UPDATE ""ComunicacaoEntregas"" e SET ""TenantId"" = p.""TenantId""
                FROM ""Pessoas"" p WHERE p.""Id"" = e.""DestinatarioPessoaId"" AND e.""TenantId"" = 0;

                UPDATE ""ComunicacaoPreferencias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""ComunicacaoCampanhaCanais"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""ComunicacaoEntregas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
                UPDATE ""NotificacoesUsuarios"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "NotificacoesUsuarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoSegmentos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoPreferencias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoEntregas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoCampanhas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoCampanhaCanais");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ComunicacaoAutomacoes");
        }
    }
}
