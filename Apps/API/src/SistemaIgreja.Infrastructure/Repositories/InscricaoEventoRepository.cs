using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class InscricaoEventoRepository : IInscricaoEventoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public InscricaoEventoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InscricaoEvento>> GetAllAsync()
    {
        return await _context.Set<InscricaoEvento>()
            .Include(i => i.Evento)
            .OrderByDescending(i => i.DataInscricao)
            .ToListAsync();
    }

    public async Task<InscricaoEvento?> GetByIdAsync(int id)
    {
        return await _context.Set<InscricaoEvento>()
            .Include(i => i.Evento)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<InscricaoEvento>> GetByEventoAsync(int eventoId)
    {
        return await _context.Set<InscricaoEvento>()
            .Include(i => i.Evento)
            .Where(i => i.EventoId == eventoId)
            .OrderByDescending(i => i.DataInscricao)
            .ToListAsync();
    }

    public async Task<IEnumerable<InscricaoEvento>> GetByStatusAsync(StatusInscricao status)
    {
        return await _context.Set<InscricaoEvento>()
            .Include(i => i.Evento)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.DataInscricao)
            .ToListAsync();
    }

    public async Task<int> ContarInscricoesPorEventoAsync(int eventoId)
    {
        return await _context.Set<InscricaoEvento>()
            .CountAsync(i => i.EventoId == eventoId);
    }

    public async Task<int> ContarInscricoesConfirmadasPorEventoAsync(int eventoId)
    {
        return await _context.Set<InscricaoEvento>()
            .CountAsync(i => i.EventoId == eventoId && i.Status == StatusInscricao.Confirmada);
    }

    public async Task<bool> ExisteInscricaoAsync(int eventoId, string whatsApp)
    {
        return await _context.Set<InscricaoEvento>()
            .AnyAsync(i => i.EventoId == eventoId && i.WhatsApp == whatsApp);
    }

    public async Task<InscricaoEvento> CreateAsync(InscricaoEvento inscricaoEvento)
    {
        _context.Set<InscricaoEvento>().Add(inscricaoEvento);
        await _context.SaveChangesAsync();
        return inscricaoEvento;
    }

    public async Task<InscricaoEvento> UpdateAsync(InscricaoEvento inscricaoEvento)
    {
        _context.Set<InscricaoEvento>().Update(inscricaoEvento);
        await _context.SaveChangesAsync();
        return inscricaoEvento;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<InscricaoEvento>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<InscricaoEvento>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

