using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class KidsNotificacaoRepository : IKidsNotificacaoRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public KidsNotificacaoRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<KidsNotificacao?> GetByIdAsync(int id)
    {
        return await _context.Set<KidsNotificacao>()
            .Include(n => n.Crianca)
            .Include(n => n.Responsavel)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<KidsNotificacao>> GetByCriancaIdAsync(int criancaPessoaId)
    {
        return await _context.Set<KidsNotificacao>()
            .Include(n => n.Responsavel)
            .Where(n => n.CriancaPessoaId == criancaPessoaId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<KidsNotificacao>> GetByResponsavelIdAsync(int responsavelPessoaId)
    {
        return await _context.Set<KidsNotificacao>()
            .Include(n => n.Crianca)
            .Where(n => n.ResponsavelPessoaId == responsavelPessoaId)
            .OrderByDescending(n => n.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<KidsNotificacao>> GetFeedByResponsavelIdAsync(int responsavelPessoaId, bool somenteNaoLidos = false, string? tipo = null, int? criancaPessoaId = null, int? limit = null)
    {
        IQueryable<KidsNotificacao> query = _context.Set<KidsNotificacao>()
            .Include(n => n.Crianca)
            .Where(n => n.ResponsavelPessoaId == responsavelPessoaId);

        if (somenteNaoLidos)
        {
            query = query.Where(n => !n.LidoEm.HasValue);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            query = query.Where(n => n.Tipo == tipoNormalizado);
        }

        if (criancaPessoaId.HasValue)
        {
            query = query.Where(n => n.CriancaPessoaId == criancaPessoaId.Value);
        }

        query = query.OrderByDescending(n => n.DataCriacao);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<KidsNotificacao>> GetAdministrativosAsync(string? tipo = null, int? responsavelPessoaId = null, int? criancaPessoaId = null, int? limit = null)
    {
        IQueryable<KidsNotificacao> query = _context.Set<KidsNotificacao>()
            .Include(n => n.Crianca)
            .Include(n => n.Responsavel)
            .Where(n => n.Origem == "MANUAL");

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            query = query.Where(n => n.Tipo == tipoNormalizado);
        }

        if (responsavelPessoaId.HasValue)
        {
            query = query.Where(n => n.ResponsavelPessoaId == responsavelPessoaId.Value);
        }

        if (criancaPessoaId.HasValue)
        {
            query = query.Where(n => n.CriancaPessoaId == criancaPessoaId.Value);
        }

        query = query.OrderByDescending(n => n.DataCriacao);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<KidsNotificacao>> GetPendentesAsync()
    {
        return await _context.Set<KidsNotificacao>()
            .Include(n => n.Crianca)
            .Include(n => n.Responsavel)
            .Where(n => n.Status == "Pendente")
            .OrderBy(n => n.DataCriacao)
            .ToListAsync();
    }

    public async Task<KidsNotificacao> CreateAsync(KidsNotificacao notificacao)
    {
        _context.Set<KidsNotificacao>().Add(notificacao);
        await _context.SaveChangesAsync();
        return notificacao;
    }

    public Task<KidsNotificacao> CreateWithoutSaveAsync(KidsNotificacao notificacao)
    {
        _context.Set<KidsNotificacao>().Add(notificacao);
        return Task.FromResult(notificacao);
    }

    public async Task CreateRangeAsync(IEnumerable<KidsNotificacao> notificacoes)
    {
        await _context.Set<KidsNotificacao>().AddRangeAsync(notificacoes);
        await _context.SaveChangesAsync();
    }

    public async Task<KidsNotificacao> UpdateAsync(KidsNotificacao notificacao)
    {
        _context.Set<KidsNotificacao>().Update(notificacao);
        await _context.SaveChangesAsync();
        return notificacao;
    }
}

