using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public FornecedorRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
    {
        return await _context.Set<Fornecedor>()
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }

    public async Task<Fornecedor?> GetByIdAsync(int id)
    {
        return await _context.Set<Fornecedor>()
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fornecedor> CreateAsync(Fornecedor fornecedor)
    {
        _context.Set<Fornecedor>().Add(fornecedor);
        await _context.SaveChangesAsync();
        return fornecedor;
    }

    public async Task<Fornecedor> UpdateAsync(Fornecedor fornecedor)
    {
        _context.Set<Fornecedor>().Update(fornecedor);
        await _context.SaveChangesAsync();
        return fornecedor;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Fornecedor>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Fornecedor>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
