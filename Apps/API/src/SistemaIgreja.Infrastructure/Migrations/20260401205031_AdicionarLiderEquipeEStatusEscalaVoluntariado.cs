using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLiderEquipeEStatusEscalaVoluntariado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataConfirmacao",
                table: "EscalasItens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConvite",
                table: "EscalasItens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRecusa",
                table: "EscalasItens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRecusa",
                table: "EscalasItens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacaoOperacional",
                table: "EscalasItens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RespondidoPorUsuarioId",
                table: "EscalasItens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EscalasItens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LiderUsuarioId",
                table: "Equipes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_RespondidoPorUsuarioId",
                table: "EscalasItens",
                column: "RespondidoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipes_LiderUsuarioId",
                table: "Equipes",
                column: "LiderUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipes_Usuarios_LiderUsuarioId",
                table: "Equipes",
                column: "LiderUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasItens_Usuarios_RespondidoPorUsuarioId",
                table: "EscalasItens",
                column: "RespondidoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipes_Usuarios_LiderUsuarioId",
                table: "Equipes");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasItens_Usuarios_RespondidoPorUsuarioId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_RespondidoPorUsuarioId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_Equipes_LiderUsuarioId",
                table: "Equipes");

            migrationBuilder.DropColumn(
                name: "DataConfirmacao",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "DataConvite",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "DataRecusa",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "MotivoRecusa",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "ObservacaoOperacional",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "RespondidoPorUsuarioId",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "LiderUsuarioId",
                table: "Equipes");
        }
    }
}
