using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CategoriaReceitaRepository : ICategoriaReceitaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CategoriaReceitaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public CategoriaReceitaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<IEnumerable<CategoriaReceita>> GetAllAsync()
    {
        return await _context.Set<CategoriaReceita>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CategoriaReceita?> GetByIdAsync(int id)
    {
        return await _context.Set<CategoriaReceita>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CategoriaReceita> CreateAsync(CategoriaReceita categoriaReceita)
    {
        categoriaReceita.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<CategoriaReceita>().Add(categoriaReceita);
        await _context.SaveChangesAsync();
        return categoriaReceita;
    }

    public async Task<CategoriaReceita> UpdateAsync(CategoriaReceita categoriaReceita)
    {
        _context.Set<CategoriaReceita>().Update(categoriaReceita);
        await _context.SaveChangesAsync();
        return categoriaReceita;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<CategoriaReceita>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CategoriaReceita>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
