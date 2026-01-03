-- Script SQL para aplicar a migration do módulo Kids
-- Execute este script no SQL Server Management Studio ou Azure Data Studio
-- Conectado ao banco: SistemaIgrejaDb

USE SistemaIgrejaDb;
GO

-- 1. Criar tabela CriancasDetalhes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CriancasDetalhes')
BEGIN
    CREATE TABLE [CriancasDetalhes] (
        [PessoaId] int NOT NULL,
        [Alergias] nvarchar(500) NULL,
        [RestricoesAlimentares] nvarchar(500) NULL,
        [Observacoes] nvarchar(1000) NULL,
        [SalaId] nvarchar(50) NULL,
        [DataCadastro] datetime2 NOT NULL,
        CONSTRAINT [PK_CriancasDetalhes] PRIMARY KEY ([PessoaId]),
        CONSTRAINT [FK_CriancasDetalhes_Pessoas_PessoaId] FOREIGN KEY ([PessoaId]) REFERENCES [Pessoas] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabela CriancasDetalhes criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela CriancasDetalhes já existe.';
END
GO

-- 2. Criar tabela ResponsaveisCriancas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResponsaveisCriancas')
BEGIN
    CREATE TABLE [ResponsaveisCriancas] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CriancaPessoaId] int NOT NULL,
        [ResponsavelPessoaId] int NOT NULL,
        [PodeRetirar] bit NOT NULL,
        [Parentesco] nvarchar(50) NULL,
        [Ativo] bit NOT NULL,
        [DataCadastro] datetime2 NOT NULL,
        CONSTRAINT [PK_ResponsaveisCriancas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResponsaveisCriancas_Pessoas_CriancaPessoaId] FOREIGN KEY ([CriancaPessoaId]) REFERENCES [Pessoas] ([Id]),
        CONSTRAINT [FK_ResponsaveisCriancas_Pessoas_ResponsavelPessoaId] FOREIGN KEY ([ResponsavelPessoaId]) REFERENCES [Pessoas] ([Id])
    );
    
    CREATE INDEX [IX_ResponsaveisCriancas_CriancaPessoaId_ResponsavelPessoaId] 
        ON [ResponsaveisCriancas] ([CriancaPessoaId], [ResponsavelPessoaId]);
    
    PRINT 'Tabela ResponsaveisCriancas criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela ResponsaveisCriancas já existe.';
END
GO

-- 3. Criar tabela KidsCheckins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KidsCheckins')
BEGIN
    CREATE TABLE [KidsCheckins] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CriancaPessoaId] int NOT NULL,
        [CheckinTime] datetime2 NOT NULL,
        [CheckoutTime] datetime2 NULL,
        [CheckinByPessoaId] int NULL,
        [CheckoutByPessoaId] int NULL,
        [Metodo] nvarchar(20) NOT NULL,
        [CodigoSessao] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Observacoes] nvarchar(500) NULL,
        CONSTRAINT [PK_KidsCheckins] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KidsCheckins_Pessoas_CriancaPessoaId] FOREIGN KEY ([CriancaPessoaId]) REFERENCES [Pessoas] ([Id]),
        CONSTRAINT [FK_KidsCheckins_Pessoas_CheckinByPessoaId] FOREIGN KEY ([CheckinByPessoaId]) REFERENCES [Pessoas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_KidsCheckins_Pessoas_CheckoutByPessoaId] FOREIGN KEY ([CheckoutByPessoaId]) REFERENCES [Pessoas] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_KidsCheckins_CodigoSessao] ON [KidsCheckins] ([CodigoSessao]);
    CREATE INDEX [IX_KidsCheckins_CriancaPessoaId_Status] ON [KidsCheckins] ([CriancaPessoaId], [Status]);
    
    PRINT 'Tabela KidsCheckins criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela KidsCheckins já existe.';
END
GO

-- 4. Criar tabela KidsNotificacoes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KidsNotificacoes')
BEGIN
    CREATE TABLE [KidsNotificacoes] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CriancaPessoaId] int NOT NULL,
        [ResponsavelPessoaId] int NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [Mensagem] nvarchar(1000) NOT NULL,
        [EnviadoEm] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        [DataCriacao] datetime2 NOT NULL,
        CONSTRAINT [PK_KidsNotificacoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KidsNotificacoes_Pessoas_CriancaPessoaId] FOREIGN KEY ([CriancaPessoaId]) REFERENCES [Pessoas] ([Id]),
        CONSTRAINT [FK_KidsNotificacoes_Pessoas_ResponsavelPessoaId] FOREIGN KEY ([ResponsavelPessoaId]) REFERENCES [Pessoas] ([Id])
    );
    
    CREATE INDEX [IX_KidsNotificacoes_CriancaPessoaId_Status] ON [KidsNotificacoes] ([CriancaPessoaId], [Status]);
    CREATE INDEX [IX_KidsNotificacoes_ResponsavelPessoaId_Status] ON [KidsNotificacoes] ([ResponsavelPessoaId], [Status]);
    
    PRINT 'Tabela KidsNotificacoes criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela KidsNotificacoes já existe.';
END
GO

-- 5. Registrar a migration na tabela __EFMigrationsHistory
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250115000000_AdicionarModuloKids')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250115000000_AdicionarModuloKids', '8.0.0');
    PRINT 'Migration registrada na tabela __EFMigrationsHistory.';
END
ELSE
BEGIN
    PRINT 'Migration já estava registrada.';
END
GO

PRINT 'Script executado com sucesso! Todas as tabelas do módulo Kids foram criadas.';
GO


