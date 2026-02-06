using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHubCasas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HubCasas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AbertoPorId = table.Column<int>(type: "int", nullable: false),
                    LiderId = table.Column<int>(type: "int", nullable: false),
                    TimoteoId = table.Column<int>(type: "int", nullable: false),
                    EnderecoCompleto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Anfitriao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HubCasas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HubCasas_Usuarios_AbertoPorId",
                        column: x => x.AbertoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HubCasas_Usuarios_LiderId",
                        column: x => x.LiderId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HubCasas_Usuarios_TimoteoId",
                        column: x => x.TimoteoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HubCasas_AbertoPorId",
                table: "HubCasas",
                column: "AbertoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_HubCasas_LiderId",
                table: "HubCasas",
                column: "LiderId");

            migrationBuilder.CreateIndex(
                name: "IX_HubCasas_TimoteoId",
                table: "HubCasas",
                column: "TimoteoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HubCasas");
        }
    }
}
