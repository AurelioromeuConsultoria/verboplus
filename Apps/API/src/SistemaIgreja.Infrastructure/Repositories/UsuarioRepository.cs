using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public UsuarioRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Set<Usuario>()
            .Include(u => u.Pessoa)
            .Include(u => u.PerfilAcesso)
                .ThenInclude(p => p.Permissoes)
            .OrderBy(u => u.Pessoa.Nome)
            .ToListAsync();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.Pessoa)
            .Include(u => u.PerfilAcesso)
                .ThenInclude(p => p.Permissoes)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Set<Usuario>()
            .Include(u => u.Pessoa)
            .Include(u => u.PerfilAcesso)
                .ThenInclude(p => p.Permissoes)
            .FirstOrDefaultAsync(u => u.EmailLogin.ToLower() == email.ToLower());
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        _context.Set<Usuario>().Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        _context.Set<Usuario>().Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Usuario>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Usuario>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteAlgumUsuarioAsync()
    {
        return await _context.Set<Usuario>().AnyAsync();
    }
}
