using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Receita_AddPessoaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "Receitas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_PessoaId",
                table: "Receitas",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_TenantId_PessoaId_DataRecebimento",
                table: "Receitas",
                columns: new[] { "TenantId", "PessoaId", "DataRecebimento" });

            migrationBuilder.AddForeignKey(
                name: "FK_Receitas_Pessoas_PessoaId",
                table: "Receitas",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receitas_Pessoas_PessoaId",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_PessoaId",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_TenantId_PessoaId_DataRecebimento",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "Receitas");
        }
    }
}
