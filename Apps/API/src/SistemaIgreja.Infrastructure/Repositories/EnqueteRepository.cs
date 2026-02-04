using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EnqueteRepository : IEnqueteRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EnqueteRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Enquete>> GetAllAsync()
    {
        return await _context.Set<Enquete>()
            .Include(e => e.Opcoes)
            .OrderByDescending(e => e.DataCriacao)
            .ToListAsync();
    }

    public async Task<Enquete?> GetByIdAsync(int id)
    {
        return await _context.Set<Enquete>()
            .Include(e => e.Opcoes.OrderBy(o => o.Ordem))
            .Include(e => e.Votos)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Enquete>> GetAtivasAsync()
    {
        var agora = DateTime.Now;
        return await _context.Set<Enquete>()
            .Include(e => e.Opcoes.OrderBy(o => o.Ordem))
            .Where(e => e.Ativo && e.DataInicio <= agora && e.DataFim >= agora)
            .OrderByDescending(e => e.DataCriacao)
            .ToListAsync();
    }

    public async Task<Enquete> CreateAsync(Enquete enquete)
    {
        _context.Set<Enquete>().Add(enquete);
        await _context.SaveChangesAsync();
        return enquete;
    }

    public async Task<Enquete> UpdateAsync(Enquete enquete)
    {
        _context.Set<Enquete>().Update(enquete);
        await _context.SaveChangesAsync();
        return enquete;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Enquete>()
            .Include(e => e.Opcoes)
            .Include(e => e.Votos)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (entity != null)
        {
            // Remove votos
            _context.Set<EnqueteVoto>().RemoveRange(entity.Votos);
            // Remove opções
            _context.Set<EnqueteOpcao>().RemoveRange(entity.Opcoes);
            // Remove enquete
            _context.Set<Enquete>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EnqueteOpcao?> GetOpcaoByIdAsync(int id)
    {
        return await _context.Set<EnqueteOpcao>()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<EnqueteOpcao> CreateOpcaoAsync(EnqueteOpcao opcao)
    {
        _context.Set<EnqueteOpcao>().Add(opcao);
        await _context.SaveChangesAsync();
        return opcao;
    }

    public async Task<EnqueteOpcao> UpdateOpcaoAsync(EnqueteOpcao opcao)
    {
        _context.Set<EnqueteOpcao>().Update(opcao);
        await _context.SaveChangesAsync();
        return opcao;
    }

    public async Task DeleteOpcaoAsync(int id)
    {
        var entity = await _context.Set<EnqueteOpcao>()
            .Include(o => o.Votos)
            .FirstOrDefaultAsync(o => o.Id == id);
        
        if (entity != null)
        {
            // Remove votos relacionados
            _context.Set<EnqueteVoto>().RemoveRange(entity.Votos);
            // Remove opção
            _context.Set<EnqueteOpcao>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EnqueteVoto> CreateVotoAsync(EnqueteVoto voto)
    {
        _context.Set<EnqueteVoto>().Add(voto);
        await _context.SaveChangesAsync();
        return voto;
    }

    public async Task<bool> UsuarioJaVotouAsync(int enqueteId, int? usuarioId)
    {
        if (!usuarioId.HasValue) return false;
        
        return await _context.Set<EnqueteVoto>()
            .AnyAsync(v => v.EnqueteId == enqueteId && v.UsuarioId == usuarioId);
    }

    public async Task<IEnumerable<EnqueteVoto>> GetVotosPorEnqueteAsync(int enqueteId)
    {
        return await _context.Set<EnqueteVoto>()
            .Where(v => v.EnqueteId == enqueteId)
            .ToListAsync();
    }
}
