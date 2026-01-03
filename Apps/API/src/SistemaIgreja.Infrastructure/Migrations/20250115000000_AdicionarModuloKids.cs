using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModuloKids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Criar tabela CriancasDetalhes
            migrationBuilder.CreateTable(
                name: "CriancasDetalhes",
                columns: table => new
                {
                    PessoaId = table.Column<int>(type: "int", nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestricoesAlimentares = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SalaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriancasDetalhes", x => x.PessoaId);
                    table.ForeignKey(
                        name: "FK_CriancasDetalhes_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Criar tabela ResponsaveisCriancas
            migrationBuilder.CreateTable(
                name: "ResponsaveisCriancas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriancaPessoaId = table.Column<int>(type: "int", nullable: false),
                    ResponsavelPessoaId = table.Column<int>(type: "int", nullable: false),
                    PodeRetirar = table.Column<bool>(type: "bit", nullable: false),
                    Parentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsaveisCriancas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsaveisCriancas_Pessoas_CriancaPessoaId",
                        column: x => x.CriancaPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsaveisCriancas_Pessoas_ResponsavelPessoaId",
                        column: x => x.ResponsavelPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Criar tabela KidsCheckins
            migrationBuilder.CreateTable(
                name: "KidsCheckins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriancaPessoaId = table.Column<int>(type: "int", nullable: false),
                    CheckinTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckinByPessoaId = table.Column<int>(type: "int", nullable: true),
                    CheckoutByPessoaId = table.Column<int>(type: "int", nullable: true),
                    Metodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoSessao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsCheckins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsCheckins_Pessoas_CriancaPessoaId",
                        column: x => x.CriancaPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KidsCheckins_Pessoas_CheckinByPessoaId",
                        column: x => x.CheckinByPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KidsCheckins_Pessoas_CheckoutByPessoaId",
                        column: x => x.CheckoutByPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Criar tabela KidsNotificacoes
            migrationBuilder.CreateTable(
                name: "KidsNotificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriancaPessoaId = table.Column<int>(type: "int", nullable: false),
                    ResponsavelPessoaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EnviadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KidsNotificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KidsNotificacoes_Pessoas_CriancaPessoaId",
                        column: x => x.CriancaPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KidsNotificacoes_Pessoas_ResponsavelPessoaId",
                        column: x => x.ResponsavelPessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Criar índices
            migrationBuilder.CreateIndex(
                name: "IX_ResponsaveisCriancas_CriancaPessoaId_ResponsavelPessoaId",
                table: "ResponsaveisCriancas",
                columns: new[] { "CriancaPessoaId", "ResponsavelPessoaId" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_CodigoSessao",
                table: "KidsCheckins",
                column: "CodigoSessao");

            migrationBuilder.CreateIndex(
                name: "IX_KidsCheckins_CriancaPessoaId_Status",
                table: "KidsCheckins",
                columns: new[] { "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_CriancaPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "CriancaPessoaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KidsNotificacoes_ResponsavelPessoaId_Status",
                table: "KidsNotificacoes",
                columns: new[] { "ResponsavelPessoaId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KidsNotificacoes");

            migrationBuilder.DropTable(
                name: "KidsCheckins");

            migrationBuilder.DropTable(
                name: "ResponsaveisCriancas");

            migrationBuilder.DropTable(
                name: "CriancasDetalhes");
        }
    }
}


