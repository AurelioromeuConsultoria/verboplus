using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class NoticiaRepository : INoticiaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public NoticiaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public NoticiaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<Noticia>> GetAllAsync()
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .OrderByDescending(n => n.Data)
            .ToListAsync();
    }

    public async Task<Noticia?> GetByIdAsync(int id)
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Noticia>> GetByCategoriaAsync(int categoriaId)
    {
        return await _context.Set<Noticia>()
            .Include(n => n.CategoriaNoticia)
            .Where(n => n.CategoriaNoticiaId == categoriaId)
            .OrderByDescending(n => n.Data)
            .ToListAsync();
    }

    public async Task<Noticia> CreateAsync(Noticia noticia)
    {
        noticia.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<Noticia>().Add(noticia);
        await _context.SaveChangesAsync();
        return noticia;
    }

    public async Task<Noticia> UpdateAsync(Noticia noticia)
    {
        _context.Set<Noticia>().Update(noticia);
        await _context.SaveChangesAsync();
        return noticia;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<Noticia>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<Noticia>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}


