using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEnquetes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enquetes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    PermitirMultiplaEscolha = table.Column<bool>(type: "bit", nullable: false),
                    PermitirVotoAnonimo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquetes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnqueteOpcoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnqueteId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnqueteOpcoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnqueteOpcoes_Enquetes_EnqueteId",
                        column: x => x.EnqueteId,
                        principalTable: "Enquetes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnqueteVotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnqueteId = table.Column<int>(type: "int", nullable: false),
                    EnqueteOpcaoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    NomeAnonimo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataVoto = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnqueteVotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnqueteVotos_EnqueteOpcoes_EnqueteOpcaoId",
                        column: x => x.EnqueteOpcaoId,
                        principalTable: "EnqueteOpcoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnqueteVotos_Enquetes_EnqueteId",
                        column: x => x.EnqueteId,
                        principalTable: "Enquetes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnqueteVotos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteOpcoes_EnqueteId",
                table: "EnqueteOpcoes",
                column: "EnqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteVotos_EnqueteId",
                table: "EnqueteVotos",
                column: "EnqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteVotos_EnqueteOpcaoId",
                table: "EnqueteVotos",
                column: "EnqueteOpcaoId");

            migrationBuilder.CreateIndex(
                name: "IX_EnqueteVotos_UsuarioId",
                table: "EnqueteVotos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnqueteVotos");

            migrationBuilder.DropTable(
                name: "EnqueteOpcoes");

            migrationBuilder.DropTable(
                name: "Enquetes");
        }
    }
}
