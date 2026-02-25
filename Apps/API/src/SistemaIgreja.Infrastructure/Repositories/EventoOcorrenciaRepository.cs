using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EventoOcorrenciaRepository : IEventoOcorrenciaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EventoOcorrenciaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventoOcorrencia>> GetByEventoAsync(int eventoId)
    {
        return await _context.EventosOcorrencias
            .Include(o => o.Evento)
            .Include(o => o.Escala)
            .Where(o => o.EventoId == eventoId)
            .OrderBy(o => o.DataHoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<EventoOcorrencia>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null)
    {
        var query = _context.EventosOcorrencias
            .Include(o => o.Evento)
            .Include(o => o.Escala)
            .Where(o => o.DataHoraInicio >= dataInicio && o.DataHoraInicio <= dataFim);

        if (eventoId.HasValue)
        {
            query = query.Where(o => o.EventoId == eventoId.Value);
        }

        return await query
            .OrderBy(o => o.DataHoraInicio)
            .ToListAsync();
    }

    public async Task<EventoOcorrencia?> GetByIdAsync(int id)
    {
        return await _context.EventosOcorrencias
            .Include(o => o.Evento)
            .Include(o => o.Escala)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.EventosOcorrencias.AnyAsync(o => o.Id == id);
    }

    public async Task<EventoOcorrencia> CreateAsync(EventoOcorrencia ocorrencia)
    {
        _context.EventosOcorrencias.Add(ocorrencia);
        await _context.SaveChangesAsync();
        return ocorrencia;
    }

    public async Task<EventoOcorrencia> UpdateAsync(EventoOcorrencia ocorrencia)
    {
        _context.EventosOcorrencias.Update(ocorrencia);
        await _context.SaveChangesAsync();
        return ocorrencia;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.EventosOcorrencias.FindAsync(id);
        if (entity == null) return;

        _context.EventosOcorrencias.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<EventoRecorrencia>> GetRecorrenciasAtivasByEventoAsync(int eventoId)
    {
        return await _context.EventosRecorrencias
            .Where(r => r.EventoId == eventoId && r.Ativo)
            .OrderBy(r => r.DiaSemana)
            .ThenBy(r => r.HoraInicio)
            .ToListAsync();
    }

    public async Task<bool> ExistsOcorrenciaNoHorarioAsync(int eventoId, DateTime dataHoraInicio)
    {
        return await _context.EventosOcorrencias.AnyAsync(o =>
            o.EventoId == eventoId &&
            o.DataHoraInicio == dataHoraInicio);
    }
}
