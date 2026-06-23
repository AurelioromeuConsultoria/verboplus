using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EscalaItem_AddPessoaId_VoluntarioIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscalasItens_Voluntarios_VoluntarioId",
                table: "EscalasItens");

            migrationBuilder.AlterColumn<int>(
                name: "VoluntarioId",
                table: "EscalasItens",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "EscalasItens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: popula PessoaId a partir do Voluntario vinculado
            migrationBuilder.Sql(@"
                UPDATE ""EscalasItens"" ei
                SET ""PessoaId"" = v.""PessoaId""
                FROM ""Voluntarios"" v
                WHERE ei.""VoluntarioId"" = v.""Id"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_PessoaId",
                table: "EscalasItens",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_PessoaId",
                table: "EscalasItens",
                columns: new[] { "TenantId", "EscalaId", "PessoaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasItens_Pessoas_PessoaId",
                table: "EscalasItens",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasItens_Voluntarios_VoluntarioId",
                table: "EscalasItens",
                column: "VoluntarioId",
                principalTable: "Voluntarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscalasItens_Pessoas_PessoaId",
                table: "EscalasItens");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalasItens_Voluntarios_VoluntarioId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_PessoaId",
                table: "EscalasItens");

            migrationBuilder.DropIndex(
                name: "IX_EscalasItens_TenantId_EscalaId_PessoaId",
                table: "EscalasItens");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "EscalasItens");

            migrationBuilder.AlterColumn<int>(
                name: "VoluntarioId",
                table: "EscalasItens",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalasItens_Voluntarios_VoluntarioId",
                table: "EscalasItens",
                column: "VoluntarioId",
                principalTable: "Voluntarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
