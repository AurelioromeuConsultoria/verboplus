using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EventoRecorrenciaRepository : IEventoRecorrenciaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public EventoRecorrenciaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public EventoRecorrenciaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<EventoRecorrencia>> GetByEventoAsync(int eventoId)
    {
        return await _context.EventosRecorrencias
            .Where(r => r.EventoId == eventoId)
            .OrderBy(r => r.DiaSemana)
            .ThenBy(r => r.HoraInicio)
            .ToListAsync();
    }

    public async Task<EventoRecorrencia?> GetByIdAsync(int id)
    {
        return await _context.EventosRecorrencias.FindAsync(id);
    }

    public async Task<EventoRecorrencia> CreateAsync(EventoRecorrencia recorrencia)
    {
        recorrencia.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.EventosRecorrencias.Add(recorrencia);
        await _context.SaveChangesAsync();
        return recorrencia;
    }

    public async Task<EventoRecorrencia> UpdateAsync(EventoRecorrencia recorrencia)
    {
        _context.EventosRecorrencias.Update(recorrencia);
        await _context.SaveChangesAsync();
        return recorrencia;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.EventosRecorrencias.FindAsync(id);
        if (entity != null)
        {
            _context.EventosRecorrencias.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
