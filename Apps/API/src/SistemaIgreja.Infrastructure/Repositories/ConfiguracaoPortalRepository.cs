using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ConfiguracaoPortalRepository : IConfiguracaoPortalRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ConfiguracaoPortalRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public ConfiguracaoPortalRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ConfiguracaoPortal?> GetAsync()
    {
        var tenantId = await ResolveTenantIdAsync();
        var config = await _context.ConfiguracoesPortal.FirstOrDefaultAsync(c => c.TenantId == tenantId);
        
        if (config == null)
        {
            config = new ConfiguracaoPortal
            {
                TenantId = tenantId,
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
        var tenantId = await ResolveTenantIdAsync();
        var existing = await _context.ConfiguracoesPortal.FirstOrDefaultAsync(c => c.TenantId == tenantId);
        
        if (existing == null)
        {
            configuracao.TenantId = tenantId;
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

    private async Task<int> ResolveTenantIdAsync()
    {
        if (_tenantContext.TenantId.HasValue)
        {
            return _tenantContext.TenantId.Value;
        }

        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == Tenant.InitialTenantSlug);

        return tenant?.Id ?? Tenant.InitialTenantId;
    }
}
