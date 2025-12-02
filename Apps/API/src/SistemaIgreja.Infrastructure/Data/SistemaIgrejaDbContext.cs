using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Infrastructure.Data;

public class SistemaIgrejaDbContext : DbContext
{
    public SistemaIgrejaDbContext(DbContextOptions<SistemaIgrejaDbContext> options) : base(options)
    {
    }

    public DbSet<Visitante> Visitantes { get; set; }
    public DbSet<ConfiguracaoMensagem> ConfiguracoesMensagens { get; set; }
    public DbSet<MensagemAgendada> MensagensAgendadas { get; set; }
    public DbSet<Equipe> Equipes { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<Voluntario> Voluntarios { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<DestaqueSite> DestaquesSite { get; set; }
    public DbSet<CategoriaNoticia> CategoriasNoticias { get; set; }
    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<InscricaoEvento> InscricoesEventos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<CategoriaMidia> CategoriasMidias { get; set; }
    public DbSet<GaleriaFoto> GaleriasFotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Visitante
        modelBuilder.Entity<Visitante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Observacoes).HasMaxLength(500);
            entity.Property(e => e.DataVisita).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired();
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
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WhatsApp).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(v => v.Equipe)
                  .WithMany(e => e.Voluntarios)
                  .HasForeignKey(v => v.EquipeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Cargo)
                  .WithMany(c => c.Voluntarios)
                  .HasForeignKey(v => v.CargoId)
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
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TipoUsuario).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            // Índice único para email
            entity.HasIndex(e => e.Email).IsUnique();
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
    }
}

