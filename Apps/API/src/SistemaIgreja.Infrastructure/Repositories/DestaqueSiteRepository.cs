using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class DestaqueSiteRepository : IDestaqueSiteRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public DestaqueSiteRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DestaqueSite>> GetAllAsync()
    {
        return await _context.Set<DestaqueSite>()
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync();
    }

    public async Task<DestaqueSite?> GetByIdAsync(int id)
    {
        return await _context.Set<DestaqueSite>()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DestaqueSite> CreateAsync(DestaqueSite destaqueSite)
    {
        _context.Set<DestaqueSite>().Add(destaqueSite);
        await _context.SaveChangesAsync();
        return destaqueSite;
    }

    public async Task<DestaqueSite> UpdateAsync(DestaqueSite destaqueSite)
    {
        _context.Set<DestaqueSite>().Update(destaqueSite);
        await _context.SaveChangesAsync();
        return destaqueSite;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<DestaqueSite>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<DestaqueSite>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}



