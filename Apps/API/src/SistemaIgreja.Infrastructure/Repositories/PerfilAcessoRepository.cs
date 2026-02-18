using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class PerfilAcessoRepository : IPerfilAcessoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public PerfilAcessoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PerfilAcesso>> GetAllAsync()
    {
        return await _context.Set<PerfilAcesso>()
            .Include(p => p.Permissoes)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<PerfilAcesso?> GetByIdAsync(int id)
    {
        return await _context.Set<PerfilAcesso>()
            .Include(p => p.Permissoes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PerfilAcesso> CreateAsync(PerfilAcesso perfil)
    {
        _context.Set<PerfilAcesso>().Add(perfil);
        await _context.SaveChangesAsync();
        return perfil;
    }

    public async Task<PerfilAcesso> UpdateAsync(PerfilAcesso perfil)
    {
        _context.Set<PerfilAcesso>().Update(perfil);
        await _context.SaveChangesAsync();
        return perfil;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<PerfilAcesso>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<PerfilAcesso>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
