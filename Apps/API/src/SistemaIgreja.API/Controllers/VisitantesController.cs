using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitantesController : ControllerBase
{
    private readonly IVisitanteService _visitanteService;
    private readonly IMensagemAgendadaService _mensagemService;

    public VisitantesController(IVisitanteService visitanteService, IMensagemAgendadaService mensagemService)
    {
        _visitanteService = visitanteService;
        _mensagemService = mensagemService;
    }

    /// <summary>
    /// Lista todos os visitantes com dados da Pessoa
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitanteDto>>> GetAll()
    {
        var visitantes = await _visitanteService.GetAllAsync();
        return Ok(visitantes);
    }

    /// <summary>
    /// Obtém detalhe de um visitante específico, incluindo dados da Pessoa e perfis
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VisitanteDto>> GetById(int id)
    {
        var visitante = await _visitanteService.GetByIdAsync(id);
        if (visitante == null)
            return NotFound();

        return Ok(visitante);
    }

    /// <summary>
    /// Lista todas as visitas de uma Pessoa específica
    /// </summary>
    [HttpGet("pessoa/{pessoaId}")]
    public async Task<ActionResult<IEnumerable<VisitanteDto>>> GetByPessoa(int pessoaId)
    {
        var visitantes = await _visitanteService.GetVisitantesPorPessoaAsync(pessoaId);
        return Ok(visitantes);
    }

    /// <summary>
    /// Cria um novo visitante seguindo fluxo de deduplicação de Pessoa
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VisitanteResponse>> Create(CreateVisitanteRequest request)
    {
        try
        {
            var visitante = await _visitanteService.CreateVisitanteAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = visitante.VisitanteId }, visitante);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar visitante", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza observações e data de visita (não altera dados da Pessoa)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VisitanteDto>> Update(int id, AtualizarVisitanteDto dto)
    {
        try
        {
            var visitante = await _visitanteService.UpdateAsync(id, dto);
            return Ok(visitante);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Regera mensagens agendadas do visitante (cancela pendentes e recria conforme configurações ativas).
    /// </summary>
    [HttpPost("{id}/regerar-mensagens")]
    public async Task<ActionResult<RegerarMensagensResultDto>> RegerarMensagens(int id)
    {
        try
        {
            var result = await _mensagemService.RegerarMensagensParaVisitanteAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao regerar mensagens", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um registro de visitante
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _visitanteService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
