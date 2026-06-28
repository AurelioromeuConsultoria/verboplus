using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLembretesEscalaItemsVoluntariado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataLembrete24HorasEnviado",
                table: "EscalasItens",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataLembrete7DiasEnviado",
                table: "EscalasItens",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataLembrete24HorasEnviado",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "DataLembrete7DiasEnviado",
                table: "EscalasItens");
        }
    }
}
