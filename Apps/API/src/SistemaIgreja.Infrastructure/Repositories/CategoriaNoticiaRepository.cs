using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CategoriaNoticiaRepository : ICategoriaNoticiaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CategoriaNoticiaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaNoticia>> GetAllAsync()
    {
        return await _context.Set<CategoriaNoticia>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CategoriaNoticia?> GetByIdAsync(int id)
    {
        return await _context.Set<CategoriaNoticia>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CategoriaNoticia> CreateAsync(CategoriaNoticia categoriaNoticia)
    {
        _context.Set<CategoriaNoticia>().Add(categoriaNoticia);
        await _context.SaveChangesAsync();
        return categoriaNoticia;
    }

    public async Task<CategoriaNoticia> UpdateAsync(CategoriaNoticia categoriaNoticia)
    {
        _context.Set<CategoriaNoticia>().Update(categoriaNoticia);
        await _context.SaveChangesAsync();
        return categoriaNoticia;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<CategoriaNoticia>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<CategoriaNoticia>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}



