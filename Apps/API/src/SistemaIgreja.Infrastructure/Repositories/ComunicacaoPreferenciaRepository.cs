using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ComunicacaoPreferenciaRepository : IComunicacaoPreferenciaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ComunicacaoPreferenciaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ComunicacaoPreferencia>> GetByPessoaIdAsync(int pessoaId)
    {
        return await _context.ComunicacaoPreferencias
            .Where(x => x.PessoaId == pessoaId)
            .OrderBy(x => x.Canal)
            .ToListAsync();
    }

    public Task<ComunicacaoPreferencia?> GetByPessoaCanalAsync(int pessoaId, CanalComunicacao canal)
    {
        return _context.ComunicacaoPreferencias
            .FirstOrDefaultAsync(x => x.PessoaId == pessoaId && x.Canal == canal);
    }

    public async Task<ComunicacaoPreferencia> CreateAsync(ComunicacaoPreferencia preferencia)
    {
        _context.ComunicacaoPreferencias.Add(preferencia);
        await _context.SaveChangesAsync();
        return preferencia;
    }

    public async Task<ComunicacaoPreferencia> UpdateAsync(ComunicacaoPreferencia preferencia)
    {
        _context.ComunicacaoPreferencias.Update(preferencia);
        await _context.SaveChangesAsync();
        return preferencia;
    }
}
