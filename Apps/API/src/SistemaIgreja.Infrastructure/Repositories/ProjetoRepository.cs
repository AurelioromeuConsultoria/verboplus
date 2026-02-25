using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ProjetoRepository : IProjetoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ProjetoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Projeto>> GetAllAsync()
    {
        return await _context.Set<Projeto>()
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Projeto?> GetByIdAsync(int id)
    {
        return await _context.Set<Projeto>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Projeto> CreateAsync(Projeto projeto)
    {
        _context.Set<Projeto>().Add(projeto);
        await _context.SaveChangesAsync();
        return projeto;
    }

    public async Task<Projeto> UpdateAsync(Projeto projeto)
    {
        _context.Set<Projeto>().Update(projeto);
        await _context.SaveChangesAsync();
        return projeto;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Projeto>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Projeto>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
