using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Infrastructure.Data;

public class SistemaIgrejaDbContext : DbContext
{
    public SistemaIgrejaDbContext(DbContextOptions<SistemaIgrejaDbContext> options) : base(options)
    {
    }

    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<PessoaPerfil> PessoasPerfis { get; set; }
    public DbSet<Visitante> Visitantes { get; set; }
    public DbSet<ConfiguracaoMensagem> ConfiguracoesMensagens { get; set; }
    public DbSet<MensagemAgendada> MensagensAgendadas { get; set; }
    public DbSet<Equipe> Equipes { get; set; }
    public DbSet<HubCasa> HubCasas { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<Voluntario> Voluntarios { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<DestaqueSite> DestaquesSite { get; set; }
    public DbSet<ConfiguracaoPortal> ConfiguracoesPortal { get; set; }
    public DbSet<CategoriaNoticia> CategoriasNoticias { get; set; }
    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<InscricaoEvento> InscricoesEventos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<PerfilAcesso> PerfisAcesso { get; set; }
    public DbSet<PerfilAcessoPermissao> PerfisAcessoPermissoes { get; set; }
    public DbSet<CategoriaMidia> CategoriasMidias { get; set; }
    public DbSet<GaleriaFoto> GaleriasFotos { get; set; }
    public DbSet<Enquete> Enquetes { get; set; }
    public DbSet<EnqueteOpcao> EnqueteOpcoes { get; set; }
    public DbSet<EnqueteVoto> EnqueteVotos { get; set; }
    
    // Kids
    public DbSet<CriancaDetalhe> CriancasDetalhes { get; set; }
    public DbSet<ResponsavelCrianca> ResponsaveisCriancas { get; set; }
    public DbSet<KidsCheckin> KidsCheckins { get; set; }
    public DbSet<KidsNotificacao> KidsNotificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Pessoa
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(20);
            entity.Property(e => e.WhatsApp).HasMaxLength(20);
            entity.Property(e => e.TipoPessoa).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            // Índice único para email (quando não nulo)
            // Usar sintaxe compatível com ambos SQL Server e PostgreSQL
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configuração da entidade PessoaPerfil
        modelBuilder.Entity<PessoaPerfil>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PessoaId).IsRequired();
            entity.Property(e => e.Perfil).IsRequired();
            entity.Property(e => e.DataInicio).IsRequired();

            entity.HasOne(p => p.Pessoa)
                  .WithMany(p => p.Perfis)
                  .HasForeignKey(p => p.PessoaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade Visitante
        modelBuilder.Entity<Visitante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PessoaId).IsRequired();
            entity.Property(e => e.Observacoes).HasMaxLength(500);
            entity.Property(e => e.DataVisita).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(v => v.Pessoa)
                  .WithMany(p => p.Visitantes)
                  .HasForeignKey(v => v.PessoaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade ConfiguracaoMensagem
        modelBuilder.Entity<ConfiguracaoMensagem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TextoMensagem).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DiasAposVisita).IsRequired();
            entity.Property(e => e.HorarioEnvio).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade MensagemAgendada
        modelBuilder.Entity<MensagemAgendada>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DataAgendamento).IsRequired();
            entity.Property(e => e.DataEnvio).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TextoFinal).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.LogErro).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();

            // Relacionamentos
            entity.HasOne(e => e.Visitante)
                  .WithMany(v => v.MensagensAgendadas)
                  .HasForeignKey(e => e.VisitanteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ConfiguracaoMensagem)
                  .WithMany(c => c.MensagensAgendadas)
                  .HasForeignKey(e => e.ConfiguracaoMensagemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade Equipe
        modelBuilder.Entity<Equipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Area).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade HubCasa
        modelBuilder.Entity<HubCasa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EnderecoCompleto).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Anfitriao).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(e => e.AbertoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AbertoPorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Lider)
                  .WithMany()
                  .HasForeignKey(e => e.LiderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Timoteo)
                  .WithMany()
                  .HasForeignKey(e => e.TimoteoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade Fornecedor
        modelBuilder.Entity<Fornecedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.RazaoSocial).HasMaxLength(200);
            entity.Property(e => e.CnpjCpf).HasMaxLength(20);
            entity.Property(e => e.InscricaoEstadual).HasMaxLength(30);
            entity.Property(e => e.Endereco).HasMaxLength(300);
            entity.Property(e => e.Telefone).HasMaxLength(30);
            entity.Property(e => e.Site).HasMaxLength(200);
            entity.Property(e => e.ContatoNome).HasMaxLength(150);
            entity.Property(e => e.ContatoCpf).HasMaxLength(20);
            entity.Property(e => e.ContatoWhatsApp).HasMaxLength(30);
            entity.Property(e => e.ContatoEmail).HasMaxLength(150);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade Cargo
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade Voluntario
        modelBuilder.Entity<Voluntario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PessoaId).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(v => v.Pessoa)
                  .WithMany(p => p.Voluntarios)
                  .HasForeignKey(v => v.PessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Equipe)
                  .WithMany(e => e.Voluntarios)
                  .HasForeignKey(v => v.EquipeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Cargo)
                  .WithMany(c => c.Voluntarios)
                  .HasForeignKey(v => v.CargoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade PerfilAcesso
        modelBuilder.Entity<PerfilAcesso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(300);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade PerfilAcessoPermissao
        modelBuilder.Entity<PerfilAcessoPermissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Recurso).IsRequired().HasMaxLength(80);

            entity.HasOne(e => e.PerfilAcesso)
                  .WithMany(p => p.Permissoes)
                  .HasForeignKey(e => e.PerfilAcessoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed perfil Administrador
        modelBuilder.Entity<PerfilAcesso>().HasData(new PerfilAcesso
        {
            Id = 1,
            Nome = "Administrador",
            Descricao = "Acesso total ao sistema",
            DataCriacao = new DateTime(2026, 2, 6, 0, 0, 0)
        });

        var recursos = new[]
        {
            "dashboard",
            "usuarios",
            "perfis-acesso",
            "pessoas",
            "perfis",
            "visitantes",
            "configuracoes-mensagens",
            "mensagens-agendadas",
            "equipes",
            "cargos",
            "voluntarios",
            "eventos",
            "inscricoes-eventos",
            "portal",
            "noticias",
            "categorias-noticias",
            "contatos",
            "destaques-site",
            "categorias-midias",
            "galerias-fotos",
            "enquetes",
            "kids",
            "hub",
            "financeiro",
            "fornecedores"
        };

        var permissoes = recursos.Select((recurso, index) => new PerfilAcessoPermissao
        {
            Id = 1000 + index,
            PerfilAcessoId = 1,
            Recurso = recurso,
            PodeVer = true,
            PodeEditar = true,
            PodeExcluir = true
        });

        modelBuilder.Entity<PerfilAcessoPermissao>().HasData(permissoes);

        // Relacionamento Usuario -> PerfilAcesso
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasOne(u => u.PerfilAcesso)
                  .WithMany(p => p.Usuarios)
                  .HasForeignKey(u => u.PerfilAcessoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade Evento
        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.ImagemDestaque).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.DataInicio).IsRequired();
            entity.Property(e => e.DataFim).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade DestaqueSite
        modelBuilder.Entity<DestaqueSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Texto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Imagem).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade ConfiguracaoPortal (singleton)
        modelBuilder.Entity<ConfiguracaoPortal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TempoTransicaoCarrossel).IsRequired().HasDefaultValue(5);
            entity.Property(e => e.DataAtualizacao).IsRequired();
            // Garantir que sempre haverá apenas um registro
            entity.HasData(new ConfiguracaoPortal
            {
                Id = 1,
                TempoTransicaoCarrossel = 5,
                DataAtualizacao = new DateTime(2026, 2, 4, 0, 0, 0) // Data fixa para migration
            });
        });

        // Configuração da entidade CategoriaNoticia
        modelBuilder.Entity<CategoriaNoticia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade Noticia
        modelBuilder.Entity<Noticia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Texto).HasMaxLength(5000);
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Imagem).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(n => n.CategoriaNoticia)
                  .WithMany(c => c.Noticias)
                  .HasForeignKey(n => n.CategoriaNoticiaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade Contato
        modelBuilder.Entity<Contato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WhatsApp).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Membro).IsRequired();
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade InscricaoEvento
        modelBuilder.Entity<InscricaoEvento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WhatsApp).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.QuantidadeAcompanhantes).IsRequired();
            entity.Property(e => e.Observacoes).HasMaxLength(500);
            entity.Property(e => e.ObservacoesInternas).HasMaxLength(500);
            entity.Property(e => e.DataInscricao).IsRequired();

            entity.HasOne(i => i.Evento)
                  .WithMany(e => e.Inscricoes)
                  .HasForeignKey(i => i.EventoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para busca rápida
            entity.HasIndex(i => new { i.EventoId, i.WhatsApp });
        });

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PessoaId).IsRequired();
            entity.Property(e => e.EmailLogin).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TipoUsuario).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(u => u.Pessoa)
                  .WithOne(p => p.Usuario)
                  .HasForeignKey<Usuario>(u => u.PessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice único para EmailLogin
            entity.HasIndex(e => e.EmailLogin).IsUnique();
        });

        // Configuração da entidade CategoriaMidia
        modelBuilder.Entity<CategoriaMidia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade GaleriaFoto
        modelBuilder.Entity<GaleriaFoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.CaminhoDiretorio).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ImagemDestaque).HasMaxLength(500);
            entity.Property(e => e.QuantidadeFotos).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(g => g.Evento)
                  .WithMany()
                  .HasForeignKey(g => g.EventoId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(g => g.CategoriaMidia)
                  .WithMany(c => c.Galerias)
                  .HasForeignKey(g => g.CategoriaMidiaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuração da entidade CriancaDetalhe
        modelBuilder.Entity<CriancaDetalhe>(entity =>
        {
            entity.HasKey(e => e.PessoaId);
            entity.Property(e => e.Alergias).HasMaxLength(500);
            entity.Property(e => e.RestricoesAlimentares).HasMaxLength(500);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);
            entity.Property(e => e.SalaId).HasMaxLength(50);
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(c => c.Pessoa)
                  .WithOne(p => p.CriancaDetalhe)
                  .HasForeignKey<CriancaDetalhe>(c => c.PessoaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade ResponsavelCrianca
        modelBuilder.Entity<ResponsavelCrianca>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CriancaPessoaId).IsRequired();
            entity.Property(e => e.ResponsavelPessoaId).IsRequired();
            entity.Property(e => e.PodeRetirar).IsRequired();
            entity.Property(e => e.Parentesco).HasMaxLength(50);
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(r => r.Crianca)
                  .WithMany(p => p.ResponsaveisComoCrianca)
                  .HasForeignKey(r => r.CriancaPessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Responsavel)
                  .WithMany(p => p.ResponsaveisComoResponsavel)
                  .HasForeignKey(r => r.ResponsavelPessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para busca rápida
            entity.HasIndex(r => new { r.CriancaPessoaId, r.ResponsavelPessoaId });
        });

        // Configuração da entidade KidsCheckin
        modelBuilder.Entity<KidsCheckin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CriancaPessoaId).IsRequired();
            entity.Property(e => e.CheckinTime).IsRequired();
            entity.Property(e => e.Metodo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CodigoSessao).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Observacoes).HasMaxLength(500);

            entity.HasOne(c => c.Crianca)
                  .WithMany(p => p.Checkins)
                  .HasForeignKey(c => c.CriancaPessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.CheckinBy)
                  .WithMany(p => p.CheckinsRealizadosPor)
                  .HasForeignKey(c => c.CheckinByPessoaId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(c => c.CheckoutBy)
                  .WithMany(p => p.CheckoutsRealizadosPor)
                  .HasForeignKey(c => c.CheckoutByPessoaId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Índices
            entity.HasIndex(c => c.CodigoSessao);
            entity.HasIndex(c => new { c.CriancaPessoaId, c.Status });
        });

        // Configuração da entidade KidsNotificacao
        modelBuilder.Entity<KidsNotificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CriancaPessoaId).IsRequired();
            entity.Property(e => e.ResponsavelPessoaId).IsRequired();
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(n => n.Crianca)
                  .WithMany(p => p.NotificacoesComoCrianca)
                  .HasForeignKey(n => n.CriancaPessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(n => n.Responsavel)
                  .WithMany(p => p.NotificacoesComoResponsavel)
                  .HasForeignKey(n => n.ResponsavelPessoaId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices
            entity.HasIndex(n => new { n.CriancaPessoaId, n.Status });
            entity.HasIndex(n => new { n.ResponsavelPessoaId, n.Status });
        });

        // Dados iniciais para ConfiguracaoMensagem (datas fixas para evitar warnings de migração)
        var seedDate = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<ConfiguracaoMensagem>().HasData(
            new ConfiguracaoMensagem
            {
                Id = 1,
                Nome = "Boas-vindas",
                TextoMensagem = "Olá {Nome}! Que alegria ter você conosco na igreja! Esperamos vê-lo novamente em breve. Deus abençoe!",
                DiasAposVisita = 1,
                HorarioEnvio = new TimeSpan(10, 0, 0), // 10:00
                Ativo = true,
                DataCriacao = seedDate
            },
            new ConfiguracaoMensagem
            {
                Id = 2,
                Nome = "Convite para retorno",
                TextoMensagem = "Oi {Nome}! Sentimos sua falta na igreja. Que tal nos visitar novamente neste domingo? Será um prazer recebê-lo!",
                DiasAposVisita = 7,
                HorarioEnvio = new TimeSpan(18, 0, 0), // 18:00
                Ativo = true,
                DataCriacao = seedDate
            }
        );

        // Configuração da entidade Enquete
        modelBuilder.Entity<Enquete>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.DataInicio).IsRequired();
            entity.Property(e => e.DataFim).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.PermitirMultiplaEscolha).IsRequired();
            entity.Property(e => e.PermitirVotoAnonimo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração da entidade EnqueteOpcao
        modelBuilder.Entity<EnqueteOpcao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EnqueteId).IsRequired();
            entity.Property(e => e.Texto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Ordem).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(e => e.Enquete)
                  .WithMany(e => e.Opcoes)
                  .HasForeignKey(e => e.EnqueteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade EnqueteVoto
        modelBuilder.Entity<EnqueteVoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EnqueteId).IsRequired();
            entity.Property(e => e.EnqueteOpcaoId).IsRequired();
            entity.Property(e => e.NomeAnonimo).HasMaxLength(100);
            entity.Property(e => e.DataVoto).IsRequired();

            entity.HasOne(e => e.Enquete)
                  .WithMany(e => e.Votos)
                  .HasForeignKey(e => e.EnqueteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Opcao)
                  .WithMany(e => e.Votos)
                  .HasForeignKey(e => e.EnqueteOpcaoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
