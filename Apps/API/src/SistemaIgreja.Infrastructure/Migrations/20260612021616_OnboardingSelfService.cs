using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OnboardingSelfService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId_EmailLogin",
                table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "VerificacoesEmail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConfirmadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacoesEmail", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios",
                column: "EmailLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId",
                table: "Usuarios",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificacoesEmail_Token",
                table: "VerificacoesEmail",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificacoesEmail");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId",
                table: "Usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_EmailLogin",
                table: "Usuarios",
                columns: new[] { "TenantId", "EmailLogin" },
                unique: true);
        }
    }
}
