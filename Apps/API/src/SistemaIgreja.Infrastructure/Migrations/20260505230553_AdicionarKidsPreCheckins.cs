using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarKidsPreCheckins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KidsPreCheckins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CriancaPessoaId = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelPessoaId = table.Column<int>(type: "integer", nullable: false),
                    EventoOcorrenciaId = table.Column<int>(type: "integer", nullable: true),
                    SalaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TurmaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QrToken = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CodigoCurto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ObservacoesResponsavel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConfirmadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ConfirmadoPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    CanceladoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CanceladoPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    CancelamentoMotivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsPreCheckins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_EventosOcorrencias_EventoOcorrenciaId",
                        column: x => x.EventoOcorrenciaId,
                        principalTable: "EventosOcorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_Pessoas_CanceladoPorPessoaId",
                        column: x => x.CanceladoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_Pessoas_ConfirmadoPorPessoaId",
                        column: x => x.ConfirmadoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_Pessoas_CriancaPessoaId",
                        column: x => x.CriancaPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_Pessoas_ResponsavelPessoaId",
                        column: x => x.ResponsavelPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KidsPreCheckins_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_CanceladoPorPessoaId",
                table: "KidsPreCheckins",
                column: "CanceladoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_ConfirmadoPorPessoaId",
                table: "KidsPreCheckins",
                column: "ConfirmadoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_CriancaPessoaId",
                table: "KidsPreCheckins",
                column: "CriancaPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_EventoOcorrenciaId",
                table: "KidsPreCheckins",
                column: "EventoOcorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_ResponsavelPessoaId",
                table: "KidsPreCheckins",
                column: "ResponsavelPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_CodigoCurto",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "CodigoCurto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_CriancaPessoaId_EventoOcorrenciaId~",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "CriancaPessoaId", "EventoOcorrenciaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_ExpiraEm_Status",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "ExpiraEm", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_QrToken",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "QrToken" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_ResponsavelPessoaId_Status",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "ResponsavelPessoaId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidsPreCheckins");
        }
    }
}
