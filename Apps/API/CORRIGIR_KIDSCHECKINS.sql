-- Script para corrigir a tabela KidsCheckins
-- Execute este script para corrigir o erro de múltiplos caminhos de cascade

USE SistemaIgrejaDb;
GO

-- Verificar se a tabela existe mas está sem as constraints corretas
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'KidsCheckins')
BEGIN
    -- Remover constraints antigas se existirem
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_KidsCheckins_Pessoas_CheckinByPessoaId')
    BEGIN
        ALTER TABLE [KidsCheckins] DROP CONSTRAINT [FK_KidsCheckins_Pessoas_CheckinByPessoaId];
        PRINT 'Constraint FK_KidsCheckins_Pessoas_CheckinByPessoaId removida.';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_KidsCheckins_Pessoas_CheckoutByPessoaId')
    BEGIN
        ALTER TABLE [KidsCheckins] DROP CONSTRAINT [FK_KidsCheckins_Pessoas_CheckoutByPessoaId];
        PRINT 'Constraint FK_KidsCheckins_Pessoas_CheckoutByPessoaId removida.';
    END
    
    -- Recriar constraints com ON DELETE NO ACTION
    ALTER TABLE [KidsCheckins]
    ADD CONSTRAINT [FK_KidsCheckins_Pessoas_CheckinByPessoaId] 
        FOREIGN KEY ([CheckinByPessoaId]) 
        REFERENCES [Pessoas] ([Id]) 
        ON DELETE NO ACTION;
    
    ALTER TABLE [KidsCheckins]
    ADD CONSTRAINT [FK_KidsCheckins_Pessoas_CheckoutByPessoaId] 
        FOREIGN KEY ([CheckoutByPessoaId]) 
        REFERENCES [Pessoas] ([Id]) 
        ON DELETE NO ACTION;
    
    PRINT 'Constraints recriadas com sucesso com ON DELETE NO ACTION.';
END
ELSE
BEGIN
    -- Se a tabela não existe, criar completa
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
GO

PRINT 'Script de correção executado com sucesso!';
GO


