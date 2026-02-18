using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPerfisAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerfilAcessoId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerfisAcesso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisAcesso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerfisAcessoPermissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilAcessoId = table.Column<int>(type: "int", nullable: false),
                    Recurso = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PodeVer = table.Column<bool>(type: "bit", nullable: false),
                    PodeEditar = table.Column<bool>(type: "bit", nullable: false),
                    PodeExcluir = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisAcessoPermissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfisAcessoPermissoes_PerfisAcesso_PerfilAcessoId",
                        column: x => x.PerfilAcessoId,
                        principalTable: "PerfisAcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PerfisAcesso",
                columns: new[] { "Id", "DataCriacao", "Descricao", "Nome" },
                values: new object[] { 1, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Acesso total ao sistema", "Administrador" });

            migrationBuilder.InsertData(
                table: "PerfisAcessoPermissoes",
                columns: new[] { "Id", "PerfilAcessoId", "PodeEditar", "PodeExcluir", "PodeVer", "Recurso" },
                values: new object[,]
                {
                    { 1000, 1, true, true, true, "dashboard" },
                    { 1001, 1, true, true, true, "usuarios" },
                    { 1002, 1, true, true, true, "perfis-acesso" },
                    { 1003, 1, true, true, true, "pessoas" },
                    { 1004, 1, true, true, true, "perfis" },
                    { 1005, 1, true, true, true, "visitantes" },
                    { 1006, 1, true, true, true, "configuracoes-mensagens" },
                    { 1007, 1, true, true, true, "mensagens-agendadas" },
                    { 1008, 1, true, true, true, "equipes" },
                    { 1009, 1, true, true, true, "cargos" },
                    { 1010, 1, true, true, true, "voluntarios" },
                    { 1011, 1, true, true, true, "eventos" },
                    { 1012, 1, true, true, true, "inscricoes-eventos" },
                    { 1013, 1, true, true, true, "portal" },
                    { 1014, 1, true, true, true, "noticias" },
                    { 1015, 1, true, true, true, "categorias-noticias" },
                    { 1016, 1, true, true, true, "contatos" },
                    { 1017, 1, true, true, true, "destaques-site" },
                    { 1018, 1, true, true, true, "categorias-midias" },
                    { 1019, 1, true, true, true, "galerias-fotos" },
                    { 1020, 1, true, true, true, "enquetes" },
                    { 1021, 1, true, true, true, "kids" },
                    { 1022, 1, true, true, true, "hub" },
                    { 1023, 1, true, true, true, "financeiro" },
                    { 1024, 1, true, true, true, "fornecedores" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilAcessoId",
                table: "Usuarios",
                column: "PerfilAcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfisAcessoPermissoes_PerfilAcessoId",
                table: "PerfisAcessoPermissoes",
                column: "PerfilAcessoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_PerfisAcesso_PerfilAcessoId",
                table: "Usuarios",
                column: "PerfilAcessoId",
                principalTable: "PerfisAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_PerfisAcesso_PerfilAcessoId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "PerfisAcessoPermissoes");

            migrationBuilder.DropTable(
                name: "PerfisAcesso");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PerfilAcessoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PerfilAcessoId",
                table: "Usuarios");
        }
    }
}
