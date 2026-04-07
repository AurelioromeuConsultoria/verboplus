using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class KidsCheckinRepository : IKidsCheckinRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public KidsCheckinRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<KidsCheckin?> GetByIdAsync(int id)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<KidsCheckin?> GetCheckinAtivoPorCriancaAsync(int criancaPessoaId)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .FirstOrDefaultAsync(c => c.CriancaPessoaId == criancaPessoaId && c.Status == "CheckedIn");
    }

    public async Task<KidsCheckin?> GetByCodigoSessaoAsync(string codigoSessao)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .FirstOrDefaultAsync(c => c.CodigoSessao == codigoSessao);
    }

    public async Task<KidsCheckin?> GetByTokenRetiradaAsync(string tokenRetirada)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .FirstOrDefaultAsync(c => c.TokenRetirada == tokenRetirada);
    }

    public async Task<KidsCheckin?> GetByPinRetiradaAsync(string pinRetirada)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .FirstOrDefaultAsync(c => c.PinRetirada == pinRetirada);
    }

    public async Task<IEnumerable<KidsCheckin>> GetByPeriodoAsync(DateTime dataInicioUtc, DateTime dataFimUtc)
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .Where(c => c.CheckinTime >= dataInicioUtc && c.CheckinTime <= dataFimUtc)
            .OrderByDescending(c => c.CheckinTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<KidsCheckin>> GetHistoricoPorCriancaAsync(int criancaPessoaId, int? limit = null)
    {
        var query = _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Include(c => c.CheckoutBy)
            .Where(c => c.CriancaPessoaId == criancaPessoaId)
            .OrderByDescending(c => c.CheckinTime);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<KidsCheckin>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<KidsCheckin>> GetCheckinsAtivosAsync()
    {
        return await _context.Set<KidsCheckin>()
            .Include(c => c.Crianca)
            .Include(c => c.CheckinBy)
            .Where(c => c.Status == "CheckedIn")
            .OrderByDescending(c => c.CheckinTime)
            .ToListAsync();
    }

    public async Task<KidsCheckin> CreateAsync(KidsCheckin checkin)
    {
        _context.Set<KidsCheckin>().Add(checkin);
        await _context.SaveChangesAsync();
        return checkin;
    }

    public Task<KidsCheckin> CreateWithoutSaveAsync(KidsCheckin checkin)
    {
        _context.Set<KidsCheckin>().Add(checkin);
        return Task.FromResult(checkin);
    }

    public async Task<KidsCheckin> UpdateAsync(KidsCheckin checkin)
    {
        _context.Set<KidsCheckin>().Update(checkin);
        await _context.SaveChangesAsync();
        return checkin;
    }

    public Task UpdateWithoutSaveAsync(KidsCheckin checkin)
    {
        _context.Set<KidsCheckin>().Update(checkin);
        return Task.CompletedTask;
    }
}
