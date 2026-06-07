using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModuloDoacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalidadesDoacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    DescricaoPublica = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImagemUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorHex = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ValoresSugeridos = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ValorMinimo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    VisivelPortal = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteAnonimo = table.Column<bool>(type: "boolean", nullable: false),
                    PermitePix = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteCartaoCredito = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaReceitaId = table.Column<int>(type: "integer", nullable: true),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: true),
                    CentroCustoId = table.Column<int>(type: "integer", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalidadesDoacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalidadesDoacao_CategoriasReceitas_CategoriaReceitaId",
                        column: x => x.CategoriaReceitaId,
                        principalTable: "CategoriasReceitas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinalidadesDoacao_CentrosCustos_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "CentrosCustos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinalidadesDoacao_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinalidadesDoacao_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinalidadesDoacao_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoacoesOnline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FinalidadeDoacaoId = table.Column<int>(type: "integer", nullable: true),
                    PessoaId = table.Column<int>(type: "integer", nullable: true),
                    NomeDoador = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Anonima = table.Column<bool>(type: "boolean", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MetodoPagamento = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ExternalPaymentId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    PixCopiaECola = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PixQrCodeUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataConfirmacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReceitaId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoacoesOnline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoacoesOnline_FinalidadesDoacao_FinalidadeDoacaoId",
                        column: x => x.FinalidadeDoacaoId,
                        principalTable: "FinalidadesDoacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DoacoesOnline_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DoacoesOnline_Receitas_ReceitaId",
                        column: x => x.ReceitaId,
                        principalTable: "Receitas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DoacoesOnline_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_FinalidadeDoacaoId",
                table: "DoacoesOnline",
                column: "FinalidadeDoacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_PessoaId",
                table: "DoacoesOnline",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_ReceitaId",
                table: "DoacoesOnline",
                column: "ReceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_TenantId_ExternalPaymentId",
                table: "DoacoesOnline",
                columns: new[] { "TenantId", "ExternalPaymentId" });

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_TenantId_Status_DataCriacao",
                table: "DoacoesOnline",
                columns: new[] { "TenantId", "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_CategoriaReceitaId",
                table: "FinalidadesDoacao",
                column: "CategoriaReceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_CentroCustoId",
                table: "FinalidadesDoacao",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_ContaBancariaId",
                table: "FinalidadesDoacao",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_ProjetoId",
                table: "FinalidadesDoacao",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_TenantId_Ativo_VisivelPortal_Ordem",
                table: "FinalidadesDoacao",
                columns: new[] { "TenantId", "Ativo", "VisivelPortal", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_FinalidadesDoacao_TenantId_Slug",
                table: "FinalidadesDoacao",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoacoesOnline");

            migrationBuilder.DropTable(
                name: "FinalidadesDoacao");
        }
    }
}
