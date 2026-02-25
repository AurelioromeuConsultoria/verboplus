using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirAutoIncrementoPerfisAcessoPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar se a coluna Id já tem IDENTITY configurado
            // Se não tiver, configurar auto-incremento usando GENERATED ALWAYS AS IDENTITY
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM pg_attribute 
                        WHERE attrelid = '""PerfisAcessoPermissoes""'::regclass 
                        AND attname = 'Id' 
                        AND attidentity != ''
                    ) THEN
                        CREATE SEQUENCE IF NOT EXISTS ""PerfisAcessoPermissoes_Id_seq"";
                        
                        PERFORM setval('""PerfisAcessoPermissoes_Id_seq""', 
                            COALESCE((SELECT MAX(""Id"") FROM ""PerfisAcessoPermissoes""), 1000) + 1, 
                            false);
                        
                        BEGIN
                            ALTER TABLE ""PerfisAcessoPermissoes"" ALTER COLUMN ""Id"" DROP DEFAULT;
                        EXCEPTION
                            WHEN OTHERS THEN NULL;
                        END;
                        
                        ALTER TABLE ""PerfisAcessoPermissoes"" 
                            ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY;
                        
                        ALTER SEQUENCE ""PerfisAcessoPermissoes_Id_seq"" OWNED BY ""PerfisAcessoPermissoes"".""Id"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverter para coluna sem IDENTITY (não recomendado, mas necessário para rollback)
            migrationBuilder.Sql(@"
                ALTER TABLE ""PerfisAcessoPermissoes"" 
                    ALTER COLUMN ""Id"" DROP IDENTITY IF EXISTS;
            ");
        }
    }
}
