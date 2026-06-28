using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModuloFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasDespesas",
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
                    table.PrimaryKey("PK_CategoriasDespesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CentrosCustos",
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
                    table.PrimaryKey("PK_CentrosCustos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Agencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Conta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TipoConta = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SaldoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasBancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataFim = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Orcamento = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ComprovanteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    CategoriaDespesaId = table.Column<int>(type: "integer", nullable: true),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: true),
                    CentroCustoId = table.Column<int>(type: "integer", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despesas_CategoriasDespesas_CategoriaDespesaId",
                        column: x => x.CategoriaDespesaId,
                        principalTable: "CategoriasDespesas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Despesas_CentrosCustos_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "CentrosCustos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Despesas_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Despesas_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Despesas_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Despesas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Receitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ComprovanteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: true),
                    CentroCustoId = table.Column<int>(type: "integer", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receitas_CentrosCustos_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "CentrosCustos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Receitas_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Receitas_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Receitas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_CategoriaDespesaId",
                table: "Despesas",
                column: "CategoriaDespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_CentroCustoId",
                table: "Despesas",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_ContaBancariaId",
                table: "Despesas",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_FornecedorId",
                table: "Despesas",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_ProjetoId",
                table: "Despesas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_UsuarioId",
                table: "Despesas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_CentroCustoId",
                table: "Receitas",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_ContaBancariaId",
                table: "Receitas",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_ProjetoId",
                table: "Receitas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_UsuarioId",
                table: "Receitas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "Receitas");

            migrationBuilder.DropTable(
                name: "CategoriasDespesas");

            migrationBuilder.DropTable(
                name: "CentrosCustos");

            migrationBuilder.DropTable(
                name: "ContasBancarias");

            migrationBuilder.DropTable(
                name: "Projetos");
        }
    }
}
