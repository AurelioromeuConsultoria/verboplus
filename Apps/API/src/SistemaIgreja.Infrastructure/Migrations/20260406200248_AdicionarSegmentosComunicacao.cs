using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSegmentosComunicacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var agora = new DateTime(2026, 4, 6, 20, 2, 48, DateTimeKind.Utc);

            migrationBuilder.CreateTable(
                name: "ComunicacaoSegmentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PublicoAlvo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Padrao = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicacaoSegmentos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ComunicacaoSegmentos",
                columns: new[] { "Id", "Nome", "Descricao", "PublicoAlvo", "Ativo", "Padrao", "DataCriacao", "DataAtualizacao" },
                values: new object[,]
                {
                    { 1, "Visitantes", "Público prioritário do Connect", "visitantes", true, true, agora, null },
                    { 2, "Membros", "Pessoas com perfil de membro ativo", "membros", true, true, agora, null },
                    { 3, "Voluntários", "Pessoas vinculadas ao voluntariado", "voluntarios", true, true, agora, null },
                    { 4, "Responsáveis do Kids", "Responsáveis com vínculo ativo no Kids", "responsaveis-kids", true, true, agora, null },
                    { 5, "Pessoas", "Base ampla de pessoas ativas", "pessoas", true, true, agora, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ComunicacaoSegmentos",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5 });

            migrationBuilder.DropTable(
                name: "ComunicacaoSegmentos");
        }
    }
}
