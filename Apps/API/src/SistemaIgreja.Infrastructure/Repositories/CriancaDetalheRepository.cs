using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class CriancaDetalheRepository : ICriancaDetalheRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public CriancaDetalheRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<CriancaDetalhe?> GetByPessoaIdAsync(int pessoaId)
    {
        return await _context.Set<CriancaDetalhe>()
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.PessoaId == pessoaId);
    }

    public async Task<CriancaDetalhe> CreateAsync(CriancaDetalhe detalhe)
    {
        _context.Set<CriancaDetalhe>().Add(detalhe);
        await _context.SaveChangesAsync();
        return detalhe;
    }

    public Task<CriancaDetalhe> CreateWithoutSaveAsync(CriancaDetalhe detalhe)
    {
        _context.Set<CriancaDetalhe>().Add(detalhe);
        return Task.FromResult(detalhe);
    }

    public async Task<CriancaDetalhe> UpdateAsync(CriancaDetalhe detalhe)
    {
        _context.Set<CriancaDetalhe>().Update(detalhe);
        await _context.SaveChangesAsync();
        return detalhe;
    }

    public async Task DeleteAsync(int pessoaId)
    {
        var detalhe = await _context.Set<CriancaDetalhe>()
            .FirstOrDefaultAsync(c => c.PessoaId == pessoaId);
        if (detalhe != null)
        {
            _context.Set<CriancaDetalhe>().Remove(detalhe);
            await _context.SaveChangesAsync();
        }
    }
}


