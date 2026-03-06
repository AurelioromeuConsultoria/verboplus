using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EscalaModeloRepository : IEscalaModeloRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EscalaModeloRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<EscalaModelo?> GetByIdAsync(int id)
    {
        return await _context.EscalasModelos
            .Include(m => m.Evento)
            .Include(m => m.Equipe)
            .Include(m => m.Itens)
                .ThenInclude(i => i.Cargo)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<EscalaModelo?> GetByEventoAndEquipeAsync(int? eventoId, int equipeId)
    {
        var modelo = eventoId.HasValue
            ? await _context.EscalasModelos
                .Include(m => m.Evento)
                .Include(m => m.Equipe)
                .Include(m => m.Itens)
                    .ThenInclude(i => i.Cargo)
                .FirstOrDefaultAsync(m => m.EventoId == eventoId && m.EquipeId == equipeId && m.Ativo)
            : null;

        if (modelo != null) return modelo;

        return await _context.EscalasModelos
            .Include(m => m.Evento)
            .Include(m => m.Equipe)
            .Include(m => m.Itens)
                .ThenInclude(i => i.Cargo)
            .FirstOrDefaultAsync(m => m.EventoId == null && m.EquipeId == equipeId && m.Ativo);
    }

    public async Task<IEnumerable<EscalaModelo>> GetByEquipeAsync(int equipeId)
    {
        return await _context.EscalasModelos
            .Include(m => m.Evento)
            .Include(m => m.Equipe)
            .Include(m => m.Itens)
                .ThenInclude(i => i.Cargo)
            .Where(m => m.EquipeId == equipeId)
            .OrderBy(m => m.EventoId == null ? 0 : 1)
            .ThenBy(m => m.Evento!.Titulo)
            .ToListAsync();
    }

    public async Task<IEnumerable<EscalaModelo>> GetByEventoAsync(int eventoId)
    {
        return await _context.EscalasModelos
            .Include(m => m.Evento)
            .Include(m => m.Equipe)
            .Include(m => m.Itens)
                .ThenInclude(i => i.Cargo)
            .Where(m => m.EventoId == eventoId)
            .OrderBy(m => m.Equipe!.Nome)
            .ToListAsync();
    }

    public async Task<EscalaModelo> CreateAsync(EscalaModelo modelo)
    {
        _context.EscalasModelos.Add(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task<EscalaModelo> UpdateAsync(EscalaModelo modelo)
    {
        _context.EscalasModelos.Update(modelo);
        await _context.SaveChangesAsync();
        return modelo;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.EscalasModelos.FindAsync(id);
        if (entity == null) return;
        _context.EscalasModelos.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
