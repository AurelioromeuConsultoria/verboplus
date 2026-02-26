using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs.Visitantes;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class VisitanteRepository : IVisitanteRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public VisitanteRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Visitante>> GetAllAsync()
    {
        return await _context.Visitantes
            .Include(v => v.Pessoa)
                .ThenInclude(p => p.Perfis)
            .OrderByDescending(v => v.DataCadastro)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Visitante> Items, int Total)> GetPagedAsync(VisitantePagedQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);

        var q = _context.Visitantes
            .AsNoTracking()
            .Include(v => v.Pessoa)
                .ThenInclude(p => p.Perfis)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Nome))
        {
            var nome = query.Nome.Trim().ToLower();
            q = q.Where(v => v.Pessoa != null && v.Pessoa.Nome.ToLower().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim().ToLower();
            q = q.Where(v => v.Pessoa != null && v.Pessoa.Email != null && v.Pessoa.Email.ToLower().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(query.Telefone))
        {
            var tel = query.Telefone.Trim();
            q = q.Where(v => v.Pessoa != null && v.Pessoa.Telefone != null && v.Pessoa.Telefone.Contains(tel));
        }

        if (!string.IsNullOrWhiteSpace(query.WhatsApp))
        {
            var whats = query.WhatsApp.Trim();
            q = q.Where(v => v.Pessoa != null && v.Pessoa.WhatsApp != null && v.Pessoa.WhatsApp.Contains(whats));
        }

        if (query.DataVisitaFrom.HasValue)
        {
            var from = query.DataVisitaFrom.Value.Date;
            q = q.Where(v => v.DataVisita >= from);
        }

        if (query.DataVisitaTo.HasValue)
        {
            var to = query.DataVisitaTo.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(v => v.DataVisita <= to);
        }

        var sort = (query.Sort ?? "datavisita").Trim().ToLowerInvariant();
        var desc = string.Equals(query.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        q = sort switch
        {
            "nome" => desc ? q.OrderByDescending(v => v.Pessoa!.Nome) : q.OrderBy(v => v.Pessoa!.Nome),
            "datacadastro" => desc ? q.OrderByDescending(v => v.DataCadastro) : q.OrderBy(v => v.DataCadastro),
            _ => desc ? q.OrderByDescending(v => v.DataVisita) : q.OrderBy(v => v.DataVisita),
        };

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Visitante?> GetByIdAsync(int id)
    {
        return await _context.Visitantes
            .Include(v => v.Pessoa)
                .ThenInclude(p => p.Perfis)
            .Include(v => v.MensagensAgendadas)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Visitante> CreateAsync(Visitante visitante)
    {
        _context.Visitantes.Add(visitante);
        await _context.SaveChangesAsync();
        return visitante;
    }

    public Task<Visitante> CreateWithoutSaveAsync(Visitante visitante)
    {
        _context.Visitantes.Add(visitante);
        return Task.FromResult(visitante);
    }

    public async Task<Visitante> UpdateAsync(Visitante visitante)
    {
        _context.Visitantes.Update(visitante);
        await _context.SaveChangesAsync();
        return visitante;
    }

    public async Task DeleteAsync(int id)
    {
        var visitante = await _context.Visitantes.FindAsync(id);
        if (visitante != null)
        {
            _context.Visitantes.Remove(visitante);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Visitante>> GetVisitantesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Visitantes
            .Include(v => v.Pessoa)
            .Where(v => v.DataVisita >= dataInicio && v.DataVisita <= dataFim)
            .OrderByDescending(v => v.DataVisita)
            .ToListAsync();
    }

    public async Task<IEnumerable<Visitante>> GetVisitantesPorPessoaAsync(int pessoaId)
    {
        return await _context.Visitantes
            .Include(v => v.Pessoa)
            .Where(v => v.PessoaId == pessoaId)
            .OrderByDescending(v => v.DataVisita)
            .ToListAsync();
    }
}

