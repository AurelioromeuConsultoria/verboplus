using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopeToFinancialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatrimonioItens_Codigo",
                table: "PatrimonioItens");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasPatrimonio_Nome",
                table: "CategoriasPatrimonio");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Receitas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Projetos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PatrimonioMovimentacoes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PatrimonioItens",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Fornecedores",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Despesas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ContasBancarias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CentrosCustos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CategoriasReceitas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CategoriasPatrimonio",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CategoriasDespesas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"UPDATE ""CategoriasDespesas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""CategoriasReceitas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""ContasBancarias"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""CentrosCustos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Projetos"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Fornecedores"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Despesas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""Receitas"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""CategoriasPatrimonio"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""PatrimonioItens"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");
            migrationBuilder.Sql(@"UPDATE ""PatrimonioMovimentacoes"" SET ""TenantId"" = 1 WHERE ""TenantId"" = 0;");

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 8,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 9,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 10,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 11,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 12,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 13,
                column: "TenantId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Receitas_TenantId_DataRecebimento_Status",
                table: "Receitas",
                columns: new[] { "TenantId", "DataRecebimento", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_TenantId_Nome",
                table: "Projetos",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioMovimentacoes_TenantId_PatrimonioItemId_DataMovim~",
                table: "PatrimonioMovimentacoes",
                columns: new[] { "TenantId", "PatrimonioItemId", "DataMovimentacao" });

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_TenantId_Codigo",
                table: "PatrimonioItens",
                columns: new[] { "TenantId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_TenantId_Nome",
                table: "Fornecedores",
                columns: new[] { "TenantId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_TenantId_DataVencimento_Status",
                table: "Despesas",
                columns: new[] { "TenantId", "DataVencimento", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContasBancarias_TenantId_Nome",
                table: "ContasBancarias",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCustos_TenantId_Nome",
                table: "CentrosCustos",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasReceitas_TenantId_Nome",
                table: "CategoriasReceitas",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasPatrimonio_TenantId_Nome",
                table: "CategoriasPatrimonio",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasDespesas_TenantId_Nome",
                table: "CategoriasDespesas",
                columns: new[] { "TenantId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasDespesas_Tenants_TenantId",
                table: "CategoriasDespesas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasPatrimonio_Tenants_TenantId",
                table: "CategoriasPatrimonio",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasReceitas_Tenants_TenantId",
                table: "CategoriasReceitas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CentrosCustos_Tenants_TenantId",
                table: "CentrosCustos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContasBancarias_Tenants_TenantId",
                table: "ContasBancarias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Tenants_TenantId",
                table: "Despesas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fornecedores_Tenants_TenantId",
                table: "Fornecedores",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PatrimonioItens_Tenants_TenantId",
                table: "PatrimonioItens",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PatrimonioMovimentacoes_Tenants_TenantId",
                table: "PatrimonioMovimentacoes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projetos_Tenants_TenantId",
                table: "Projetos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receitas_Tenants_TenantId",
                table: "Receitas",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasDespesas_Tenants_TenantId",
                table: "CategoriasDespesas");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasPatrimonio_Tenants_TenantId",
                table: "CategoriasPatrimonio");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasReceitas_Tenants_TenantId",
                table: "CategoriasReceitas");

            migrationBuilder.DropForeignKey(
                name: "FK_CentrosCustos_Tenants_TenantId",
                table: "CentrosCustos");

            migrationBuilder.DropForeignKey(
                name: "FK_ContasBancarias_Tenants_TenantId",
                table: "ContasBancarias");

            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Tenants_TenantId",
                table: "Despesas");

            migrationBuilder.DropForeignKey(
                name: "FK_Fornecedores_Tenants_TenantId",
                table: "Fornecedores");

            migrationBuilder.DropForeignKey(
                name: "FK_PatrimonioItens_Tenants_TenantId",
                table: "PatrimonioItens");

            migrationBuilder.DropForeignKey(
                name: "FK_PatrimonioMovimentacoes_Tenants_TenantId",
                table: "PatrimonioMovimentacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Projetos_Tenants_TenantId",
                table: "Projetos");

            migrationBuilder.DropForeignKey(
                name: "FK_Receitas_Tenants_TenantId",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Receitas_TenantId_DataRecebimento_Status",
                table: "Receitas");

            migrationBuilder.DropIndex(
                name: "IX_Projetos_TenantId_Nome",
                table: "Projetos");

            migrationBuilder.DropIndex(
                name: "IX_PatrimonioMovimentacoes_TenantId_PatrimonioItemId_DataMovim~",
                table: "PatrimonioMovimentacoes");

            migrationBuilder.DropIndex(
                name: "IX_PatrimonioItens_TenantId_Codigo",
                table: "PatrimonioItens");

            migrationBuilder.DropIndex(
                name: "IX_Fornecedores_TenantId_Nome",
                table: "Fornecedores");

            migrationBuilder.DropIndex(
                name: "IX_Despesas_TenantId_DataVencimento_Status",
                table: "Despesas");

            migrationBuilder.DropIndex(
                name: "IX_ContasBancarias_TenantId_Nome",
                table: "ContasBancarias");

            migrationBuilder.DropIndex(
                name: "IX_CentrosCustos_TenantId_Nome",
                table: "CentrosCustos");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasReceitas_TenantId_Nome",
                table: "CategoriasReceitas");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasPatrimonio_TenantId_Nome",
                table: "CategoriasPatrimonio");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasDespesas_TenantId_Nome",
                table: "CategoriasDespesas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Receitas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Projetos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PatrimonioMovimentacoes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PatrimonioItens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Fornecedores");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ContasBancarias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CentrosCustos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoriasReceitas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoriasPatrimonio");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoriasDespesas");

            migrationBuilder.CreateIndex(
                name: "IX_PatrimonioItens_Codigo",
                table: "PatrimonioItens",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasPatrimonio_Nome",
                table: "CategoriasPatrimonio",
                column: "Nome",
                unique: true);
        }
    }
}
