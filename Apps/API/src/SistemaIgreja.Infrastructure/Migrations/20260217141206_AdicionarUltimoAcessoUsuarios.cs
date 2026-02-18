using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUltimoAcessoUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna UltimoAcesso na tabela Usuarios (PostgreSQL)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Usuarios""
                ADD COLUMN IF NOT EXISTS ""UltimoAcesso"" TIMESTAMP NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Usuarios""
                DROP COLUMN IF EXISTS ""UltimoAcesso"";
            ");
        }
    }
}
