using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration baseline vazia - o banco PostgreSQL já foi criado via script SQL
            // Esta migration serve apenas como ponto de referência para o EF Core
            // Todas as tabelas já existem no banco de produção
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migration baseline vazia - não há nada para reverter
        }
    }
}
