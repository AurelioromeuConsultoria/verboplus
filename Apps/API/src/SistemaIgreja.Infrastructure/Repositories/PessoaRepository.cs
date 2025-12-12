using Microsoft.EntityFrameworkCore;
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



