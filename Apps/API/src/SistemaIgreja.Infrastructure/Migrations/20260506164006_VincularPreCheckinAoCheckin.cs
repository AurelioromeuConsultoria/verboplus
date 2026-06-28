using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VincularPreCheckinAoCheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckinId",
                table: "KidsPreCheckins",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_CheckinId",
                table: "KidsPreCheckins",
                column: "CheckinId");

            migrationBuilder.CreateIndex(
                name: "IX_KidsPreCheckins_TenantId_CheckinId",
                table: "KidsPreCheckins",
                columns: new[] { "TenantId", "CheckinId" });

            migrationBuilder.AddForeignKey(
                name: "FK_KidsPreCheckins_KidsCheckins_CheckinId",
                table: "KidsPreCheckins",
                column: "CheckinId",
                principalTable: "KidsCheckins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KidsPreCheckins_KidsCheckins_CheckinId",
                table: "KidsPreCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsPreCheckins_CheckinId",
                table: "KidsPreCheckins");

            migrationBuilder.DropIndex(
                name: "IX_KidsPreCheckins_TenantId_CheckinId",
                table: "KidsPreCheckins");

            migrationBuilder.DropColumn(
                name: "CheckinId",
                table: "KidsPreCheckins");
        }
    }
}
