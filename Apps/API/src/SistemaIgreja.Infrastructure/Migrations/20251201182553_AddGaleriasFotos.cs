using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGaleriasFotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasMidias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasMidias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GaleriasFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CaminhoDiretorio = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImagemDestaque = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    QuantidadeFotos = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    EventoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CategoriaMidiaId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaleriasFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GaleriasFotos_CategoriasMidias_CategoriaMidiaId",
                        column: x => x.CategoriaMidiaId,
                        principalTable: "CategoriasMidias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GaleriasFotos_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GaleriasFotos_CategoriaMidiaId",
                table: "GaleriasFotos",
                column: "CategoriaMidiaId");

            migrationBuilder.CreateIndex(
                name: "IX_GaleriasFotos_EventoId",
                table: "GaleriasFotos",
                column: "EventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GaleriasFotos");

            migrationBuilder.DropTable(
                name: "CategoriasMidias");
        }
    }
}
