using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EvoluirKidsNotificacoesAvisosReais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CriancaPessoaId",
                table: "KidsNotificacoes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CriadoByPessoaId",
                table: "KidsNotificacoes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LidoEm",
                table: "KidsNotificacoes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "KidsNotificacoes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "KidsNotificacoes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "KidsNotificacoes"
                SET "Origem" = CASE
                        WHEN "Tipo" IN ('CHECKIN', 'CHECKOUT', 'ALERTA') THEN 'AUTOMATICA'
                        ELSE 'MANUAL'
                    END,
                    "Titulo" = CASE
                        WHEN "Tipo" = 'CHECKIN' THEN 'Check-in realizado'
                        WHEN "Tipo" = 'CHECKOUT' THEN 'Check-out realizado'
                        WHEN "Tipo" = 'ALERTA' THEN 'Alerta do Kids'
                        ELSE 'Aviso Kids'
                    END
                WHERE "Origem" = '' OR "Titulo" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes",
                columns: new[] { "ResponsavelPessoaId", "LidoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_LidoEm",
                table: "KidsNotificacoes");

            migrationBuilder.DropColumn(
                name: "CriadoByPessoaId",
                table: "KidsNotificacoes");

            migrationBuilder.DropColumn(
                name: "LidoEm",
                table: "KidsNotificacoes");

            migrationBuilder.DropColumn(
                name: "Origem",
                table: "KidsNotificacoes");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "KidsNotificacoes");

            migrationBuilder.AlterColumn<int>(
                name: "CriancaPessoaId",
                table: "KidsNotificacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
