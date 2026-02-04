using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ConfiguracaoPortalRepository : IConfiguracaoPortalRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ConfiguracaoPortalRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracaoPortal?> GetAsync()
    {
        var config = await _context.ConfiguracoesPortal.FirstOrDefaultAsync();
        
        // Se não existir, cria uma configuração padrão
        if (config == null)
        {
            config = new ConfiguracaoPortal
            {
                Id = 1,
                TempoTransicaoCarrossel = 5,
                DataAtualizacao = DateTime.Now
            };
            _context.ConfiguracoesPortal.Add(config);
            await _context.SaveChangesAsync();
        }
        
        return config;
    }

    public async Task<ConfiguracaoPortal> UpdateAsync(ConfiguracaoPortal configuracao)
    {
        var existing = await _context.ConfiguracoesPortal.FirstOrDefaultAsync();
        
        if (existing == null)
        {
            configuracao.Id = 1;
            configuracao.DataAtualizacao = DateTime.Now;
            _context.ConfiguracoesPortal.Add(configuracao);
        }
        else
        {
            existing.TempoTransicaoCarrossel = configuracao.TempoTransicaoCarrossel;
            existing.DataAtualizacao = DateTime.Now;
        }
        
        await _context.SaveChangesAsync();
        return existing ?? configuracao;
    }
}
