using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ReceitaRepository : IReceitaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ReceitaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Receita>> GetAllAsync()
    {
        return await _context.Set<Receita>()
            .Include(r => r.CategoriaReceita)
            .Include(r => r.ContaBancaria)
            .Include(r => r.CentroCusto)
            .Include(r => r.Projeto)
            .Include(r => r.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .OrderByDescending(r => r.DataRecebimento)
            .ToListAsync();
    }

    public async Task<Receita?> GetByIdAsync(int id)
    {
        return await _context.Set<Receita>()
            .Include(r => r.CategoriaReceita)
            .Include(r => r.ContaBancaria)
            .Include(r => r.CentroCusto)
            .Include(r => r.Projeto)
            .Include(r => r.Usuario)
                .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Receita> CreateAsync(Receita receita)
    {
        _context.Set<Receita>().Add(receita);
        await _context.SaveChangesAsync();
        return receita;
    }

    public async Task<Receita> UpdateAsync(Receita receita)
    {
        _context.Set<Receita>().Update(receita);
        await _context.SaveChangesAsync();
        return receita;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Receita>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Receita>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
