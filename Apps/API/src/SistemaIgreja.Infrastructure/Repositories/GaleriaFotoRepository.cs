using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class GaleriaFotoRepository : IGaleriaFotoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public GaleriaFotoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GaleriaFoto>> GetAllAsync()
    {
        return await _context.Set<GaleriaFoto>()
            .Include(g => g.Evento)
            .Include(g => g.CategoriaMidia)
            .OrderByDescending(g => g.Data)
            .ToListAsync();
    }

    public async Task<IEnumerable<GaleriaFoto>> GetAtivasAsync()
    {
        return await _context.Set<GaleriaFoto>()
            .Include(g => g.Evento)
            .Include(g => g.CategoriaMidia)
            .Where(g => g.Ativo)
            .OrderByDescending(g => g.Data)
            .ToListAsync();
    }

    public async Task<GaleriaFoto?> GetByIdAsync(int id)
    {
        return await _context.Set<GaleriaFoto>()
            .Include(g => g.Evento)
            .Include(g => g.CategoriaMidia)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<GaleriaFoto>> GetByEventoIdAsync(int eventoId)
    {
        return await _context.Set<GaleriaFoto>()
            .Include(g => g.Evento)
            .Include(g => g.CategoriaMidia)
            .Where(g => g.EventoId == eventoId)
            .OrderByDescending(g => g.Data)
            .ToListAsync();
    }

    public async Task<IEnumerable<GaleriaFoto>> GetByCategoriaMidiaIdAsync(int categoriaMidiaId)
    {
        return await _context.Set<GaleriaFoto>()
            .Include(g => g.Evento)
            .Include(g => g.CategoriaMidia)
            .Where(g => g.CategoriaMidiaId == categoriaMidiaId)
            .OrderByDescending(g => g.Data)
            .ToListAsync();
    }

    public async Task<GaleriaFoto> CreateAsync(GaleriaFoto galeria)
    {
        _context.Set<GaleriaFoto>().Add(galeria);
        await _context.SaveChangesAsync();
        return galeria;
    }

    public async Task<GaleriaFoto> UpdateAsync(GaleriaFoto galeria)
    {
        _context.Set<GaleriaFoto>().Update(galeria);
        await _context.SaveChangesAsync();
        return galeria;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Set<GaleriaFoto>().FindAsync(id);
        if (entity == null) return false;

        _context.Set<GaleriaFoto>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}

