using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Marcar migrations antigas como aplicadas (para evitar conflitos)
            migrationBuilder.Sql(@"
                INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                VALUES 
                ('20251211042104_InitialCreate', '9.0.6'),
                ('20251212034213_RefatoracaoPessoaCentralizada', '9.0.6'),
                ('20260204014943_AdicionarEnquetes', '9.0.6'),
                ('20260204022604_AdicionarTempoTransicaoDestaqueSite', '9.0.6'),
                ('20260204023037_RemoverTempoTransicaoEAdicionarConfiguracaoPortal', '9.0.6'),
                ('20260206204124_AdicionarHubCasas', '9.0.6'),
                ('20260206213029_AdicionarFornecedores', '9.0.6'),
                ('20260216205904_AdicionarPerfisAcesso', '9.0.6'),
                ('20260216210044_AssociarUsuariosAoPerfilAdmin', '9.0.6')
                ON CONFLICT (""MigrationId"") DO NOTHING;
            ");

            // Migration inicial completa para PostgreSQL usando SQL direto
            migrationBuilder.Sql(@"
                -- Criar tabela Pessoas
                CREATE TABLE IF NOT EXISTS ""Pessoas"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""Email"" VARCHAR(100),
                    ""Telefone"" VARCHAR(20),
                    ""WhatsApp"" VARCHAR(20),
                    ""DataNascimento"" TIMESTAMP,
                    ""TipoPessoa"" INTEGER NOT NULL,
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Pessoas_Email"" ON ""Pessoas"" (""Email"") WHERE ""Email"" IS NOT NULL;

                -- Criar tabela PessoasPerfis
                CREATE TABLE IF NOT EXISTS ""PessoasPerfis"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PessoaId"" INTEGER NOT NULL,
                    ""Perfil"" INTEGER NOT NULL,
                    ""DataInicio"" TIMESTAMP NOT NULL,
                    ""DataFim"" TIMESTAMP,
                    CONSTRAINT ""FK_PessoasPerfis_Pessoas_PessoaId"" FOREIGN KEY (""PessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE CASCADE
                );

                -- Criar tabela Cargos
                CREATE TABLE IF NOT EXISTS ""Cargos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela Equipes
                CREATE TABLE IF NOT EXISTS ""Equipes"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""Area"" INTEGER NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela Voluntarios
                CREATE TABLE IF NOT EXISTS ""Voluntarios"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PessoaId"" INTEGER NOT NULL,
                    ""EquipeId"" INTEGER,
                    ""CargoId"" INTEGER,
                    ""DataCadastro"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_Voluntarios_Pessoas_PessoaId"" FOREIGN KEY (""PessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Voluntarios_Equipes_EquipeId"" FOREIGN KEY (""EquipeId"") REFERENCES ""Equipes"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Voluntarios_Cargos_CargoId"" FOREIGN KEY (""CargoId"") REFERENCES ""Cargos"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_Voluntarios_PessoaId"" ON ""Voluntarios"" (""PessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_Voluntarios_EquipeId"" ON ""Voluntarios"" (""EquipeId"");
                CREATE INDEX IF NOT EXISTS ""IX_Voluntarios_CargoId"" ON ""Voluntarios"" (""CargoId"");

                -- Criar tabela Visitantes
                CREATE TABLE IF NOT EXISTS ""Visitantes"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PessoaId"" INTEGER NOT NULL,
                    ""Observacoes"" VARCHAR(500),
                    ""DataVisita"" TIMESTAMP NOT NULL,
                    ""DataCadastro"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_Visitantes_Pessoas_PessoaId"" FOREIGN KEY (""PessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_Visitantes_PessoaId"" ON ""Visitantes"" (""PessoaId"");

                -- Criar tabela ConfiguracoesMensagens
                CREATE TABLE IF NOT EXISTS ""ConfiguracoesMensagens"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(200) NOT NULL,
                    ""TextoMensagem"" VARCHAR(1000) NOT NULL,
                    ""DiasAposVisita"" INTEGER NOT NULL,
                    ""HorarioEnvio"" TIME NOT NULL,
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Inserir dados iniciais em ConfiguracoesMensagens
                INSERT INTO ""ConfiguracoesMensagens"" (""Id"", ""Nome"", ""TextoMensagem"", ""DiasAposVisita"", ""HorarioEnvio"", ""Ativo"", ""DataCriacao"")
                VALUES 
                (1, 'Boas-vindas', 'Olá {Nome}! Que alegria ter você conosco na igreja! Esperamos vê-lo novamente em breve. Deus abençoe!', 1, '10:00:00', true, '2025-01-01 00:00:00'),
                (2, 'Convite para retorno', 'Oi {Nome}! Sentimos sua falta na igreja. Que tal nos visitar novamente neste domingo? Será um prazer recebê-lo!', 7, '18:00:00', true, '2025-01-01 00:00:00')
                ON CONFLICT (""Id"") DO NOTHING;

                -- Criar tabela MensagensAgendadas
                CREATE TABLE IF NOT EXISTS ""MensagensAgendadas"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""VisitanteId"" INTEGER NOT NULL,
                    ""ConfiguracaoMensagemId"" INTEGER NOT NULL,
                    ""DataAgendamento"" TIMESTAMP NOT NULL,
                    ""DataEnvio"" TIMESTAMP NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""TextoFinal"" VARCHAR(1000) NOT NULL,
                    ""DataProcessamento"" TIMESTAMP,
                    ""LogErro"" VARCHAR(500),
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_MensagensAgendadas_Visitantes_VisitanteId"" FOREIGN KEY (""VisitanteId"") REFERENCES ""Visitantes"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_MensagensAgendadas_ConfiguracoesMensagens_ConfiguracaoMensagemId"" FOREIGN KEY (""ConfiguracaoMensagemId"") REFERENCES ""ConfiguracoesMensagens"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_MensagensAgendadas_VisitanteId"" ON ""MensagensAgendadas"" (""VisitanteId"");
                CREATE INDEX IF NOT EXISTS ""IX_MensagensAgendadas_ConfiguracaoMensagemId"" ON ""MensagensAgendadas"" (""ConfiguracaoMensagemId"");

                -- Criar tabela Usuarios
                CREATE TABLE IF NOT EXISTS ""Usuarios"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""PessoaId"" INTEGER NOT NULL,
                    ""EmailLogin"" VARCHAR(100) NOT NULL,
                    ""SenhaHash"" VARCHAR(255) NOT NULL,
                    ""TipoUsuario"" INTEGER NOT NULL,
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    ""PerfilAcessoId"" INTEGER,
                    CONSTRAINT ""FK_Usuarios_Pessoas_PessoaId"" FOREIGN KEY (""PessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_EmailLogin"" ON ""Usuarios"" (""EmailLogin"");
                CREATE INDEX IF NOT EXISTS ""IX_Usuarios_PessoaId"" ON ""Usuarios"" (""PessoaId"");

                -- Criar tabela PerfisAcesso
                CREATE TABLE IF NOT EXISTS ""PerfisAcesso"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""Descricao"" VARCHAR(300),
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Inserir perfil Administrador
                INSERT INTO ""PerfisAcesso"" (""Id"", ""Nome"", ""Descricao"", ""DataCriacao"")
                VALUES (1, 'Administrador', 'Acesso total ao sistema', '2026-02-06 00:00:00')
                ON CONFLICT (""Id"") DO NOTHING;

                -- Criar tabela PerfisAcessoPermissoes
                CREATE TABLE IF NOT EXISTS ""PerfisAcessoPermissoes"" (
                    ""Id"" INTEGER PRIMARY KEY,
                    ""PerfilAcessoId"" INTEGER NOT NULL,
                    ""Recurso"" VARCHAR(80) NOT NULL,
                    ""PodeVer"" BOOLEAN NOT NULL,
                    ""PodeEditar"" BOOLEAN NOT NULL,
                    ""PodeExcluir"" BOOLEAN NOT NULL,
                    CONSTRAINT ""FK_PerfisAcessoPermissoes_PerfisAcesso_PerfilAcessoId"" FOREIGN KEY (""PerfilAcessoId"") REFERENCES ""PerfisAcesso"" (""Id"") ON DELETE CASCADE
                );

                -- Inserir permissões do perfil Administrador
                INSERT INTO ""PerfisAcessoPermissoes"" (""Id"", ""PerfilAcessoId"", ""Recurso"", ""PodeVer"", ""PodeEditar"", ""PodeExcluir"")
                VALUES
                (1000, 1, 'dashboard', true, true, true),
                (1001, 1, 'usuarios', true, true, true),
                (1002, 1, 'perfis-acesso', true, true, true),
                (1003, 1, 'pessoas', true, true, true),
                (1004, 1, 'perfis', true, true, true),
                (1005, 1, 'visitantes', true, true, true),
                (1006, 1, 'configuracoes-mensagens', true, true, true),
                (1007, 1, 'mensagens-agendadas', true, true, true),
                (1008, 1, 'equipes', true, true, true),
                (1009, 1, 'cargos', true, true, true),
                (1010, 1, 'voluntarios', true, true, true),
                (1011, 1, 'eventos', true, true, true),
                (1012, 1, 'inscricoes-eventos', true, true, true),
                (1013, 1, 'portal', true, true, true),
                (1014, 1, 'noticias', true, true, true),
                (1015, 1, 'categorias-noticias', true, true, true),
                (1016, 1, 'contatos', true, true, true),
                (1017, 1, 'destaques-site', true, true, true),
                (1018, 1, 'categorias-midias', true, true, true),
                (1019, 1, 'galerias-fotos', true, true, true),
                (1020, 1, 'enquetes', true, true, true),
                (1021, 1, 'kids', true, true, true),
                (1022, 1, 'hub', true, true, true),
                (1023, 1, 'financeiro', true, true, true),
                (1024, 1, 'fornecedores', true, true, true)
                ON CONFLICT (""Id"") DO NOTHING;

                -- Adicionar foreign key de Usuarios para PerfisAcesso
                ALTER TABLE ""Usuarios"" ADD CONSTRAINT ""FK_Usuarios_PerfisAcesso_PerfilAcessoId"" 
                    FOREIGN KEY (""PerfilAcessoId"") REFERENCES ""PerfisAcesso"" (""Id"") ON DELETE RESTRICT;

                CREATE INDEX IF NOT EXISTS ""IX_Usuarios_PerfilAcessoId"" ON ""Usuarios"" (""PerfilAcessoId"");

                -- Criar tabela Eventos
                CREATE TABLE IF NOT EXISTS ""Eventos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Titulo"" VARCHAR(200) NOT NULL,
                    ""Descricao"" VARCHAR(1000),
                    ""ImagemDestaque"" VARCHAR(500),
                    ""Url"" VARCHAR(500),
                    ""DataInicio"" TIMESTAMP NOT NULL,
                    ""DataFim"" TIMESTAMP NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela InscricoesEventos
                CREATE TABLE IF NOT EXISTS ""InscricoesEventos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""EventoId"" INTEGER NOT NULL,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""WhatsApp"" VARCHAR(20) NOT NULL,
                    ""Email"" VARCHAR(100),
                    ""Status"" INTEGER NOT NULL,
                    ""QuantidadeAcompanhantes"" INTEGER NOT NULL,
                    ""Observacoes"" VARCHAR(500),
                    ""ObservacoesInternas"" VARCHAR(500),
                    ""DataInscricao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_InscricoesEventos_Eventos_EventoId"" FOREIGN KEY (""EventoId"") REFERENCES ""Eventos"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_InscricoesEventos_EventoId"" ON ""InscricoesEventos"" (""EventoId"");
                CREATE INDEX IF NOT EXISTS ""IX_InscricoesEventos_EventoId_WhatsApp"" ON ""InscricoesEventos"" (""EventoId"", ""WhatsApp"");

                -- Criar tabela DestaquesSite
                CREATE TABLE IF NOT EXISTS ""DestaquesSite"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Texto"" VARCHAR(200) NOT NULL,
                    ""Descricao"" VARCHAR(1000),
                    ""Url"" VARCHAR(500),
                    ""Imagem"" VARCHAR(500),
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela ConfiguracoesPortal
                CREATE TABLE IF NOT EXISTS ""ConfiguracoesPortal"" (
                    ""Id"" INTEGER PRIMARY KEY,
                    ""TempoTransicaoCarrossel"" INTEGER NOT NULL DEFAULT 5,
                    ""DataAtualizacao"" TIMESTAMP NOT NULL
                );

                -- Inserir configuração inicial do portal
                INSERT INTO ""ConfiguracoesPortal"" (""Id"", ""TempoTransicaoCarrossel"", ""DataAtualizacao"")
                VALUES (1, 5, '2026-02-04 00:00:00')
                ON CONFLICT (""Id"") DO NOTHING;

                -- Criar tabela CategoriasNoticias
                CREATE TABLE IF NOT EXISTS ""CategoriasNoticias"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela Noticias
                CREATE TABLE IF NOT EXISTS ""Noticias"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Titulo"" VARCHAR(200) NOT NULL,
                    ""Descricao"" VARCHAR(1000),
                    ""Texto"" VARCHAR(5000),
                    ""Data"" TIMESTAMP NOT NULL,
                    ""Url"" VARCHAR(500),
                    ""Imagem"" VARCHAR(500),
                    ""CategoriaNoticiaId"" INTEGER NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_Noticias_CategoriasNoticias_CategoriaNoticiaId"" FOREIGN KEY (""CategoriaNoticiaId"") REFERENCES ""CategoriasNoticias"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_Noticias_CategoriaNoticiaId"" ON ""Noticias"" (""CategoriaNoticiaId"");

                -- Criar tabela Contatos
                CREATE TABLE IF NOT EXISTS ""Contatos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""WhatsApp"" VARCHAR(20) NOT NULL,
                    ""Email"" VARCHAR(100),
                    ""Membro"" BOOLEAN NOT NULL,
                    ""Mensagem"" VARCHAR(2000) NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela CategoriasMidias
                CREATE TABLE IF NOT EXISTS ""CategoriasMidias"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""Descricao"" VARCHAR(500),
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela GaleriasFotos
                CREATE TABLE IF NOT EXISTS ""GaleriasFotos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(200) NOT NULL,
                    ""Descricao"" VARCHAR(1000),
                    ""Data"" TIMESTAMP NOT NULL,
                    ""CaminhoDiretorio"" VARCHAR(500) NOT NULL,
                    ""ImagemDestaque"" VARCHAR(500),
                    ""QuantidadeFotos"" INTEGER NOT NULL,
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""EventoId"" INTEGER,
                    ""CategoriaMidiaId"" INTEGER,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_GaleriasFotos_Eventos_EventoId"" FOREIGN KEY (""EventoId"") REFERENCES ""Eventos"" (""Id"") ON DELETE SET NULL,
                    CONSTRAINT ""FK_GaleriasFotos_CategoriasMidias_CategoriaMidiaId"" FOREIGN KEY (""CategoriaMidiaId"") REFERENCES ""CategoriasMidias"" (""Id"") ON DELETE SET NULL
                );

                CREATE INDEX IF NOT EXISTS ""IX_GaleriasFotos_EventoId"" ON ""GaleriasFotos"" (""EventoId"");
                CREATE INDEX IF NOT EXISTS ""IX_GaleriasFotos_CategoriaMidiaId"" ON ""GaleriasFotos"" (""CategoriaMidiaId"");

                -- Criar tabela HubCasas
                CREATE TABLE IF NOT EXISTS ""HubCasas"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(100) NOT NULL,
                    ""EnderecoCompleto"" VARCHAR(300) NOT NULL,
                    ""Anfitriao"" VARCHAR(100) NOT NULL,
                    ""AbertoPorId"" INTEGER,
                    ""LiderId"" INTEGER,
                    ""TimoteoId"" INTEGER,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_HubCasas_Pessoas_AbertoPorId"" FOREIGN KEY (""AbertoPorId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_HubCasas_Pessoas_LiderId"" FOREIGN KEY (""LiderId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_HubCasas_Pessoas_TimoteoId"" FOREIGN KEY (""TimoteoId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_HubCasas_AbertoPorId"" ON ""HubCasas"" (""AbertoPorId"");
                CREATE INDEX IF NOT EXISTS ""IX_HubCasas_LiderId"" ON ""HubCasas"" (""LiderId"");
                CREATE INDEX IF NOT EXISTS ""IX_HubCasas_TimoteoId"" ON ""HubCasas"" (""TimoteoId"");

                -- Criar tabela Fornecedores
                CREATE TABLE IF NOT EXISTS ""Fornecedores"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Nome"" VARCHAR(150) NOT NULL,
                    ""RazaoSocial"" VARCHAR(200),
                    ""CnpjCpf"" VARCHAR(20),
                    ""InscricaoEstadual"" VARCHAR(30),
                    ""Endereco"" VARCHAR(300),
                    ""Telefone"" VARCHAR(30),
                    ""Site"" VARCHAR(200),
                    ""ContatoNome"" VARCHAR(150),
                    ""ContatoCpf"" VARCHAR(20),
                    ""ContatoWhatsApp"" VARCHAR(30),
                    ""ContatoEmail"" VARCHAR(150),
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela Enquetes
                CREATE TABLE IF NOT EXISTS ""Enquetes"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Titulo"" VARCHAR(200) NOT NULL,
                    ""Descricao"" VARCHAR(1000),
                    ""DataInicio"" TIMESTAMP NOT NULL,
                    ""DataFim"" TIMESTAMP NOT NULL,
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""PermitirMultiplaEscolha"" BOOLEAN NOT NULL,
                    ""PermitirVotoAnonimo"" BOOLEAN NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL
                );

                -- Criar tabela EnqueteOpcoes
                CREATE TABLE IF NOT EXISTS ""EnqueteOpcoes"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""EnqueteId"" INTEGER NOT NULL,
                    ""Texto"" VARCHAR(200) NOT NULL,
                    ""Ordem"" INTEGER NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_EnqueteOpcoes_Enquetes_EnqueteId"" FOREIGN KEY (""EnqueteId"") REFERENCES ""Enquetes"" (""Id"") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS ""IX_EnqueteOpcoes_EnqueteId"" ON ""EnqueteOpcoes"" (""EnqueteId"");

                -- Criar tabela EnqueteVotos
                CREATE TABLE IF NOT EXISTS ""EnqueteVotos"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""EnqueteId"" INTEGER NOT NULL,
                    ""EnqueteOpcaoId"" INTEGER NOT NULL,
                    ""UsuarioId"" INTEGER,
                    ""NomeAnonimo"" VARCHAR(100),
                    ""DataVoto"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_EnqueteVotos_Enquetes_EnqueteId"" FOREIGN KEY (""EnqueteId"") REFERENCES ""Enquetes"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_EnqueteVotos_EnqueteOpcoes_EnqueteOpcaoId"" FOREIGN KEY (""EnqueteOpcaoId"") REFERENCES ""EnqueteOpcoes"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_EnqueteVotos_Usuarios_UsuarioId"" FOREIGN KEY (""UsuarioId"") REFERENCES ""Usuarios"" (""Id"") ON DELETE SET NULL
                );

                CREATE INDEX IF NOT EXISTS ""IX_EnqueteVotos_EnqueteId"" ON ""EnqueteVotos"" (""EnqueteId"");
                CREATE INDEX IF NOT EXISTS ""IX_EnqueteVotos_EnqueteOpcaoId"" ON ""EnqueteVotos"" (""EnqueteOpcaoId"");
                CREATE INDEX IF NOT EXISTS ""IX_EnqueteVotos_UsuarioId"" ON ""EnqueteVotos"" (""UsuarioId"");

                -- Criar tabela CriancasDetalhes
                CREATE TABLE IF NOT EXISTS ""CriancasDetalhes"" (
                    ""PessoaId"" INTEGER PRIMARY KEY,
                    ""Alergias"" VARCHAR(500),
                    ""RestricoesAlimentares"" VARCHAR(500),
                    ""Observacoes"" VARCHAR(1000),
                    ""SalaId"" VARCHAR(50),
                    ""DataCadastro"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_CriancasDetalhes_Pessoas_PessoaId"" FOREIGN KEY (""PessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE CASCADE
                );

                -- Criar tabela ResponsaveisCriancas
                CREATE TABLE IF NOT EXISTS ""ResponsaveisCriancas"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CriancaPessoaId"" INTEGER NOT NULL,
                    ""ResponsavelPessoaId"" INTEGER NOT NULL,
                    ""PodeRetirar"" BOOLEAN NOT NULL,
                    ""Parentesco"" VARCHAR(50),
                    ""Ativo"" BOOLEAN NOT NULL,
                    ""DataCadastro"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_ResponsaveisCriancas_Pessoas_CriancaPessoaId"" FOREIGN KEY (""CriancaPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_ResponsaveisCriancas_Pessoas_ResponsavelPessoaId"" FOREIGN KEY (""ResponsavelPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_ResponsaveisCriancas_CriancaPessoaId"" ON ""ResponsaveisCriancas"" (""CriancaPessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_ResponsaveisCriancas_ResponsavelPessoaId"" ON ""ResponsaveisCriancas"" (""ResponsavelPessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_ResponsaveisCriancas_CriancaPessoaId_ResponsavelPessoaId"" ON ""ResponsaveisCriancas"" (""CriancaPessoaId"", ""ResponsavelPessoaId"");

                -- Criar tabela KidsCheckins
                CREATE TABLE IF NOT EXISTS ""KidsCheckins"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CriancaPessoaId"" INTEGER NOT NULL,
                    ""CheckinTime"" TIMESTAMP NOT NULL,
                    ""Metodo"" VARCHAR(20) NOT NULL,
                    ""CodigoSessao"" VARCHAR(50) NOT NULL,
                    ""Status"" VARCHAR(20) NOT NULL,
                    ""Observacoes"" VARCHAR(500),
                    ""CheckinByPessoaId"" INTEGER,
                    ""CheckoutByPessoaId"" INTEGER,
                    CONSTRAINT ""FK_KidsCheckins_Pessoas_CriancaPessoaId"" FOREIGN KEY (""CriancaPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_KidsCheckins_Pessoas_CheckinByPessoaId"" FOREIGN KEY (""CheckinByPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE NO ACTION,
                    CONSTRAINT ""FK_KidsCheckins_Pessoas_CheckoutByPessoaId"" FOREIGN KEY (""CheckoutByPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE NO ACTION
                );

                CREATE INDEX IF NOT EXISTS ""IX_KidsCheckins_CriancaPessoaId"" ON ""KidsCheckins"" (""CriancaPessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_KidsCheckins_CodigoSessao"" ON ""KidsCheckins"" (""CodigoSessao"");
                CREATE INDEX IF NOT EXISTS ""IX_KidsCheckins_CriancaPessoaId_Status"" ON ""KidsCheckins"" (""CriancaPessoaId"", ""Status"");

                -- Criar tabela KidsNotificacoes
                CREATE TABLE IF NOT EXISTS ""KidsNotificacoes"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""CriancaPessoaId"" INTEGER NOT NULL,
                    ""ResponsavelPessoaId"" INTEGER NOT NULL,
                    ""Tipo"" VARCHAR(20) NOT NULL,
                    ""Mensagem"" VARCHAR(1000) NOT NULL,
                    ""Status"" VARCHAR(20) NOT NULL,
                    ""DataCriacao"" TIMESTAMP NOT NULL,
                    CONSTRAINT ""FK_KidsNotificacoes_Pessoas_CriancaPessoaId"" FOREIGN KEY (""CriancaPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_KidsNotificacoes_Pessoas_ResponsavelPessoaId"" FOREIGN KEY (""ResponsavelPessoaId"") REFERENCES ""Pessoas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS ""IX_KidsNotificacoes_CriancaPessoaId"" ON ""KidsNotificacoes"" (""CriancaPessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_KidsNotificacoes_ResponsavelPessoaId"" ON ""KidsNotificacoes"" (""ResponsavelPessoaId"");
                CREATE INDEX IF NOT EXISTS ""IX_KidsNotificacoes_CriancaPessoaId_Status"" ON ""KidsNotificacoes"" (""CriancaPessoaId"", ""Status"");
                CREATE INDEX IF NOT EXISTS ""IX_KidsNotificacoes_ResponsavelPessoaId_Status"" ON ""KidsNotificacoes"" (""ResponsavelPessoaId"", ""Status"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover tabelas na ordem inversa (respeitando foreign keys)
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""KidsNotificacoes"";
                DROP TABLE IF EXISTS ""KidsCheckins"";
                DROP TABLE IF EXISTS ""ResponsaveisCriancas"";
                DROP TABLE IF EXISTS ""CriancasDetalhes"";
                DROP TABLE IF EXISTS ""EnqueteVotos"";
                DROP TABLE IF EXISTS ""EnqueteOpcoes"";
                DROP TABLE IF EXISTS ""Enquetes"";
                DROP TABLE IF EXISTS ""Fornecedores"";
                DROP TABLE IF EXISTS ""HubCasas"";
                DROP TABLE IF EXISTS ""GaleriasFotos"";
                DROP TABLE IF EXISTS ""CategoriasMidias"";
                DROP TABLE IF EXISTS ""Contatos"";
                DROP TABLE IF EXISTS ""Noticias"";
                DROP TABLE IF EXISTS ""CategoriasNoticias"";
                DROP TABLE IF EXISTS ""ConfiguracoesPortal"";
                DROP TABLE IF EXISTS ""DestaquesSite"";
                DROP TABLE IF EXISTS ""InscricoesEventos"";
                DROP TABLE IF EXISTS ""Eventos"";
                DROP TABLE IF EXISTS ""PerfisAcessoPermissoes"";
                DROP TABLE IF EXISTS ""PerfisAcesso"";
                DROP TABLE IF EXISTS ""Usuarios"";
                DROP TABLE IF EXISTS ""MensagensAgendadas"";
                DROP TABLE IF EXISTS ""ConfiguracoesMensagens"";
                DROP TABLE IF EXISTS ""Visitantes"";
                DROP TABLE IF EXISTS ""Voluntarios"";
                DROP TABLE IF EXISTS ""Equipes"";
                DROP TABLE IF EXISTS ""Cargos"";
                DROP TABLE IF EXISTS ""PessoasPerfis"";
                DROP TABLE IF EXISTS ""Pessoas"";
            ");
        }
    }
}
