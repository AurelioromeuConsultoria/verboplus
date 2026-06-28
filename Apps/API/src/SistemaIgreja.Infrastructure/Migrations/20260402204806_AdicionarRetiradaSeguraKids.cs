using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRetiradaSeguraKids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PinRetirada",
                table: "KidsCheckins",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetiradaConfirmadaPorPessoaId",
                table: "KidsCheckins",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RetiradaEmModoExcecao",
                table: "KidsCheckins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RetiradaMetodo",
                table: "KidsCheckins",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiradaMotivoExcecao",
                table: "KidsCheckins",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiradaPessoaDocumento",
                table: "KidsCheckins",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiradaPessoaNome",
                table: "KidsCheckins",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRetirada",
                table: "KidsCheckins",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenRetiradaExpiraEm",
                table: "KidsCheckins",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_TokenRetirada",
                table: "KidsCheckins",
                column: "TokenRetirada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KidsCheckins_TokenRetirada",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "PinRetirada",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaConfirmadaPorPessoaId",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaEmModoExcecao",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaMetodo",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaMotivoExcecao",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaPessoaDocumento",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "RetiradaPessoaNome",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "TokenRetirada",
                table: "KidsCheckins");

            migrationBuilder.DropColumn(
                name: "TokenRetiradaExpiraEm",
                table: "KidsCheckins");
        }
    }
}
