using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBillingAssinaturas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosWebhookBilling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GatewayEventId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Evento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    GatewayPaymentId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    GatewaySubscriptionId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    Processado = table.Column<bool>(type: "boolean", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecebidoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosWebhookBilling", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecoMensal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecoAnual = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxUsuarios = table.Column<int>(type: "integer", nullable: true),
                    MaxMembros = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PlanoId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Ciclo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MetodoPagamento = table.Column<int>(type: "integer", nullable: true),
                    TrialFim = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VigenciaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProximaCobranca = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    InadimplenteDesde = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SuspensaEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CanceladaEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GatewayCustomerId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    GatewaySubscriptionId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Planos_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Planos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Faturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    AssinaturaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Vencimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PagaEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GatewayPaymentId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LinkPagamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PixCopiaECola = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturas_Assinaturas_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faturas_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Planos",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Descricao", "MaxMembros", "MaxUsuarios", "Nome", "Ordem", "PrecoAnual", "PrecoMensal", "Slug" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Para igrejas começando a se organizar.", null, null, "Essencial", 1, 499.00m, 49.90m, "essencial" },
                    { 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Para igrejas em crescimento que precisam de gestão completa.", null, null, "Organização", 2, 999.00m, 99.90m, "organizacao" },
                    { 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Para igrejas com múltiplos ministérios e alto volume.", null, null, "Crescimento", 3, 1999.00m, 199.90m, "crescimento" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_GatewaySubscriptionId",
                table: "Assinaturas",
                column: "GatewaySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_PlanoId",
                table: "Assinaturas",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_TenantId",
                table: "Assinaturas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosWebhookBilling_GatewayEventId",
                table: "EventosWebhookBilling",
                column: "GatewayEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_AssinaturaId",
                table: "Faturas",
                column: "AssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_GatewayPaymentId",
                table: "Faturas",
                column: "GatewayPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_TenantId_Status",
                table: "Faturas",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Planos_Slug",
                table: "Planos",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosWebhookBilling");

            migrationBuilder.DropTable(
                name: "Faturas");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Planos");
        }
    }
}
