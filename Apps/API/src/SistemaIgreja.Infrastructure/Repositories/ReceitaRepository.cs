using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ReceitaRepository : IReceitaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ReceitaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public ReceitaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<IEnumerable<Receita>> GetAllAsync()
    {
        return await _context.Set<Receita>()
            .Include(r => r.CategoriaReceita)
            .Include(r => r.ContaBancaria)
            .Include(r => r.CentroCusto)
            .Include(r => r.Projeto)
            .Include(r => r.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .OrderByDescending(r => r.DataRecebimento)
            .ToListAsync();
    }

    public async Task<Receita?> GetByIdAsync(int id)
    {
        return await _context.Set<Receita>()
            .Include(r => r.CategoriaReceita)
            .Include(r => r.ContaBancaria)
            .Include(r => r.CentroCusto)
            .Include(r => r.Projeto)
            .Include(r => r.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Receita> CreateAsync(Receita receita)
    {
        receita.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<Receita>().Add(receita);
        await _context.SaveChangesAsync();
        return receita;
    }

    public async Task<Receita> UpdateAsync(Receita receita)
    {
        _context.Set<Receita>().Update(receita);
        await _context.SaveChangesAsync();
        return receita;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Receita>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Receita>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
