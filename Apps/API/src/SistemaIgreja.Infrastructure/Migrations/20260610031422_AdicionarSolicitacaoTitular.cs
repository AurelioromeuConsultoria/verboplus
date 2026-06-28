using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSolicitacaoTitular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesTitular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PessoaId = table.Column<int>(type: "integer", nullable: true),
                    NomeSolicitante = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContatoSolicitante = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Canal = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SolicitadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PrazoLimite = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AtendidoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AtendidoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    ResultadoObservacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesTitular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTitular_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitacoesTitular_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTitular_PessoaId",
                table: "SolicitacoesTitular",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTitular_TenantId_PrazoLimite",
                table: "SolicitacoesTitular",
                columns: new[] { "TenantId", "PrazoLimite" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesTitular_TenantId_Status",
                table: "SolicitacoesTitular",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesTitular");
        }
    }
}
