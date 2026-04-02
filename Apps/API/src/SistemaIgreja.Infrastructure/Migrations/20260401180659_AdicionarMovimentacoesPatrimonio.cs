using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMovimentacoesPatrimonio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatrimonioMovimentacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatrimonioItemId = table.Column<int>(type: "integer", nullable: false),
                    TipoMovimentacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Origem = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Destino = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ResponsavelOrigem = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ResponsavelDestino = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatrimonioMovimentacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatrimonioMovimentacoes_PatrimonioItens_PatrimonioItemId",
                        column: x => x.PatrimonioItemId,
                        principalTable: "PatrimonioItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatrimonioMovimentacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioMovimentacoes_PatrimonioItemId",
                table: "PatrimonioMovimentacoes",
                column: "PatrimonioItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioMovimentacoes_UsuarioId",
                table: "PatrimonioMovimentacoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatrimonioMovimentacoes");
        }
    }
}
