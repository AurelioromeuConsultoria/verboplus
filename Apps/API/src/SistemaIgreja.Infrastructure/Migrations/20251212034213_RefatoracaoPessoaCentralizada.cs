using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoPessoaCentralizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Criar tabela Pessoas
            migrationBuilder.CreateTable(
                name: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoPessoa = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                });

            // 2. Criar tabela PessoasPerfis
            migrationBuilder.CreateTable(
                name: "PessoasPerfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PessoaId = table.Column<int>(type: "int", nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PessoasPerfis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PessoasPerfis_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 3. Adicionar coluna PessoaId temporária (nullable) em todas as tabelas
            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "Voluntarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "Visitantes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            // 4. Migrar dados: Visitantes -> Pessoas
            migrationBuilder.Sql(@"
                INSERT INTO Pessoas (Nome, Email, Telefone, WhatsApp, TipoPessoa, Ativo, DataCriacao)
                SELECT Nome, Email, Telefone, NULL, 1, 1, DataCadastro
                FROM Visitantes;

                UPDATE Visitantes
                SET PessoaId = Pessoas.Id
                FROM Visitantes
                INNER JOIN Pessoas ON Pessoas.Nome = Visitantes.Nome 
                    AND (Pessoas.Email = Visitantes.Email OR (Pessoas.Email IS NULL AND Visitantes.Email IS NULL))
                    AND Pessoas.DataCriacao = Visitantes.DataCadastro;

                INSERT INTO PessoasPerfis (PessoaId, Perfil, DataInicio)
                SELECT PessoaId, 1, DataCadastro
                FROM Visitantes
                WHERE PessoaId IS NOT NULL;
            ");

            // 5. Migrar dados: Voluntarios -> Pessoas (evitar duplicatas por email)
            migrationBuilder.Sql(@"
                INSERT INTO Pessoas (Nome, Email, Telefone, WhatsApp, TipoPessoa, Ativo, DataCriacao)
                SELECT v.Nome, v.Email, NULL, v.WhatsApp, 1, 1, v.DataCadastro
                FROM Voluntarios v
                WHERE NOT EXISTS (
                    SELECT 1 FROM Pessoas p 
                    WHERE p.Email = v.Email AND p.Email IS NOT NULL
                );

                UPDATE Voluntarios
                SET PessoaId = Pessoas.Id
                FROM Voluntarios
                INNER JOIN Pessoas ON (
                    (Pessoas.Email = Voluntarios.Email AND Pessoas.Email IS NOT NULL)
                    OR (Pessoas.Nome = Voluntarios.Nome AND Pessoas.Email IS NULL AND Voluntarios.Email IS NULL)
                )
                WHERE Voluntarios.PessoaId IS NULL;

                INSERT INTO PessoasPerfis (PessoaId, Perfil, DataInicio)
                SELECT PessoaId, 3, DataCadastro
                FROM Voluntarios
                WHERE PessoaId IS NOT NULL;
            ");

            // 6. Migrar dados: Usuarios -> Pessoas (evitar duplicatas por email)
            migrationBuilder.Sql(@"
                INSERT INTO Pessoas (Nome, Email, Telefone, WhatsApp, TipoPessoa, Ativo, DataCriacao)
                SELECT u.Nome, u.Email, NULL, NULL, 1, 1, u.DataCriacao
                FROM Usuarios u
                WHERE NOT EXISTS (
                    SELECT 1 FROM Pessoas p 
                    WHERE p.Email = u.Email AND p.Email IS NOT NULL
                );

                UPDATE Usuarios
                SET PessoaId = Pessoas.Id
                FROM Usuarios
                INNER JOIN Pessoas ON Pessoas.Email = Usuarios.Email
                WHERE Usuarios.PessoaId IS NULL;

                INSERT INTO PessoasPerfis (PessoaId, Perfil, DataInicio)
                SELECT PessoaId, 
                    CASE 
                        WHEN TipoUsuario = 1 THEN 6  -- Admin
                        ELSE 2  -- Membro
                    END,
                    DataCriacao
                FROM Usuarios
                WHERE PessoaId IS NOT NULL;
            ");

            // 7. Tornar PessoaId NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "PessoaId",
                table: "Voluntarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PessoaId",
                table: "Visitantes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PessoaId",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 8. Renomear Nome para EmailLogin em Usuarios
            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Usuarios",
                newName: "EmailLogin");

            // 9. Remover índice antigo de Email em Usuarios
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            // 10. Remover colunas antigas
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Voluntarios");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Voluntarios");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Voluntarios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Visitantes");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Visitantes");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Visitantes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            // 11. Criar índices
            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_PessoaId",
                table: "Voluntarios",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitantes_PessoaId",
                table: "Visitantes",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios",
                column: "EmailLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PessoaId",
                table: "Usuarios",
                column: "PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Email",
                table: "Pessoas",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PessoasPerfis_PessoaId",
                table: "PessoasPerfis",
                column: "PessoaId");

            // 12. Adicionar Foreign Keys
            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Pessoas_PessoaId",
                table: "Usuarios",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitantes_Pessoas_PessoaId",
                table: "Visitantes",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voluntarios_Pessoas_PessoaId",
                table: "Voluntarios",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Pessoas_PessoaId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitantes_Pessoas_PessoaId",
                table: "Visitantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Voluntarios_Pessoas_PessoaId",
                table: "Voluntarios");

            migrationBuilder.DropTable(
                name: "PessoasPerfis");

            migrationBuilder.DropTable(
                name: "Pessoas");

            migrationBuilder.DropIndex(
                name: "IX_Voluntarios_PessoaId",
                table: "Voluntarios");

            migrationBuilder.DropIndex(
                name: "IX_Visitantes_PessoaId",
                table: "Visitantes");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmailLogin",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PessoaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "Voluntarios");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "Visitantes");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "EmailLogin",
                table: "Usuarios",
                newName: "Nome");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Voluntarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Voluntarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Voluntarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Visitantes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Visitantes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Visitantes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }
    }
}
