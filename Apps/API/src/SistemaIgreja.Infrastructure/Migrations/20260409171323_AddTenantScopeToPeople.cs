using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToPeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pessoas_Email",
                table: "Pessoas");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Visitantes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PessoasPerfis",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE \"Pessoas\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"PessoasPerfis\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"Visitantes\" SET \"TenantId\" = 1 WHERE \"TenantId\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Visitantes_TenantId",
                table: "Visitantes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PessoasPerfis_TenantId",
                table: "PessoasPerfis",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_TenantId_Email",
                table: "Pessoas",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoas_Tenants_TenantId",
                table: "Pessoas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasPerfis_Tenants_TenantId",
                table: "PessoasPerfis",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitantes_Tenants_TenantId",
                table: "Visitantes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoas_Tenants_TenantId",
                table: "Pessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasPerfis_Tenants_TenantId",
                table: "PessoasPerfis");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitantes_Tenants_TenantId",
                table: "Visitantes");

            migrationBuilder.DropIndex(
                name: "IX_Visitantes_TenantId",
                table: "Visitantes");

            migrationBuilder.DropIndex(
                name: "IX_PessoasPerfis_TenantId",
                table: "PessoasPerfis");

            migrationBuilder.DropIndex(
                name: "IX_Pessoas_TenantId_Email",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Visitantes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PessoasPerfis");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Pessoas");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Email",
                table: "Pessoas",
                column: "Email",
                unique: true);
        }
    }
}
