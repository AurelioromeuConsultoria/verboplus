using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class KidsConteudoAulaRepository : IKidsConteudoAulaRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public KidsConteudoAulaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public KidsConteudoAulaRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<KidsConteudoAula?> GetByIdAsync(int id)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<KidsConteudoAula>> GetAllAsync(string? status = null, string? salaId = null, string? turmaId = null, DateTime? dataReferencia = null, int? limit = null)
    {
        IQueryable<KidsConteudoAula> query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(salaId))
        {
            query = query.Where(x => x.SalaId == salaId);
        }

        if (!string.IsNullOrWhiteSpace(turmaId))
        {
            query = query.Where(x => x.TurmaId == turmaId);
        }

        if (dataReferencia.HasValue)
        {
            var day = dataReferencia.Value.Date;
            query = query.Where(x => x.DataReferencia.Date == day);
        }

        query = query
            .OrderByDescending(x => x.DataReferencia)
            .ThenByDescending(x => x.PublicadoEm ?? x.CriadoEm);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public Task<KidsConteudoAula> CreateWithoutSaveAsync(KidsConteudoAula conteudo)
    {
        conteudo.TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        _context.Set<KidsConteudoAula>().Add(conteudo);
        return Task.FromResult(conteudo);
    }

    public Task UpdateWithoutSaveAsync(KidsConteudoAula conteudo)
    {
        _context.Set<KidsConteudoAula>().Update(conteudo);
        return Task.CompletedTask;
    }

    private IQueryable<KidsConteudoAula> BaseQuery()
    {
        IQueryable<KidsConteudoAula> query = _context.Set<KidsConteudoAula>()
            .Include(x => x.EventoOcorrencia)
            .Include(x => x.PublicadoPor)
            .Include(x => x.Anexos);

        if (_tenantContext.TenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == _tenantContext.TenantId.Value);
        }

        return query;
    }
}

public class KidsConteudoAulaAnexoRepository : IKidsConteudoAulaAnexoRepository
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly ITenantContext _tenantContext;

    public KidsConteudoAulaAnexoRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public KidsConteudoAulaAnexoRepository(SistemaIgrejaDbContext context)
        : this(context, new DefaultTenantContext())
    {
    }

    public async Task<IEnumerable<KidsConteudoAulaAnexo>> GetByConteudoAulaIdAsync(int conteudoAulaId)
    {
        return await BaseQuery()
            .Where(x => x.ConteudoAulaId == conteudoAulaId)
            .OrderBy(x => x.Ordem)
            .ToListAsync();
    }

    public Task CreateRangeWithoutSaveAsync(IEnumerable<KidsConteudoAulaAnexo> anexos)
    {
        var tenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId;
        foreach (var anexo in anexos)
        {
            anexo.TenantId = tenantId;
        }

        _context.Set<KidsConteudoAulaAnexo>().AddRange(anexos);
        return Task.CompletedTask;
    }

    public async Task DeleteByConteudoAulaIdWithoutSaveAsync(int conteudoAulaId)
    {
        var anexos = await _context.Set<KidsConteudoAulaAnexo>()
            .Where(x => x.ConteudoAulaId == conteudoAulaId)
            .ToListAsync();

        if (anexos.Count == 0)
        {
            return;
        }

        _context.Set<KidsConteudoAulaAnexo>().RemoveRange(anexos);
    }

    private IQueryable<KidsConteudoAulaAnexo> BaseQuery()
    {
        var query = _context.Set<KidsConteudoAulaAnexo>().AsQueryable();

        if (_tenantContext.TenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == _tenantContext.TenantId.Value);
        }

        return query;
    }
}
