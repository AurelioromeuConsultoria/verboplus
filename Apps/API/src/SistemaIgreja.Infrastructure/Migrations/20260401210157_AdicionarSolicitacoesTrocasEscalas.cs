using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSolicitacoesTrocasEscalas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesTrocasEscalas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EscalaItemId = table.Column<int>(type: "integer", nullable: false),
                    VoluntarioSolicitanteId = table.Column<int>(type: "integer", nullable: false),
                    VoluntarioSubstitutoId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ObservacaoResposta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RespondidoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesTrocasEscalas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTrocasEscalas_EscalasItens_EscalaItemId",
                        column: x => x.EscalaItemId,
                        principalTable: "EscalasItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTrocasEscalas_Usuarios_RespondidoPorUsuarioId",
                        column: x => x.RespondidoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTrocasEscalas_Voluntarios_VoluntarioSolicitante~",
                        column: x => x.VoluntarioSolicitanteId,
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTrocasEscalas_Voluntarios_VoluntarioSubstitutoId",
                        column: x => x.VoluntarioSubstitutoId,
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_EscalaItemId_Status",
                table: "SolicitacoesTrocasEscalas",
                columns: new[] { "EscalaItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_RespondidoPorUsuarioId",
                table: "SolicitacoesTrocasEscalas",
                column: "RespondidoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_VoluntarioSolicitanteId",
                table: "SolicitacoesTrocasEscalas",
                column: "VoluntarioSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTrocasEscalas_VoluntarioSubstitutoId",
                table: "SolicitacoesTrocasEscalas",
                column: "VoluntarioSubstitutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesTrocasEscalas");
        }
    }
}
