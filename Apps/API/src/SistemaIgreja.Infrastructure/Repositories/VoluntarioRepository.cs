using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class VoluntarioRepository : IVoluntarioRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public VoluntarioRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public VoluntarioRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<Voluntario>> GetAllAsync()
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Pessoa)
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .OrderBy(v => v.Pessoa.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Voluntario>> GetByEquipeAsync(int equipeId)
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Pessoa)
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .Where(v => v.EquipeId == equipeId)
            .OrderBy(v => v.Pessoa.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Voluntario>> GetByPessoaIdAsync(int pessoaId)
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Pessoa)
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .Where(v => v.PessoaId == pessoaId)
            .OrderBy(v => v.Equipe.Nome)
            .ThenBy(v => v.Cargo.Nome)
            .ToListAsync();
    }

    public async Task<Voluntario?> GetByIdAsync(int id)
    {
        return await _context.Set<Voluntario>()
            .Include(v => v.Pessoa)
            .Include(v => v.Equipe)
            .Include(v => v.Cargo)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> ExistsByPessoaEquipeCargoAsync(int pessoaId, int equipeId, int cargoId, int? ignoreVoluntarioId = null)
    {
        var q = _context.Set<Voluntario>()
            .AsNoTracking()
            .Where(v => v.PessoaId == pessoaId && v.EquipeId == equipeId && v.CargoId == cargoId);

        if (ignoreVoluntarioId.HasValue)
        {
            q = q.Where(v => v.Id != ignoreVoluntarioId.Value);
        }

        return await q.AnyAsync();
    }

    public async Task<Voluntario> CreateAsync(Voluntario voluntario)
    {
        voluntario.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<Voluntario>().Add(voluntario);
        await _context.SaveChangesAsync();
        return voluntario;
    }

    public async Task<Voluntario> UpdateAsync(Voluntario voluntario)
    {
        _context.Set<Voluntario>().Update(voluntario);
        await _context.SaveChangesAsync();
        return voluntario;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Voluntario>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Voluntario>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
