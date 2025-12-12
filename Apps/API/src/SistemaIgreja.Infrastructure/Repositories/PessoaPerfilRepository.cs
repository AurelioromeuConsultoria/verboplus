using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class PessoaPerfilRepository : IPessoaPerfilRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public PessoaPerfilRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<PessoaPerfil?> GetPerfilAtivoAsync(int pessoaId, PerfilPessoa perfil)
    {
        return await _context.Set<PessoaPerfil>()
            .FirstOrDefaultAsync(p => p.PessoaId == pessoaId 
                && p.Perfil == perfil 
                && p.DataFim == null);
    }

    public async Task<PessoaPerfil> CreateAsync(PessoaPerfil pessoaPerfil)
    {
        _context.Set<PessoaPerfil>().Add(pessoaPerfil);
        await _context.SaveChangesAsync();
        return pessoaPerfil;
    }

    public Task<PessoaPerfil> CreateWithoutSaveAsync(PessoaPerfil pessoaPerfil)
    {
        _context.Set<PessoaPerfil>().Add(pessoaPerfil);
        return Task.FromResult(pessoaPerfil);
    }

    public async Task<IEnumerable<PessoaPerfil>> GetAllAsync()
    {
        return await _context.Set<PessoaPerfil>()
            .Include(p => p.Pessoa)
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();
    }

    public async Task<PessoaPerfil?> GetByIdAsync(int id)
    {
        return await _context.Set<PessoaPerfil>()
            .Include(p => p.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PessoaPerfil> UpdateAsync(PessoaPerfil pessoaPerfil)
    {
        _context.Set<PessoaPerfil>().Update(pessoaPerfil);
        await _context.SaveChangesAsync();
        return pessoaPerfil;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<PessoaPerfil>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<PessoaPerfil>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PessoaPerfil>> GetPerfisPorPessoaAsync(int pessoaId)
    {
        return await _context.Set<PessoaPerfil>()
            .Include(p => p.Pessoa)
            .Where(p => p.PessoaId == pessoaId)
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();
    }
}



