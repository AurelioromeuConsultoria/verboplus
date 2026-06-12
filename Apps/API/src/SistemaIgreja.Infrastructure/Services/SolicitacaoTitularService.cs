using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Services;

public class SolicitacaoTitularService : ISolicitacaoTitularService
{
    private const int PrazoLegalDias = 15;

    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext _currentUser;

    public SolicitacaoTitularService(
        SistemaIgrejaDbContext context,
        ITenantContext tenantContext,
        ICurrentUserContext currentUser)
    {
        _context = context;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<SolicitacaoTitularDto> CriarAsync(CriarSolicitacaoTitularDto dto)
    {
        var agora = DateTime.UtcNow;
        var solicitacao = new SolicitacaoTitular
        {
            TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId,
            PessoaId = dto.PessoaId,
            NomeSolicitante = dto.NomeSolicitante,
            ContatoSolicitante = dto.ContatoSolicitante,
            Tipo = dto.Tipo,
            Status = StatusSolicitacaoTitular.Aberta,
            Canal = dto.Canal,
            Descricao = dto.Descricao,
            SolicitadoEm = agora,
            PrazoLimite = agora.AddDays(PrazoLegalDias)
        };

        _context.Set<SolicitacaoTitular>().Add(solicitacao);
        await _context.SaveChangesAsync();

        return await ObterAsync(solicitacao.Id) ?? Map(solicitacao, null);
    }

    public async Task<IEnumerable<SolicitacaoTitularDto>> ListarAsync(StatusSolicitacaoTitular? status = null)
    {
        var query = _context.Set<SolicitacaoTitular>().AsNoTracking().AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var itens = await query
            .OrderBy(s => s.Status == StatusSolicitacaoTitular.Aberta || s.Status == StatusSolicitacaoTitular.EmAtendimento ? 0 : 1)
            .ThenBy(s => s.PrazoLimite)
            .Select(s => new { Solicitacao = s, NomePessoa = s.Pessoa != null ? s.Pessoa.Nome : null })
            .ToListAsync();

        return itens.Select(i => Map(i.Solicitacao, i.NomePessoa)).ToList();
    }

    public async Task<SolicitacaoTitularDto?> ObterAsync(int id)
    {
        var item = await _context.Set<SolicitacaoTitular>().AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new { Solicitacao = s, NomePessoa = s.Pessoa != null ? s.Pessoa.Nome : null })
            .FirstOrDefaultAsync();

        return item == null ? null : Map(item.Solicitacao, item.NomePessoa);
    }

    public async Task<SolicitacaoTitularDto?> AtenderAsync(int id)
    {
        var solicitacao = await _context.Set<SolicitacaoTitular>().FirstOrDefaultAsync(s => s.Id == id);
        if (solicitacao == null)
        {
            return null;
        }

        solicitacao.Status = StatusSolicitacaoTitular.EmAtendimento;
        await _context.SaveChangesAsync();
        return await ObterAsync(id);
    }

    public async Task<SolicitacaoTitularDto?> ConcluirAsync(int id, string? observacao)
    {
        var solicitacao = await _context.Set<SolicitacaoTitular>().FirstOrDefaultAsync(s => s.Id == id);
        if (solicitacao == null)
        {
            return null;
        }

        solicitacao.Status = StatusSolicitacaoTitular.Concluida;
        solicitacao.AtendidoEm = DateTime.UtcNow;
        solicitacao.AtendidoPorUsuarioId = _currentUser.UserId;
        solicitacao.ResultadoObservacao = observacao;
        await _context.SaveChangesAsync();
        return await ObterAsync(id);
    }

    public async Task<SolicitacaoTitularDto?> RecusarAsync(int id, string motivo)
    {
        var solicitacao = await _context.Set<SolicitacaoTitular>().FirstOrDefaultAsync(s => s.Id == id);
        if (solicitacao == null)
        {
            return null;
        }

        solicitacao.Status = StatusSolicitacaoTitular.Recusada;
        solicitacao.AtendidoEm = DateTime.UtcNow;
        solicitacao.AtendidoPorUsuarioId = _currentUser.UserId;
        solicitacao.ResultadoObservacao = motivo;
        await _context.SaveChangesAsync();
        return await ObterAsync(id);
    }

    private static SolicitacaoTitularDto Map(SolicitacaoTitular s, string? nomePessoa)
    {
        var agora = DateTime.UtcNow;
        var emAberto = s.Status is StatusSolicitacaoTitular.Aberta or StatusSolicitacaoTitular.EmAtendimento;
        return new SolicitacaoTitularDto
        {
            Id = s.Id,
            PessoaId = s.PessoaId,
            NomePessoa = nomePessoa,
            NomeSolicitante = s.NomeSolicitante,
            ContatoSolicitante = s.ContatoSolicitante,
            Tipo = s.Tipo.ToString(),
            Status = s.Status.ToString(),
            Canal = s.Canal,
            Descricao = s.Descricao,
            SolicitadoEm = s.SolicitadoEm,
            PrazoLimite = s.PrazoLimite,
            AtendidoEm = s.AtendidoEm,
            AtendidoPorUsuarioId = s.AtendidoPorUsuarioId,
            ResultadoObservacao = s.ResultadoObservacao,
            PrazoVencido = emAberto && agora > s.PrazoLimite,
            DiasRestantes = (int)Math.Ceiling((s.PrazoLimite - agora).TotalDays)
        };
    }
}
