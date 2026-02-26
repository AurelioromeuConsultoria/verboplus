using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs.Pessoas;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class PessoaRepository : IPessoaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public PessoaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pessoa>> GetAllAsync()
    {
        return await _context.Set<Pessoa>()
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Pessoa> Items, int Total)> GetPagedAsync(PessoaPagedQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);

        var q = _context.Set<Pessoa>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Nome))
        {
            var nome = query.Nome.Trim().ToLower();
            q = q.Where(p => p.Nome.ToLower().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim().ToLower();
            q = q.Where(p => p.Email != null && p.Email.ToLower().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(query.Telefone))
        {
            var telefone = query.Telefone.Trim();
            q = q.Where(p => p.Telefone != null && p.Telefone.Contains(telefone));
        }

        if (!string.IsNullOrWhiteSpace(query.WhatsApp))
        {
            var whats = query.WhatsApp.Trim();
            q = q.Where(p => p.WhatsApp != null && p.WhatsApp.Contains(whats));
        }

        if (query.TipoPessoa.HasValue)
        {
            var tipo = query.TipoPessoa.Value;
            q = q.Where(p => p.TipoPessoa == tipo);
        }

        if (query.Ativo.HasValue)
        {
            var ativo = query.Ativo.Value;
            q = q.Where(p => p.Ativo == ativo);
        }

        if (query.Perfil.HasValue)
        {
            var perfil = query.Perfil.Value;
            q = q.Where(p => p.Perfis.Any(pp => pp.DataFim == null && pp.Perfil == perfil));
        }

        // Ordenação
        var sort = (query.Sort ?? "nome").Trim().ToLowerInvariant();
        var desc = string.Equals(query.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        q = sort switch
        {
            "email" => desc ? q.OrderByDescending(p => p.Email) : q.OrderBy(p => p.Email),
            "telefone" => desc ? q.OrderByDescending(p => p.Telefone) : q.OrderBy(p => p.Telefone),
            "whatsapp" => desc ? q.OrderByDescending(p => p.WhatsApp) : q.OrderBy(p => p.WhatsApp),
            "tipopessoa" => desc ? q.OrderByDescending(p => p.TipoPessoa) : q.OrderBy(p => p.TipoPessoa),
            "ativo" => desc ? q.OrderByDescending(p => p.Ativo) : q.OrderBy(p => p.Ativo),
            "datacriacao" => desc ? q.OrderByDescending(p => p.DataCriacao) : q.OrderBy(p => p.DataCriacao),
            _ => desc ? q.OrderByDescending(p => p.Nome) : q.OrderBy(p => p.Nome),
        };

        var total = await q.CountAsync();

        var items = await q
            .Include(p => p.Perfis)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Pessoa?> GetByIdAsync(int id)
    {
        return await _context.Set<Pessoa>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pessoa?> GetByEmailAsync(string email)
    {
        return await _context.Set<Pessoa>()
            .FirstOrDefaultAsync(p => p.Email != null && p.Email.ToLower() == email.ToLower());
    }

    public async Task<Pessoa?> GetByWhatsAppAsync(string whatsApp)
    {
        var whatsAppNormalizado = NormalizarTelefone(whatsApp);
        return await _context.Set<Pessoa>()
            .FirstOrDefaultAsync(p => p.WhatsApp != null && NormalizarTelefone(p.WhatsApp) == whatsAppNormalizado);
    }

    public async Task<Pessoa?> GetByTelefoneAsync(string telefone)
    {
        var telefoneNormalizado = NormalizarTelefone(telefone);
        return await _context.Set<Pessoa>()
            .FirstOrDefaultAsync(p => p.Telefone != null && NormalizarTelefone(p.Telefone) == telefoneNormalizado);
    }

    private static string NormalizarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return string.Empty;
        
        // Remove tudo exceto dígitos
        return new string(telefone.Where(char.IsDigit).ToArray());
    }

    public async Task<Pessoa> CreateAsync(Pessoa pessoa)
    {
        _context.Set<Pessoa>().Add(pessoa);
        await _context.SaveChangesAsync();
        return pessoa;
    }

    public Task<Pessoa> CreateWithoutSaveAsync(Pessoa pessoa)
    {
        _context.Set<Pessoa>().Add(pessoa);
        return Task.FromResult(pessoa);
    }

    public async Task<Pessoa> UpdateAsync(Pessoa pessoa)
    {
        _context.Set<Pessoa>().Update(pessoa);
        await _context.SaveChangesAsync();
        return pessoa;
    }

    public Task UpdateWithoutSaveAsync(Pessoa pessoa)
    {
        _context.Set<Pessoa>().Update(pessoa);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Pessoa>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Pessoa>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}



