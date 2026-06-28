using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarKidsConteudoAula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KidsConteudosAula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tema = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Versiculo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Resumo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AtividadeEmCasa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ObservacaoResponsavel = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EventoOcorrenciaId = table.Column<int>(type: "integer", nullable: true),
                    SalaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TurmaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PublicadoPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsConteudosAula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsConteudosAula_EventosOcorrencias_EventoOcorrenciaId",
                        column: x => x.EventoOcorrenciaId,
                        principalTable: "EventosOcorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsConteudosAula_Pessoas_PublicadoPorPessoaId",
                        column: x => x.PublicadoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsConteudosAula_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KidsConteudosAulaAnexos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ConteudoAulaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NomeExibicao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsConteudosAulaAnexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsConteudosAulaAnexos_KidsConteudosAula_ConteudoAulaId",
                        column: x => x.ConteudoAulaId,
                        principalTable: "KidsConteudosAula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KidsConteudosAulaAnexos_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAula_EventoOcorrenciaId",
                table: "KidsConteudosAula",
                column: "EventoOcorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAula_PublicadoPorPessoaId",
                table: "KidsConteudosAula",
                column: "PublicadoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAula_TenantId_SalaId_Status_DataReferencia",
                table: "KidsConteudosAula",
                columns: new[] { "TenantId", "SalaId", "Status", "DataReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAula_TenantId_Status_DataReferencia",
                table: "KidsConteudosAula",
                columns: new[] { "TenantId", "Status", "DataReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAula_TenantId_TurmaId_Status_DataReferencia",
                table: "KidsConteudosAula",
                columns: new[] { "TenantId", "TurmaId", "Status", "DataReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAulaAnexos_ConteudoAulaId",
                table: "KidsConteudosAulaAnexos",
                column: "ConteudoAulaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAulaAnexos_TenantId_ConteudoAulaId_Ordem",
                table: "KidsConteudosAulaAnexos",
                columns: new[] { "TenantId", "ConteudoAulaId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsConteudosAulaAnexos_TenantId_Tipo",
                table: "KidsConteudosAulaAnexos",
                columns: new[] { "TenantId", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidsConteudosAulaAnexos");

            migrationBuilder.DropTable(
                name: "KidsConteudosAula");
        }
    }
}
