using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class DespesaRepository : IDespesaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public DespesaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Despesa>> GetAllAsync()
    {
        return await _context.Set<Despesa>()
            .Include(d => d.Fornecedor)
            .Include(d => d.CategoriaDespesa)
            .Include(d => d.ContaBancaria)
            .Include(d => d.CentroCusto)
            .Include(d => d.Projeto)
            .Include(d => d.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .OrderByDescending(d => d.DataVencimento)
            .ToListAsync();
    }

    public async Task<Despesa?> GetByIdAsync(int id)
    {
        return await _context.Set<Despesa>()
            .Include(d => d.Fornecedor)
            .Include(d => d.CategoriaDespesa)
            .Include(d => d.ContaBancaria)
            .Include(d => d.CentroCusto)
            .Include(d => d.Projeto)
            .Include(d => d.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Despesa> CreateAsync(Despesa despesa)
    {
        _context.Set<Despesa>().Add(despesa);
        await _context.SaveChangesAsync();
        return despesa;
    }

    public async Task<Despesa> UpdateAsync(Despesa despesa)
    {
        _context.Set<Despesa>().Update(despesa);
        await _context.SaveChangesAsync();
        return despesa;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Despesa>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Despesa>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
