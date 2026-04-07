using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComunicacaoEntregasController : ControllerBase
{
    private readonly IComunicacaoEntregaService _service;
    private readonly IComunicacaoProcessamentoService _processamentoService;

    public ComunicacaoEntregasController(IComunicacaoEntregaService service, IComunicacaoProcessamentoService processamentoService)
    {
        _service = service;
        _processamentoService = processamentoService;
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<ComunicacaoEntregaResumoDto>>> GetPaged([FromQuery] ComunicacaoEntregaPagedQueryDto query)
    {
        return Ok(await _service.GetPagedAsync(query));
    }

    [HttpPost("processar")]
    public async Task<ActionResult<object>> ProcessarPendentes([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var processadas = await _processamentoService.ProcessarPendentesAsync(limit, cancellationToken);
        return Ok(new { processadas });
    }
}
