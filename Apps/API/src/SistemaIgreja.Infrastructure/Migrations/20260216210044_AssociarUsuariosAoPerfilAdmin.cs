using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssociarUsuariosAoPerfilAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Associar todos os usuários existentes ao perfil Administrador (ID = 1)
            migrationBuilder.Sql(@"
                UPDATE [Usuarios]
                SET [PerfilAcessoId] = 1
                WHERE [PerfilAcessoId] IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
