using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class MensagemAgendadaRepository : IMensagemAgendadaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public MensagemAgendadaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MensagemAgendada>> GetAllAsync()
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem)
            .OrderByDescending(m => m.DataCriacao)
            .ToListAsync();
    }

    public async Task<MensagemAgendada?> GetByIdAsync(int id)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MensagemAgendada> CreateAsync(MensagemAgendada mensagem)
    {
        _context.MensagensAgendadas.Add(mensagem);
        await _context.SaveChangesAsync();
        return mensagem;
    }

    public async Task<MensagemAgendada> UpdateAsync(MensagemAgendada mensagem)
    {
        _context.MensagensAgendadas.Update(mensagem);
        await _context.SaveChangesAsync();
        return mensagem;
    }

    public async Task DeleteAsync(int id)
    {
        var mensagem = await _context.MensagensAgendadas.FindAsync(id);
        if (mensagem != null)
        {
            _context.MensagensAgendadas.Remove(mensagem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MensagemAgendada>> GetMensagensProntasParaEnvioAsync()
    {
        var agora = DateTime.Now;
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.Status == StatusMensagem.Agendada && m.DataEnvio <= agora)
            .OrderBy(m => m.DataEnvio)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MensagemAgendada>> ReservarProntasParaEnvioAsync(int limit)
    {
        var agora = DateTime.Now;
        var statusAgendada = (int)StatusMensagem.Agendada;
        List<int> ids = new();

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (_context.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
                {
                    ids = await _context.MensagensAgendadas
                        .FromSqlRaw(
                            "SELECT * FROM \"MensagensAgendadas\" WHERE \"Status\" = {0} AND \"DataEnvio\" <= {1} ORDER BY \"DataEnvio\" FOR UPDATE SKIP LOCKED",
                            statusAgendada,
                            agora)
                        .Take(limit)
                        .Select(m => m.Id)
                        .ToListAsync();
                }
                else
                {
                    ids = await _context.MensagensAgendadas
                        .FromSqlRaw(
                            "SELECT * FROM MensagensAgendadas WITH (UPDLOCK, ROWLOCK) WHERE Status = {0} AND DataEnvio <= {1} ORDER BY DataEnvio",
                            statusAgendada,
                            agora)
                        .Take(limit)
                        .Select(m => m.Id)
                        .ToListAsync();
                }

                if (ids.Count == 0)
                {
                    await transaction.CommitAsync();
                    return;
                }

                var now = DateTime.Now;
                await _context.MensagensAgendadas
                    .Where(m => ids.Contains(m.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.Status, StatusMensagem.EmProcessamento)
                        .SetProperty(m => m.DataProcessamento, now));

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        return await _context.MensagensAgendadas
            .Include(m => m.Visitante!)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem!)
            .Where(m => ids.Contains(m.Id))
            .OrderBy(m => m.DataEnvio)
            .ToListAsync();
    }

    public async Task<IEnumerable<MensagemAgendada>> GetMensagensPorVisitanteAsync(int visitanteId)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.VisitanteId == visitanteId)
            .OrderBy(m => m.DataEnvio)
            .ToListAsync();
    }

    public async Task<IEnumerable<MensagemAgendada>> GetMensagensPorStatusAsync(StatusMensagem status)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
                .ThenInclude(v => v.Pessoa)
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.DataCriacao)
            .ToListAsync();
    }

    public async Task<int> CancelarPendentesPorVisitanteAsync(int visitanteId, string motivo)
    {
        var now = DateTime.Now;
        var motivoFinal = string.IsNullOrWhiteSpace(motivo)
            ? "Cancelada por regeneração"
            : motivo;

        return await _context.MensagensAgendadas
            .Where(m => m.VisitanteId == visitanteId && m.Status != StatusMensagem.Enviada)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, StatusMensagem.Cancelada)
                .SetProperty(m => m.DataProcessamento, now)
                .SetProperty(m => m.LogErro, motivoFinal));
    }
}

