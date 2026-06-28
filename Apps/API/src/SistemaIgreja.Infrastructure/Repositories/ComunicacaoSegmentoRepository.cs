using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ComunicacaoSegmentoRepository : IComunicacaoSegmentoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ComunicacaoSegmentoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ComunicacaoSegmento>> GetAllAsync()
    {
        return await _context.ComunicacaoSegmentos
            .OrderByDescending(x => x.Padrao)
            .ThenBy(x => x.Nome)
            .ToListAsync();
    }

    public Task<ComunicacaoSegmento?> GetByIdAsync(int id)
    {
        return _context.ComunicacaoSegmentos.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ComunicacaoSegmento> CreateAsync(ComunicacaoSegmento segmento)
    {
        _context.ComunicacaoSegmentos.Add(segmento);
        await _context.SaveChangesAsync();
        return segmento;
    }

    public async Task<ComunicacaoSegmento> UpdateAsync(ComunicacaoSegmento segmento)
    {
        _context.ComunicacaoSegmentos.Update(segmento);
        await _context.SaveChangesAsync();
        return segmento;
    }
}
