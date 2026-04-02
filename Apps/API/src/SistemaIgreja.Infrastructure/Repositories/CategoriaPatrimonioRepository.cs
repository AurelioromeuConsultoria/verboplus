using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CategoriaPatrimonioRepository : ICategoriaPatrimonioRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CategoriaPatrimonioRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaPatrimonio>> GetAllAsync()
    {
        return await _context.Set<CategoriaPatrimonio>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CategoriaPatrimonio?> GetByIdAsync(int id)
    {
        return await _context.Set<CategoriaPatrimonio>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CategoriaPatrimonio?> GetByNomeAsync(string nome)
    {
        return await _context.Set<CategoriaPatrimonio>()
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nome.ToLower());
    }

    public async Task<CategoriaPatrimonio> CreateAsync(CategoriaPatrimonio categoria)
    {
        _context.Set<CategoriaPatrimonio>().Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<CategoriaPatrimonio> UpdateAsync(CategoriaPatrimonio categoria)
    {
        _context.Set<CategoriaPatrimonio>().Update(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<CategoriaPatrimonio>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CategoriaPatrimonio>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
