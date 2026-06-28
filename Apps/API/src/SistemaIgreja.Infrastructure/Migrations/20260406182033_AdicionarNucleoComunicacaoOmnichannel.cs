using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNucleoComunicacaoOmnichannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComunicacaoCampanhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PublicoAlvo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    DataAgendamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoCampanhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicacaoCampanhas_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComunicacaoPreferencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrigemConsentimento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoPreferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicacaoPreferencias_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComunicacaoTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    Assunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Corpo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CorpoHtml = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VariaveisPermitidas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Versao = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComunicacaoEntregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComunicacaoCampanhaId = table.Column<int>(type: "integer", nullable: true),
                    DestinatarioPessoaId = table.Column<int>(type: "integer", nullable: true),
                    DestinatarioVisitanteId = table.Column<int>(type: "integer", nullable: true),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    DestinoResolvido = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RemetenteResolvido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConteudoFinal = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConteudoHtmlFinal = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EntregueEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChaveDedupe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoEntregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicacaoEntregas_ComunicacaoCampanhas_ComunicacaoCampanh~",
                        column: x => x.ComunicacaoCampanhaId,
                        principalTable: "ComunicacaoCampanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComunicacaoEntregas_Pessoas_DestinatarioPessoaId",
                        column: x => x.DestinatarioPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComunicacaoEntregas_Visitantes_DestinatarioVisitanteId",
                        column: x => x.DestinatarioVisitanteId,
                        principalTable: "Visitantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComunicacaoAutomacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Gatilho = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SegmentoAlvo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    DelayMinutos = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoAutomacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicacaoAutomacoes_ComunicacaoTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ComunicacaoTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComunicacaoCampanhaCanais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComunicacaoCampanhaId = table.Column<int>(type: "integer", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    Prioridade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoCampanhaCanais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicacaoCampanhaCanais_ComunicacaoCampanhas_ComunicacaoC~",
                        column: x => x.ComunicacaoCampanhaId,
                        principalTable: "ComunicacaoCampanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComunicacaoCampanhaCanais_ComunicacaoTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ComunicacaoTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoAutomacoes_TemplateId",
                table: "ComunicacaoAutomacoes",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoCampanhaCanais_ComunicacaoCampanhaId",
                table: "ComunicacaoCampanhaCanais",
                column: "ComunicacaoCampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoCampanhaCanais_TemplateId",
                table: "ComunicacaoCampanhaCanais",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoCampanhas_CriadoPorUsuarioId",
                table: "ComunicacaoCampanhas",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoEntregas_ChaveDedupe",
                table: "ComunicacaoEntregas",
                column: "ChaveDedupe");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoEntregas_ComunicacaoCampanhaId",
                table: "ComunicacaoEntregas",
                column: "ComunicacaoCampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoEntregas_DestinatarioPessoaId",
                table: "ComunicacaoEntregas",
                column: "DestinatarioPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoEntregas_DestinatarioVisitanteId",
                table: "ComunicacaoEntregas",
                column: "DestinatarioVisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoEntregas_Status_Canal_DataCriacao",
                table: "ComunicacaoEntregas",
                columns: new[] { "Status", "Canal", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_ComunicacaoPreferencias_PessoaId_Canal",
                table: "ComunicacaoPreferencias",
                columns: new[] { "PessoaId", "Canal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComunicacaoAutomacoes");

            migrationBuilder.DropTable(
                name: "ComunicacaoCampanhaCanais");

            migrationBuilder.DropTable(
                name: "ComunicacaoEntregas");

            migrationBuilder.DropTable(
                name: "ComunicacaoPreferencias");

            migrationBuilder.DropTable(
                name: "ComunicacaoTemplates");

            migrationBuilder.DropTable(
                name: "ComunicacaoCampanhas");
        }
    }
}
