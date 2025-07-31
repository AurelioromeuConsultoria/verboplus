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
            .Include(m => m.ConfiguracaoMensagem)
            .OrderByDescending(m => m.DataCriacao)
            .ToListAsync();
    }

    public async Task<MensagemAgendada?> GetByIdAsync(int id)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
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
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.Status == StatusMensagem.Agendada && m.DataEnvio <= agora)
            .OrderBy(m => m.DataEnvio)
            .ToListAsync();
    }

    public async Task<IEnumerable<MensagemAgendada>> GetMensagensPorVisitanteAsync(int visitanteId)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.VisitanteId == visitanteId)
            .OrderBy(m => m.DataEnvio)
            .ToListAsync();
    }

    public async Task<IEnumerable<MensagemAgendada>> GetMensagensPorStatusAsync(StatusMensagem status)
    {
        return await _context.MensagensAgendadas
            .Include(m => m.Visitante)
            .Include(m => m.ConfiguracaoMensagem)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.DataCriacao)
            .ToListAsync();
    }
}

