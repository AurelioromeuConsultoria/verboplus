using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEstruturaEscalasVoluntariado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Eventos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EhRecorrente",
                table: "Eventos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Eventos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "EventosRecorrencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Periodicidade = table.Column<int>(type: "integer", nullable: false),
                    DataInicioVigencia = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFimVigencia = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosRecorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosRecorrencias_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventosOcorrencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    EventoRecorrenciaId = table.Column<int>(type: "integer", nullable: true),
                    DataHoraInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataHoraFim = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeradaAutomaticamente = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosOcorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosOcorrencias_EventosRecorrencias_EventoRecorrenciaId",
                        column: x => x.EventoRecorrenciaId,
                        principalTable: "EventosRecorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventosOcorrencias_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Escalas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoOcorrenciaId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataPublicacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Escalas_EventosOcorrencias_EventoOcorrenciaId",
                        column: x => x.EventoOcorrenciaId,
                        principalTable: "EventosOcorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Escalas_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EscalasItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EscalaId = table.Column<int>(type: "integer", nullable: false),
                    EquipeId = table.Column<int>(type: "integer", nullable: false),
                    CargoId = table.Column<int>(type: "integer", nullable: true),
                    VoluntarioId = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    ConflitoAprovado = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoExcecao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AprovadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    AprovadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalasItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalasItens_Cargos_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalasItens_Equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "Equipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalasItens_Escalas_EscalaId",
                        column: x => x.EscalaId,
                        principalTable: "Escalas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalasItens_Usuarios_AprovadoPorUsuarioId",
                        column: x => x.AprovadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalasItens_Voluntarios_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_CriadoPorUsuarioId",
                table: "Escalas",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EventoOcorrenciaId",
                table: "Escalas",
                column: "EventoOcorrenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_AprovadoPorUsuarioId",
                table: "EscalasItens",
                column: "AprovadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_CargoId",
                table: "EscalasItens",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_EquipeId",
                table: "EscalasItens",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_EscalaId_EquipeId",
                table: "EscalasItens",
                columns: new[] { "EscalaId", "EquipeId" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_EscalaId_VoluntarioId",
                table: "EscalasItens",
                columns: new[] { "EscalaId", "VoluntarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_VoluntarioId",
                table: "EscalasItens",
                column: "VoluntarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosOcorrencias_EventoId_DataHoraInicio",
                table: "EventosOcorrencias",
                columns: new[] { "EventoId", "DataHoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosOcorrencias_EventoRecorrenciaId",
                table: "EventosOcorrencias",
                column: "EventoRecorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosRecorrencias_EventoId_Ativo",
                table: "EventosRecorrencias",
                columns: new[] { "EventoId", "Ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalasItens");

            migrationBuilder.DropTable(
                name: "Escalas");

            migrationBuilder.DropTable(
                name: "EventosOcorrencias");

            migrationBuilder.DropTable(
                name: "EventosRecorrencias");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "EhRecorrente",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Eventos");
        }
    }
}
