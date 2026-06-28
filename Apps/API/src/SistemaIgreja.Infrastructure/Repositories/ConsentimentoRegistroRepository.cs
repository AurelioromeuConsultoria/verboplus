using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ConsentimentoRegistroRepository : IConsentimentoRegistroRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ConsentimentoRegistroRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public ConsentimentoRegistroRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ConsentimentoRegistro> CreateWithoutSaveAsync(ConsentimentoRegistro registro)
    {
        if (registro.TenantId <= 0)
        {
            registro.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        }

        await _context.Set<ConsentimentoRegistro>().AddAsync(registro);
        return registro;
    }

    public async Task<IEnumerable<ConsentimentoRegistro>> GetByPessoaAsync(int pessoaId)
    {
        return await _context.Set<ConsentimentoRegistro>()
            .Where(c => c.PessoaId == pessoaId)
            .OrderByDescending(c => c.AceitoEm)
            .ToListAsync();
    }
}
