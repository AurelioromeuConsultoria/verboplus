using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EscalaModeloEIndisponibilidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxEscalasPorMes",
                table: "Voluntarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EscalasModelos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoId = table.Column<int>(type: "integer", nullable: true),
                    EquipeId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DiasFolgaAposEscala = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalasModelos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalasModelos_Equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "Equipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalasModelos_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndisponibilidadesVoluntarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoluntarioId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndisponibilidadesVoluntarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndisponibilidadesVoluntarios_Voluntarios_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscalasModelosItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EscalaModeloId = table.Column<int>(type: "integer", nullable: false),
                    CargoId = table.Column<int>(type: "integer", nullable: true),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalasModelosItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalasModelosItens_Cargos_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalasModelosItens_EscalasModelos_EscalaModeloId",
                        column: x => x.EscalaModeloId,
                        principalTable: "EscalasModelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelos_EquipeId",
                table: "EscalasModelos",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelos_EventoId_EquipeId",
                table: "EscalasModelos",
                columns: new[] { "EventoId", "EquipeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelosItens_CargoId",
                table: "EscalasModelosItens",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasModelosItens_EscalaModeloId",
                table: "EscalasModelosItens",
                column: "EscalaModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_IndisponibilidadesVoluntarios_VoluntarioId_Data",
                table: "IndisponibilidadesVoluntarios",
                columns: new[] { "VoluntarioId", "Data" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalasModelosItens");

            migrationBuilder.DropTable(
                name: "IndisponibilidadesVoluntarios");

            migrationBuilder.DropTable(
                name: "EscalasModelos");

            migrationBuilder.DropColumn(
                name: "MaxEscalasPorMes",
                table: "Voluntarios");
        }
    }
}
