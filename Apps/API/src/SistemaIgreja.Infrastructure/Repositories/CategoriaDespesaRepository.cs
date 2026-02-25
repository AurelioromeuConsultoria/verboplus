using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CategoriaDespesaRepository : ICategoriaDespesaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CategoriaDespesaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaDespesa>> GetAllAsync()
    {
        return await _context.Set<CategoriaDespesa>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CategoriaDespesa?> GetByIdAsync(int id)
    {
        return await _context.Set<CategoriaDespesa>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CategoriaDespesa> CreateAsync(CategoriaDespesa categoriaDespesa)
    {
        _context.Set<CategoriaDespesa>().Add(categoriaDespesa);
        await _context.SaveChangesAsync();
        return categoriaDespesa;
    }

    public async Task<CategoriaDespesa> UpdateAsync(CategoriaDespesa categoriaDespesa)
    {
        _context.Set<CategoriaDespesa>().Update(categoriaDespesa);
        await _context.SaveChangesAsync();
        return categoriaDespesa;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<CategoriaDespesa>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CategoriaDespesa>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
