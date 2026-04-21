using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public FornecedorRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public FornecedorRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
    {
        return await _context.Set<Fornecedor>()
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }

    public async Task<Fornecedor?> GetByIdAsync(int id)
    {
        return await _context.Set<Fornecedor>()
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fornecedor> CreateAsync(Fornecedor fornecedor)
    {
        fornecedor.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<Fornecedor>().Add(fornecedor);
        await _context.SaveChangesAsync();
        return fornecedor;
    }

    public async Task<Fornecedor> UpdateAsync(Fornecedor fornecedor)
    {
        _context.Set<Fornecedor>().Update(fornecedor);
        await _context.SaveChangesAsync();
        return fornecedor;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Fornecedor>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Fornecedor>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
