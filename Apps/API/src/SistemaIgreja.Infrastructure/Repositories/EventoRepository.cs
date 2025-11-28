using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EventoRepository : IEventoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EventoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Evento>> GetAllAsync()
    {
        return await _context.Set<Evento>()
            .OrderBy(e => e.DataInicio)
            .ToListAsync();
    }

    public async Task<Evento?> GetByIdAsync(int id)
    {
        return await _context.Set<Evento>()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Evento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Set<Evento>()
            .Where(e => e.DataInicio <= dataFim && e.DataFim >= dataInicio)
            .OrderBy(e => e.DataInicio)
            .ToListAsync();
    }

    public async Task<Evento> CreateAsync(Evento evento)
    {
        _context.Set<Evento>().Add(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task<Evento> UpdateAsync(Evento evento)
    {
        _context.Set<Evento>().Update(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Evento>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Evento>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

