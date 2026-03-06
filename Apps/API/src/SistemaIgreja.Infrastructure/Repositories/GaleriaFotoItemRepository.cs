using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class GaleriaFotoItemRepository : IGaleriaFotoItemRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public GaleriaFotoItemRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<List<GaleriaFotoItem>> GetByGaleriaIdAsync(int galeriaId)
    {
        return await _context.Set<GaleriaFotoItem>()
            .Where(i => i.GaleriaFotoId == galeriaId)
            .OrderBy(i => i.Ordem)
            .ThenBy(i => i.NomeArquivo)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<GaleriaFotoItem> items)
    {
        await _context.Set<GaleriaFotoItem>().AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task SetDestaqueAsync(int galeriaId, string nomeArquivoDestaque)
    {
        var itens = await _context.Set<GaleriaFotoItem>()
            .Where(i => i.GaleriaFotoId == galeriaId)
            .ToListAsync();
        foreach (var item in itens)
        {
            item.Destaque = string.Equals(item.NomeArquivo, nomeArquivoDestaque, StringComparison.OrdinalIgnoreCase);
        }
        await _context.SaveChangesAsync();
    }
}
