using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ComunicacaoTemplateRepository : IComunicacaoTemplateRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ComunicacaoTemplateRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ComunicacaoTemplate>> GetAllAsync()
    {
        return await _context.ComunicacaoTemplates
            .AsNoTracking()
            .OrderBy(t => t.Nome)
            .ToListAsync();
    }

    public async Task<ComunicacaoTemplate?> GetByIdAsync(int id)
    {
        return await _context.ComunicacaoTemplates
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<ComunicacaoTemplate> CreateAsync(ComunicacaoTemplate template)
    {
        _context.ComunicacaoTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<ComunicacaoTemplate> UpdateAsync(ComunicacaoTemplate template)
    {
        _context.ComunicacaoTemplates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }
}
