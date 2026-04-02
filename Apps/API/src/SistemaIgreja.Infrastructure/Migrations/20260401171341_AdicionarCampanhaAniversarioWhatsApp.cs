using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCampanhaAniversarioWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasPatrimonio",
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
                    table.PrimaryKey("PK_CategoriasPatrimonio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesCampanhaAniversario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MensagemTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    HorarioEnvio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesCampanhaAniversario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnviosCampanhaAniversario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    AnoReferencia = table.Column<int>(type: "integer", nullable: false),
                    DataAniversario = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    DataUltimaTentativa = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataEnvioSucesso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WhatsAppUtilizado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImagemUrlUtilizada = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MensagemUtilizada = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LogErro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosCampanhaAniversario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnviosCampanhaAniversario_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatrimonioItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoriaPatrimonioId = table.Column<int>(type: "integer", nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Campus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Localizacao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MinisterioArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponsavelPessoaId = table.Column<int>(type: "integer", nullable: true),
                    TipoAquisicao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataAquisicao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ValorAquisicao = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    NumeroNotaFiscal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DespesaId = table.Column<int>(type: "integer", nullable: true),
                    CentroCustoId = table.Column<int>(type: "integer", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstadoConservacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataUltimaAvaliacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PossuiGarantia = table.Column<bool>(type: "boolean", nullable: false),
                    GarantiaAte = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataUltimaManutencao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataProximaManutencao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatrimonioItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_CategoriasPatrimonio_CategoriaPatrimonioId",
                        column: x => x.CategoriaPatrimonioId,
                        principalTable: "CategoriasPatrimonio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_CentrosCustos_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "CentrosCustos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_Despesas_DespesaId",
                        column: x => x.DespesaId,
                        principalTable: "Despesas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_Pessoas_ResponsavelPessoaId",
                        column: x => x.ResponsavelPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatrimonioItens_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasPatrimonio_Nome",
                table: "CategoriasPatrimonio",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCampanhaAniversario_PessoaId_AnoReferencia",
                table: "EnviosCampanhaAniversario",
                columns: new[] { "PessoaId", "AnoReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_CategoriaPatrimonioId",
                table: "PatrimonioItens",
                column: "CategoriaPatrimonioId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_CentroCustoId",
                table: "PatrimonioItens",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_Codigo",
                table: "PatrimonioItens",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_DespesaId",
                table: "PatrimonioItens",
                column: "DespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_FornecedorId",
                table: "PatrimonioItens",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_ProjetoId",
                table: "PatrimonioItens",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_ResponsavelPessoaId",
                table: "PatrimonioItens",
                column: "ResponsavelPessoaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesCampanhaAniversario");

            migrationBuilder.DropTable(
                name: "EnviosCampanhaAniversario");

            migrationBuilder.DropTable(
                name: "PatrimonioItens");

            migrationBuilder.DropTable(
                name: "CategoriasPatrimonio");
        }
    }
}
