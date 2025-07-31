using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesMensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TextoMensagem = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DiasAposVisita = table.Column<int>(type: "INTEGER", nullable: false),
                    HorarioEnvio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesMensagens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Visitantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataVisita = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MensagensAgendadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VisitanteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfiguracaoMensagemId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAgendamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TextoFinal = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LogErro = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensAgendadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensAgendadas_ConfiguracoesMensagens_ConfiguracaoMensagemId",
                        column: x => x.ConfiguracaoMensagemId,
                        principalTable: "ConfiguracoesMensagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensagensAgendadas_Visitantes_VisitanteId",
                        column: x => x.VisitanteId,
                        principalTable: "Visitantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracoesMensagens",
                columns: new[] { "Id", "Ativo", "DataCriacao", "DiasAposVisita", "HorarioEnvio", "Nome", "TextoMensagem" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 6, 26, 23, 10, 6, 153, DateTimeKind.Local).AddTicks(841), 1, new TimeSpan(0, 10, 0, 0, 0), "Boas-vindas", "Olá {Nome}! Que alegria ter você conosco na igreja! Esperamos vê-lo novamente em breve. Deus abençoe!" },
                    { 2, true, new DateTime(2025, 6, 26, 23, 10, 6, 153, DateTimeKind.Local).AddTicks(1116), 7, new TimeSpan(0, 18, 0, 0, 0), "Convite para retorno", "Oi {Nome}! Sentimos sua falta na igreja. Que tal nos visitar novamente neste domingo? Será um prazer recebê-lo!" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_ConfiguracaoMensagemId",
                table: "MensagensAgendadas",
                column: "ConfiguracaoMensagemId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_VisitanteId",
                table: "MensagensAgendadas",
                column: "VisitanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensagensAgendadas");

            migrationBuilder.DropTable(
                name: "ConfiguracoesMensagens");

            migrationBuilder.DropTable(
                name: "Visitantes");
        }
    }
}
