using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarKidsOcorrencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KidsOcorrencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriancaPessoaId = table.Column<int>(type: "integer", nullable: false),
                    CheckinId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequerContatoResponsavel = table.Column<bool>(type: "boolean", nullable: false),
                    ContatoResponsavelRealizadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ContatoResponsavelPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    SalaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TurmaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistradoPorPessoaId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EncerradoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EncerradoPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    VisivelAoResponsavel = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsOcorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsOcorrencias_KidsCheckins_CheckinId",
                        column: x => x.CheckinId,
                        principalTable: "KidsCheckins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsOcorrencias_Pessoas_ContatoResponsavelPorPessoaId",
                        column: x => x.ContatoResponsavelPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsOcorrencias_Pessoas_CriancaPessoaId",
                        column: x => x.CriancaPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KidsOcorrencias_Pessoas_EncerradoPorPessoaId",
                        column: x => x.EncerradoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsOcorrencias_Pessoas_RegistradoPorPessoaId",
                        column: x => x.RegistradoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_CheckinId",
                table: "KidsOcorrencias",
                column: "CheckinId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_ContatoResponsavelPorPessoaId",
                table: "KidsOcorrencias",
                column: "ContatoResponsavelPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_CriancaPessoaId_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "CriancaPessoaId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_EncerradoPorPessoaId",
                table: "KidsOcorrencias",
                column: "EncerradoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_RegistradoPorPessoaId",
                table: "KidsOcorrencias",
                column: "RegistradoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsOcorrencias_Status_DataCriacao",
                table: "KidsOcorrencias",
                columns: new[] { "Status", "DataCriacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidsOcorrencias");
        }
    }
}
