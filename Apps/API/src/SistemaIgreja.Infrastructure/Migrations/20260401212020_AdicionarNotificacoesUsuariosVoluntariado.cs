using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNotificacoesUsuariosVoluntariado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificacoesUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataLeitura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacoesUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacoesUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacoesUsuarios_UsuarioId_DataLeitura_DataCriacao",
                table: "NotificacoesUsuarios",
                columns: new[] { "UsuarioId", "DataLeitura", "DataCriacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacoesUsuarios");
        }
    }
}
