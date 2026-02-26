using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly SistemaIgrejaDbContext _db;

    public AuditLogService(SistemaIgrejaDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<AuditLogDto>> GetPagedAsync(AuditLogPagedQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);

        var q = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var entity = query.EntityName.Trim();
            q = q.Where(a => a.EntityName == entity);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim();
            q = q.Where(a => a.Action == action);
        }

        if (query.UserId.HasValue)
        {
            var userId = query.UserId.Value;
            q = q.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.UserEmail))
        {
            var email = query.UserEmail.Trim().ToLowerInvariant();
            q = q.Where(a => a.UserEmail != null && a.UserEmail.ToLower().Contains(email));
        }

        if (query.From.HasValue)
        {
            var from = DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc);
            q = q.Where(a => a.CreatedAt >= from);
        }

        if (query.To.HasValue)
        {
            var to = DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc);
            q = q.Where(a => a.CreatedAt <= to);
        }

        q = q.OrderByDescending(a => a.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action,
                UserId = a.UserId,
                UserName = a.UserName,
                UserEmail = a.UserEmail,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt,
                ChangesJson = a.ChangesJson
            })
            .ToListAsync();

        return new PagedResultDto<AuditLogDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}

