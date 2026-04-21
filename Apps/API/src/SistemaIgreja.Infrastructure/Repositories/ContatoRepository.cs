using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ContatoRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public ContatoRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<Contato>> GetAllAsync()
    {
        return await _context.Set<Contato>()
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<Contato?> GetByIdAsync(int id)
    {
        return await _context.Set<Contato>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contato> CreateAsync(Contato contato)
    {
        contato.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<Contato>().Add(contato);
        await _context.SaveChangesAsync();
        return contato;
    }

    public async Task<Contato> UpdateAsync(Contato contato)
    {
        _context.Set<Contato>().Update(contato);
        await _context.SaveChangesAsync();
        return contato;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Contato>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Contato>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}





