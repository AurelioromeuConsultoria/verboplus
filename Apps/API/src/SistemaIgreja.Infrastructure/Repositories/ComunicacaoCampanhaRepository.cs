using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ComunicacaoCampanhaRepository : IComunicacaoCampanhaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ComunicacaoCampanhaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<ComunicacaoCampanha> Items, int Total)> GetPagedAsync(ComunicacaoCampanhaPagedQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);

        var q = _context.ComunicacaoCampanhas
            .AsNoTracking()
            .Include(c => c.Canais)
                .ThenInclude(cc => cc.Template)
            .Include(c => c.Entregas)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            q = q.Where(c => c.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.PublicoAlvo))
        {
            var publico = query.PublicoAlvo.Trim().ToLowerInvariant();
            q = q.Where(c => c.PublicoAlvo.ToLower() == publico);
        }

        if (!string.IsNullOrWhiteSpace(query.Texto))
        {
            var texto = query.Texto.Trim().ToLowerInvariant();
            q = q.Where(c => c.Nome.ToLower().Contains(texto) || c.Objetivo.ToLower().Contains(texto));
        }

        q = q.OrderByDescending(c => c.DataCriacao);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<ComunicacaoCampanha?> GetByIdAsync(int id)
    {
        return await _context.ComunicacaoCampanhas
            .Include(c => c.Canais)
                .ThenInclude(cc => cc.Template)
            .Include(c => c.Entregas.OrderByDescending(e => e.DataCriacao).Take(50))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ComunicacaoCampanha> CreateAsync(ComunicacaoCampanha campanha)
    {
        _context.ComunicacaoCampanhas.Add(campanha);
        await _context.SaveChangesAsync();
        return campanha;
    }

    public async Task<ComunicacaoCampanha> UpdateAsync(ComunicacaoCampanha campanha)
    {
        _context.ComunicacaoCampanhas.Update(campanha);
        await _context.SaveChangesAsync();
        return campanha;
    }

    public async Task<ComunicacaoStatsDto> GetStatsAsync()
    {
        var totalCampanhas = await _context.ComunicacaoCampanhas.CountAsync();
        var campanhasRascunho = await _context.ComunicacaoCampanhas.CountAsync(c => c.Status == StatusComunicacaoCampanha.Rascunho);
        var campanhasAgendadas = await _context.ComunicacaoCampanhas.CountAsync(c => c.Status == StatusComunicacaoCampanha.Agendada);
        var entregasPendentes = await _context.ComunicacaoEntregas.CountAsync(e =>
            e.Status == StatusComunicacaoEntrega.Pendente || e.Status == StatusComunicacaoEntrega.Reservado);
        var entregasEnviadas = await _context.ComunicacaoEntregas.CountAsync(e =>
            e.Status == StatusComunicacaoEntrega.Enviado || e.Status == StatusComunicacaoEntrega.Entregue);
        var entregasComFalha = await _context.ComunicacaoEntregas.CountAsync(e => e.Status == StatusComunicacaoEntrega.Falhou);

        return new ComunicacaoStatsDto
        {
            TotalCampanhas = totalCampanhas,
            CampanhasRascunho = campanhasRascunho,
            CampanhasAgendadas = campanhasAgendadas,
            EntregasPendentes = entregasPendentes,
            EntregasEnviadas = entregasEnviadas,
            EntregasComFalha = entregasComFalha
        };
    }
}
