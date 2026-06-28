using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class PatrimonioMovimentacaoRepository : IPatrimonioMovimentacaoRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public PatrimonioMovimentacaoRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public PatrimonioMovimentacaoRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<IEnumerable<PatrimonioMovimentacao>> GetByPatrimonioIdAsync(int patrimonioItemId)
    {
        return await _context.Set<PatrimonioMovimentacao>()
            .Where(m => m.PatrimonioItemId == patrimonioItemId)
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<PatrimonioMovimentacao> CreateAsync(PatrimonioMovimentacao movimentacao)
    {
        movimentacao.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<PatrimonioMovimentacao>().Add(movimentacao);
        await _context.SaveChangesAsync();
        return movimentacao;
    }
}
