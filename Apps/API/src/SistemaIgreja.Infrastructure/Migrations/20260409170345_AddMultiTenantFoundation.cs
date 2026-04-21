using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PerfisAcessoPermissoes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PerfisAcesso",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ConfiguracoesPortal",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ConfiguracoesMensagens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ConfiguracoesCampanhaAniversario",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDomains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantDomains_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ConfiguracoesMensagens",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ConfiguracoesMensagens",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ConfiguracoesPortal",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1000,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1001,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1002,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1003,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1004,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1005,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1006,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1007,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1008,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1009,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1010,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1011,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1012,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1013,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1014,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1015,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1016,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1017,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1018,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1019,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1020,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1021,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1022,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1023,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PerfisAcessoPermissoes",
                keyColumn: "Id",
                keyValue: 1024,
                column: "TenantId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Nome", "Slug" },
                values: new object[] { 1, true, new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Mang Guarulhos", "mang-guarulhos" });

            migrationBuilder.Sql("UPDATE \"Usuarios\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"PerfisAcesso\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"PerfisAcessoPermissoes\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"ConfiguracoesPortal\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"ConfiguracoesMensagens\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"ConfiguracoesCampanhaAniversario\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");
            migrationBuilder.Sql("UPDATE \"AuditLogs\" SET \"TenantId\" = 1 WHERE \"TenantId\" IS NULL OR \"TenantId\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_EmailLogin",
                table: "Usuarios",
                columns: new[] { "TenantId", "EmailLogin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfisAcessoPermissoes_TenantId_PerfilAcessoId_Recurso",
                table: "PerfisAcessoPermissoes",
                columns: new[] { "TenantId", "PerfilAcessoId", "Recurso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfisAcesso_TenantId_Nome",
                table: "PerfisAcesso",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesPortal_TenantId",
                table: "ConfiguracoesPortal",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesMensagens_TenantId_Nome",
                table: "ConfiguracoesMensagens",
                columns: new[] { "TenantId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesCampanhaAniversario_TenantId",
                table: "ConfiguracoesCampanhaAniversario",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "TenantId", "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantDomains_Domain",
                table: "TenantDomains",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantDomains_TenantId",
                table: "TenantDomains",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                table: "AuditLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracoesCampanhaAniversario_Tenants_TenantId",
                table: "ConfiguracoesCampanhaAniversario",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracoesMensagens_Tenants_TenantId",
                table: "ConfiguracoesMensagens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracoesPortal_Tenants_TenantId",
                table: "ConfiguracoesPortal",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerfisAcesso_Tenants_TenantId",
                table: "PerfisAcesso",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerfisAcessoPermissoes_Tenants_TenantId",
                table: "PerfisAcessoPermissoes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracoesCampanhaAniversario_Tenants_TenantId",
                table: "ConfiguracoesCampanhaAniversario");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracoesMensagens_Tenants_TenantId",
                table: "ConfiguracoesMensagens");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracoesPortal_Tenants_TenantId",
                table: "ConfiguracoesPortal");

            migrationBuilder.DropForeignKey(
                name: "FK_PerfisAcesso_Tenants_TenantId",
                table: "PerfisAcesso");

            migrationBuilder.DropForeignKey(
                name: "FK_PerfisAcessoPermissoes_Tenants_TenantId",
                table: "PerfisAcessoPermissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "TenantDomains");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId_EmailLogin",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_PerfisAcessoPermissoes_TenantId_PerfilAcessoId_Recurso",
                table: "PerfisAcessoPermissoes");

            migrationBuilder.DropIndex(
                name: "IX_PerfisAcesso_TenantId_Nome",
                table: "PerfisAcesso");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracoesPortal_TenantId",
                table: "ConfiguracoesPortal");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracoesMensagens_TenantId_Nome",
                table: "ConfiguracoesMensagens");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracoesCampanhaAniversario_TenantId",
                table: "ConfiguracoesCampanhaAniversario");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId_EntityName_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PerfisAcessoPermissoes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PerfisAcesso");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ConfiguracoesPortal");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ConfiguracoesMensagens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ConfiguracoesCampanhaAniversario");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios",
                column: "EmailLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });
        }
    }
}
