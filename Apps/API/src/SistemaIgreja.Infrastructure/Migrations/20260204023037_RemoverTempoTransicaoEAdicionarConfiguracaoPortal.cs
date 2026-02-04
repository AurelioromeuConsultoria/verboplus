using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverTempoTransicaoEAdicionarConfiguracaoPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempoTransicao",
                table: "DestaquesSite");

            migrationBuilder.CreateTable(
                name: "ConfiguracoesPortal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TempoTransicaoCarrossel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesPortal", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracoesPortal",
                columns: new[] { "Id", "DataAtualizacao", "TempoTransicaoCarrossel" },
                values: new object[] { 1, new DateTime(2026, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesPortal");

            migrationBuilder.AddColumn<int>(
                name: "TempoTransicao",
                table: "DestaquesSite",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }
    }
}
