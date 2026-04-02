using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EnvioCampanhaAniversarioRepository : IEnvioCampanhaAniversarioRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EnvioCampanhaAniversarioRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<EnvioCampanhaAniversario?> GetByIdAsync(int id)
    {
        return await _context.EnviosCampanhaAniversario
            .Include(x => x.Pessoa)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EnvioCampanhaAniversario?> GetByPessoaAnoAsync(int pessoaId, int anoReferencia)
    {
        return await _context.EnviosCampanhaAniversario
            .Include(x => x.Pessoa)
            .FirstOrDefaultAsync(x => x.PessoaId == pessoaId && x.AnoReferencia == anoReferencia);
    }

    public async Task<EnvioCampanhaAniversario> CreateAsync(EnvioCampanhaAniversario envio)
    {
        _context.EnviosCampanhaAniversario.Add(envio);
        await _context.SaveChangesAsync();
        return envio;
    }

    public async Task<EnvioCampanhaAniversario> UpdateAsync(EnvioCampanhaAniversario envio)
    {
        _context.EnviosCampanhaAniversario.Update(envio);
        await _context.SaveChangesAsync();
        return envio;
    }

    public async Task<IReadOnlyList<EnvioCampanhaAniversario>> GetRecentesAsync(int limit)
    {
        return await _context.EnviosCampanhaAniversario
            .AsNoTracking()
            .Include(x => x.Pessoa)
            .OrderByDescending(x => x.DataUltimaTentativa ?? x.DataCriacao)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EnvioCampanhaAniversario>> GetHistoricoAsync(string? busca, string? status, int limit)
    {
        var query = _context.EnviosCampanhaAniversario
            .AsNoTracking()
            .Include(x => x.Pessoa)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaNormalizada = busca.Trim().ToLower();
            query = query.Where(x =>
                x.Pessoa.Nome.ToLower().Contains(buscaNormalizada) ||
                (x.WhatsAppUtilizado != null && x.WhatsAppUtilizado.Contains(busca.Trim())));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<StatusEnvioCampanhaAniversario>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        return await query
            .OrderByDescending(x => x.DataUltimaTentativa ?? x.DataCriacao)
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.EnviosCampanhaAniversario.CountAsync();
    }

    public async Task<int> CountByStatusAnoAsync(StatusEnvioCampanhaAniversario status, int anoReferencia)
    {
        return await _context.EnviosCampanhaAniversario
            .CountAsync(x => x.AnoReferencia == anoReferencia && x.Status == status);
    }

    public async Task<int> CountByStatusDataAsync(StatusEnvioCampanhaAniversario status, DateTime dataReferencia)
    {
        return await _context.EnviosCampanhaAniversario
            .CountAsync(x => x.DataAniversario.Date == dataReferencia.Date && x.Status == status);
    }

    public async Task<int> CountPendentesAnoAsync(int anoReferencia)
    {
        return await _context.EnviosCampanhaAniversario
            .CountAsync(x => x.AnoReferencia == anoReferencia &&
                             (x.Status == StatusEnvioCampanhaAniversario.Pendente ||
                              x.Status == StatusEnvioCampanhaAniversario.EmProcessamento));
    }
}
