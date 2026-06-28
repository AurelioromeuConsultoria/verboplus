using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarReciboDoacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReciboToken",
                table: "DoacoesOnline",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoacoesOnline_TenantId_ReciboToken",
                table: "DoacoesOnline",
                columns: new[] { "TenantId", "ReciboToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoacoesOnline_TenantId_ReciboToken",
                table: "DoacoesOnline");

            migrationBuilder.DropColumn(
                name: "ReciboToken",
                table: "DoacoesOnline");
        }
    }
}
