using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriaReceitaEDashboardFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaReceitaId",
                table: "Receitas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoriasReceitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasReceitas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_CategoriaReceitaId",
                table: "Receitas",
                column: "CategoriaReceitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receitas_CategoriasReceitas_CategoriaReceitaId",
                table: "Receitas",
                column: "CategoriaReceitaId",
                principalTable: "CategoriasReceitas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receitas_CategoriasReceitas_CategoriaReceitaId",
                table: "Receitas");

            migrationBuilder.DropTable(
                name: "CategoriasReceitas");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_CategoriaReceitaId",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "CategoriaReceitaId",
                table: "Receitas");
        }
    }
}
