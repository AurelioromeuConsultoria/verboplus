using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CargoRepository : ICargoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CargoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cargo>> GetAllAsync()
    {
        return await _context.Set<Cargo>()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Cargo?> GetByIdAsync(int id)
    {
        return await _context.Set<Cargo>()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Cargo> CreateAsync(Cargo cargo)
    {
        _context.Set<Cargo>().Add(cargo);
        await _context.SaveChangesAsync();
        return cargo;
    }

    public async Task<Cargo> UpdateAsync(Cargo cargo)
    {
        _context.Set<Cargo>().Update(cargo);
        await _context.SaveChangesAsync();
        return cargo;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Cargo>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Cargo>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
