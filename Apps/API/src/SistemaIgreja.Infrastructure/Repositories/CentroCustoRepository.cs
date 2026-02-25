using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CentroCustoRepository : ICentroCustoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CentroCustoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CentroCusto>> GetAllAsync()
    {
        return await _context.Set<CentroCusto>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CentroCusto?> GetByIdAsync(int id)
    {
        return await _context.Set<CentroCusto>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CentroCusto> CreateAsync(CentroCusto centroCusto)
    {
        _context.Set<CentroCusto>().Add(centroCusto);
        await _context.SaveChangesAsync();
        return centroCusto;
    }

    public async Task<CentroCusto> UpdateAsync(CentroCusto centroCusto)
    {
        _context.Set<CentroCusto>().Update(centroCusto);
        await _context.SaveChangesAsync();
        return centroCusto;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<CentroCusto>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CentroCusto>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
