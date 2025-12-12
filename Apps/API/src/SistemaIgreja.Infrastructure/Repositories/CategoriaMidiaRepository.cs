using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CategoriaMidiaRepository : ICategoriaMidiaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CategoriaMidiaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaMidia>> GetAllAsync()
    {
        return await _context.Set<CategoriaMidia>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CategoriaMidia?> GetByIdAsync(int id)
    {
        return await _context.Set<CategoriaMidia>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CategoriaMidia> CreateAsync(CategoriaMidia categoria)
    {
        _context.Set<CategoriaMidia>().Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<CategoriaMidia> UpdateAsync(CategoriaMidia categoria)
    {
        _context.Set<CategoriaMidia>().Update(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Set<CategoriaMidia>().FindAsync(id);
        if (entity == null) return false;

        _context.Set<CategoriaMidia>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}





