using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ConfiguracaoMensagemRepository : IConfiguracaoMensagemRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ConfiguracaoMensagemRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConfiguracaoMensagem>> GetAllAsync()
    {
        return await _context.ConfiguracoesMensagens
            .OrderBy(c => c.DiasAposVisita)
            .ToListAsync();
    }

    public async Task<ConfiguracaoMensagem?> GetByIdAsync(int id)
    {
        return await _context.ConfiguracoesMensagens
            .Include(c => c.MensagensAgendadas)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ConfiguracaoMensagem> CreateAsync(ConfiguracaoMensagem configuracao)
    {
        _context.ConfiguracoesMensagens.Add(configuracao);
        await _context.SaveChangesAsync();
        return configuracao;
    }

    public async Task<ConfiguracaoMensagem> UpdateAsync(ConfiguracaoMensagem configuracao)
    {
        _context.ConfiguracoesMensagens.Update(configuracao);
        await _context.SaveChangesAsync();
        return configuracao;
    }

    public async Task DeleteAsync(int id)
    {
        var configuracao = await _context.ConfiguracoesMensagens.FindAsync(id);
        if (configuracao != null)
        {
            _context.ConfiguracoesMensagens.Remove(configuracao);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ConfiguracaoMensagem>> GetAtivasAsync()
    {
        return await _context.ConfiguracoesMensagens
            .Where(c => c.Ativo)
            .OrderBy(c => c.DiasAposVisita)
            .ToListAsync();
    }
}

