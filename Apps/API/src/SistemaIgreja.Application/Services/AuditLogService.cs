using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;

namespace SistemaIgreja.Application.Services;

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLogDto>> GetPagedAsync(AuditLogPagedQueryDto query);
}

