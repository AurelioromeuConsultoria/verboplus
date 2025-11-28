using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EquipeRepository : IEquipeRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EquipeRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Equipe>> GetAllAsync()
    {
        return await _context.Set<Equipe>()
            .Include(e => e.Voluntarios)
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }

    public async Task<Equipe?> GetByIdAsync(int id)
    {
        return await _context.Set<Equipe>()
            .Include(e => e.Voluntarios)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Equipe> CreateAsync(Equipe equipe)
    {
        _context.Set<Equipe>().Add(equipe);
        await _context.SaveChangesAsync();
        return equipe;
    }

    public async Task<Equipe> UpdateAsync(Equipe equipe)
    {
        _context.Set<Equipe>().Update(equipe);
        await _context.SaveChangesAsync();
        return equipe;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Equipe>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Equipe>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
