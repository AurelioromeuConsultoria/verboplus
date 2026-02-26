using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetPaged([FromQuery] AuditLogPagedQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
    }
}

