using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ResponsavelCriancaRepository : IResponsavelCriancaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ResponsavelCriancaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ResponsavelCrianca>> GetByCriancaIdAsync(int criancaPessoaId)
    {
        return await _context.Set<ResponsavelCrianca>()
            .Include(r => r.Responsavel)
            .Where(r => r.CriancaPessoaId == criancaPessoaId && r.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponsavelCrianca>> GetByResponsavelIdAsync(int responsavelPessoaId)
    {
        return await _context.Set<ResponsavelCrianca>()
            .Include(r => r.Crianca)
            .Where(r => r.ResponsavelPessoaId == responsavelPessoaId && r.Ativo)
            .ToListAsync();
    }

    public async Task<ResponsavelCrianca?> GetByIdAsync(int id)
    {
        return await _context.Set<ResponsavelCrianca>()
            .Include(r => r.Crianca)
            .Include(r => r.Responsavel)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ResponsavelCrianca?> GetByCriancaAndResponsavelAsync(int criancaPessoaId, int responsavelPessoaId)
    {
        return await _context.Set<ResponsavelCrianca>()
            .FirstOrDefaultAsync(r => r.CriancaPessoaId == criancaPessoaId && 
                                      r.ResponsavelPessoaId == responsavelPessoaId);
    }

    public async Task<ResponsavelCrianca> CreateAsync(ResponsavelCrianca responsavel)
    {
        _context.Set<ResponsavelCrianca>().Add(responsavel);
        await _context.SaveChangesAsync();
        return responsavel;
    }

    public Task<ResponsavelCrianca> CreateWithoutSaveAsync(ResponsavelCrianca responsavel)
    {
        _context.Set<ResponsavelCrianca>().Add(responsavel);
        return Task.FromResult(responsavel);
    }

    public async Task<ResponsavelCrianca> UpdateAsync(ResponsavelCrianca responsavel)
    {
        _context.Set<ResponsavelCrianca>().Update(responsavel);
        await _context.SaveChangesAsync();
        return responsavel;
    }

    public async Task DeleteAsync(int id)
    {
        var responsavel = await _context.Set<ResponsavelCrianca>().FindAsync(id);
        if (responsavel != null)
        {
            responsavel.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> PodeRetirarAsync(int criancaPessoaId, int responsavelPessoaId)
    {
        var responsavel = await _context.Set<ResponsavelCrianca>()
            .FirstOrDefaultAsync(r => r.CriancaPessoaId == criancaPessoaId && 
                                      r.ResponsavelPessoaId == responsavelPessoaId && 
                                      r.Ativo);
        return responsavel?.PodeRetirar ?? false;
    }
}


