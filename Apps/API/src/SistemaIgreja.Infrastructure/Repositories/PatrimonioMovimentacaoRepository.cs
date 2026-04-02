using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class PatrimonioMovimentacaoRepository : IPatrimonioMovimentacaoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public PatrimonioMovimentacaoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatrimonioMovimentacao>> GetByPatrimonioIdAsync(int patrimonioItemId)
    {
        return await _context.Set<PatrimonioMovimentacao>()
            .Where(m => m.PatrimonioItemId == patrimonioItemId)
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<PatrimonioMovimentacao> CreateAsync(PatrimonioMovimentacao movimentacao)
    {
        _context.Set<PatrimonioMovimentacao>().Add(movimentacao);
        await _context.SaveChangesAsync();
        return movimentacao;
    }
}
