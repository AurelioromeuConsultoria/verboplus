-- Corrigir a coluna Id da tabela PerfisAcessoPermissoes para usar auto-incremento
-- Este script altera a coluna Id para usar GENERATED ALWAYS AS IDENTITY (equivalente a SERIAL no PostgreSQL moderno)

-- Primeiro, criar uma sequência para a coluna Id
CREATE SEQUENCE IF NOT EXISTS "PerfisAcessoPermissoes_Id_seq";

-- Definir o valor inicial da sequência baseado no maior Id existente (ou 1000 se não houver registros)
SELECT setval('"PerfisAcessoPermissoes_Id_seq"', COALESCE((SELECT MAX("Id") FROM "PerfisAcessoPermissoes"), 1000), true);

-- Remover a constraint PRIMARY KEY temporariamente (se necessário)
-- ALTER TABLE "PerfisAcessoPermissoes" DROP CONSTRAINT IF EXISTS "PerfisAcessoPermissoes_pkey";

-- Alterar a coluna Id para usar GENERATED ALWAYS AS IDENTITY
-- Primeiro, remover o DEFAULT atual se existir
ALTER TABLE "PerfisAcessoPermissoes" ALTER COLUMN "Id" DROP DEFAULT;

-- Alterar para usar IDENTITY
ALTER TABLE "PerfisAcessoPermissoes" 
    ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (START WITH 1001);

-- Tornar a sequência propriedade da coluna Id (opcional, mas recomendado)
ALTER SEQUENCE "PerfisAcessoPermissoes_Id_seq" OWNED BY "PerfisAcessoPermissoes"."Id";
