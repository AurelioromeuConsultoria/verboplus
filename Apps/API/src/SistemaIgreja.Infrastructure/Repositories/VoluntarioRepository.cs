using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class VoluntarioRepository : IVoluntarioRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public VoluntarioRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Voluntario>> GetAllAsync()
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .OrderBy(v => v.Nome)
            .ToListAsync();
    }

    public async Task<Voluntario?> GetByIdAsync(int id)
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Voluntario> CreateAsync(Voluntario voluntario)
    {
        _context.Set<Voluntario>().Add(voluntario);
        await _context.SaveChangesAsync();
        return voluntario;
    }

    public async Task<Voluntario> UpdateAsync(Voluntario voluntario)
    {
        _context.Set<Voluntario>().Update(voluntario);
        await _context.SaveChangesAsync();
        return voluntario;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Voluntario>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Voluntario>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
