using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarConsentimentoRegistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsentimentosRegistros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    VersaoDocumento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AceitoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IpOrigem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origem = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ConcedidoPorPessoaId = table.Column<int>(type: "integer", nullable: true),
                    RevogadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimentosRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentimentosRegistros_Pessoas_ConcedidoPorPessoaId",
                        column: x => x.ConcedidoPorPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentimentosRegistros_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentimentosRegistros_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentosRegistros_ConcedidoPorPessoaId",
                table: "ConsentimentosRegistros",
                column: "ConcedidoPorPessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentosRegistros_PessoaId",
                table: "ConsentimentosRegistros",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentosRegistros_TenantId_PessoaId_Tipo",
                table: "ConsentimentosRegistros",
                columns: new[] { "TenantId", "PessoaId", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentimentosRegistros");
        }
    }
}
