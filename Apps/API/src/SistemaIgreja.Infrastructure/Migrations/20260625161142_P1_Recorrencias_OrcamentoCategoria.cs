using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P1_Recorrencias_OrcamentoCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaOriginalId",
                table: "Receitas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Recorrente",
                table: "Receitas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoRecorrencia",
                table: "Receitas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaOriginalId",
                table: "Despesas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Recorrente",
                table: "Despesas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoRecorrencia",
                table: "Despesas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrcamentoCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    CategoriaReceitaId = table.Column<int>(type: "integer", nullable: true),
                    CategoriaDespesaId = table.Column<int>(type: "integer", nullable: true),
                    ValorOrcado = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentoCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentoCategorias_CategoriasDespesas_CategoriaDespesaId",
                        column: x => x.CategoriaDespesaId,
                        principalTable: "CategoriasDespesas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoCategorias_CategoriasReceitas_CategoriaReceitaId",
                        column: x => x.CategoriaReceitaId,
                        principalTable: "CategoriasReceitas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoCategorias_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_RecorrenciaOriginalId",
                table: "Receitas",
                column: "RecorrenciaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_RecorrenciaOriginalId",
                table: "Despesas",
                column: "RecorrenciaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoCategorias_CategoriaDespesaId",
                table: "OrcamentoCategorias",
                column: "CategoriaDespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoCategorias_CategoriaReceitaId",
                table: "OrcamentoCategorias",
                column: "CategoriaReceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoCategorias_TenantId_Ano_Tipo",
                table: "OrcamentoCategorias",
                columns: new[] { "TenantId", "Ano", "Tipo" });

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Despesas_RecorrenciaOriginalId",
                table: "Despesas",
                column: "RecorrenciaOriginalId",
                principalTable: "Despesas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receitas_Receitas_RecorrenciaOriginalId",
                table: "Receitas",
                column: "RecorrenciaOriginalId",
                principalTable: "Receitas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Despesas_RecorrenciaOriginalId",
                table: "Despesas");

            migrationBuilder.DropForeignKey(
                name: "FK_Receitas_Receitas_RecorrenciaOriginalId",
                table: "Receitas");

            migrationBuilder.DropTable(
                name: "OrcamentoCategorias");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_RecorrenciaOriginalId",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Despesas_RecorrenciaOriginalId",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaOriginalId",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "Recorrente",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "TipoRecorrencia",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaOriginalId",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "Recorrente",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "TipoRecorrencia",
                table: "Despesas");
        }
    }
}
