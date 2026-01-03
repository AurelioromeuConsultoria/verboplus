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

    public async Task<KidsNotificacao> UpdateAsync(KidsNotificacao notificacao)
    {
        _context.Set<KidsNotificacao>().Update(notificacao);
        await _context.SaveChangesAsync();
        return notificacao;
    }
}


