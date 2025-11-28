using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class NoticiaRepository : INoticiaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public NoticiaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Noticia>> GetAllAsync()
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .OrderByDescending(n => n.Data)
            .ToListAsync();
    }

    public async Task<Noticia?> GetByIdAsync(int id)
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Noticia>> GetByCategoriaAsync(int categoriaId)
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .Where(n => n.CategoriaNoticiaId == categoriaId)
            .OrderByDescending(n => n.Data)
            .ToListAsync();
    }

    public async Task<Noticia> CreateAsync(Noticia noticia)
    {
        _context.Set<Noticia>().Add(noticia);
        await _context.SaveChangesAsync();
        return noticia;
    }

    public async Task<Noticia> UpdateAsync(Noticia noticia)
    {
        _context.Set<Noticia>().Update(noticia);
        await _context.SaveChangesAsync();
        return noticia;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Noticia>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Noticia>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}



