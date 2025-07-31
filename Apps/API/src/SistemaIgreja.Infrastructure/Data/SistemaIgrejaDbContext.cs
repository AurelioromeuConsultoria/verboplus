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

        // Dados iniciais para ConfiguracaoMensagem
        modelBuilder.Entity<ConfiguracaoMensagem>().HasData(
            new ConfiguracaoMensagem
            {
                Id = 1,
                Nome = "Boas-vindas",
                TextoMensagem = "Olá {Nome}! Que alegria ter você conosco na igreja! Esperamos vê-lo novamente em breve. Deus abençoe!",
                DiasAposVisita = 1,
                HorarioEnvio = new TimeSpan(10, 0, 0), // 10:00
                Ativo = true,
                DataCriacao = DateTime.Now
            },
            new ConfiguracaoMensagem
            {
                Id = 2,
                Nome = "Convite para retorno",
                TextoMensagem = "Oi {Nome}! Sentimos sua falta na igreja. Que tal nos visitar novamente neste domingo? Será um prazer recebê-lo!",
                DiasAposVisita = 7,
                HorarioEnvio = new TimeSpan(18, 0, 0), // 18:00
                Ativo = true,
                DataCriacao = DateTime.Now
            }
        );
    }
}

