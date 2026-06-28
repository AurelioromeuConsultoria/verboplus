using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class HubCasaRepository : IHubCasaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public HubCasaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public HubCasaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<HubCasa>> GetAllAsync()
    {
        return await _context.Set<HubCasa>()
            .Include(c => c.AbertoPor).ThenInclude(u => u.Pessoa)
            .Include(c => c.Lider).ThenInclude(u => u.Pessoa)
            .Include(c => c.Timoteo).ThenInclude(u => u.Pessoa)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<HubCasa?> GetByIdAsync(int id)
    {
        return await _context.Set<HubCasa>()
            .Include(c => c.AbertoPor).ThenInclude(u => u.Pessoa)
            .Include(c => c.Lider).ThenInclude(u => u.Pessoa)
            .Include(c => c.Timoteo).ThenInclude(u => u.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<HubCasa> CreateAsync(HubCasa casa)
    {
        casa.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<HubCasa>().Add(casa);
        await _context.SaveChangesAsync();
        return casa;
    }

    public async Task<HubCasa> UpdateAsync(HubCasa casa)
    {
        _context.Set<HubCasa>().Update(casa);
        await _context.SaveChangesAsync();
        return casa;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<HubCasa>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<HubCasa>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
