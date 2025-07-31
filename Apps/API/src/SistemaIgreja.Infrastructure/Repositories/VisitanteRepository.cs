using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class VisitanteRepository : IVisitanteRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public VisitanteRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Visitante>> GetAllAsync()
    {
        return await _context.Visitantes
            .OrderByDescending(v => v.DataCadastro)
            .ToListAsync();
    }

    public async Task<Visitante?> GetByIdAsync(int id)
    {
        return await _context.Visitantes
            .Include(v => v.MensagensAgendadas)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Visitante> CreateAsync(Visitante visitante)
    {
        _context.Visitantes.Add(visitante);
        await _context.SaveChangesAsync();
        return visitante;
    }

    public async Task<Visitante> UpdateAsync(Visitante visitante)
    {
        _context.Visitantes.Update(visitante);
        await _context.SaveChangesAsync();
        return visitante;
    }

    public async Task DeleteAsync(int id)
    {
        var visitante = await _context.Visitantes.FindAsync(id);
        if (visitante != null)
        {
            _context.Visitantes.Remove(visitante);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Visitante>> GetVisitantesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Visitantes
            .Where(v => v.DataVisita >= dataInicio && v.DataVisita <= dataFim)
            .OrderByDescending(v => v.DataVisita)
            .ToListAsync();
    }
}

