using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/kids")]
[Authorize]
public class KidsController : ControllerBase
{
    private readonly IKidsService _service;

    public KidsController(IKidsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as crianças cadastradas
    /// </summary>
    [HttpGet("criancas")]
    public async Task<ActionResult<IEnumerable<CriancaDto>>> GetCriancas()
    {
        try
        {
            var criancas = await _service.GetCriancasAsync();
            return Ok(criancas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar crianças", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma criança específica
    /// </summary>
    [HttpGet("criancas/{criancaPessoaId}")]
    public async Task<ActionResult<CriancaDto>> GetCriancaById(int criancaPessoaId)
    {
        try
        {
            var crianca = await _service.GetCriancaByIdAsync(criancaPessoaId);
            if (crianca == null)
                return NotFound(new { message = "Criança não encontrada" });

            return Ok(crianca);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar criança", error = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova criança
    /// </summary>
    [HttpPost("criancas")]
    public async Task<ActionResult<CriancaDto>> CreateCrianca([FromBody] CreateCriancaRequest request)
    {
        try
        {
            var crianca = await _service.CreateCriancaAsync(request);
            return CreatedAtAction(nameof(GetCriancaById), new { criancaPessoaId = crianca.PessoaId }, crianca);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Incluir inner exception para debug
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner: {ex.InnerException.Message}";
            }
            return StatusCode(500, new { message = "Erro ao criar criança", error = errorMessage, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Atualiza dados de uma criança
    /// </summary>
    [HttpPut("criancas/{criancaPessoaId}")]
    public async Task<ActionResult<CriancaDto>> UpdateCrianca(int criancaPessoaId, [FromBody] UpdateCriancaRequest request)
    {
        try
        {
            var crianca = await _service.UpdateCriancaAsync(criancaPessoaId, request);
            return Ok(crianca);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Desativa uma criança (soft delete)
    /// </summary>
    [HttpDelete("criancas/{criancaPessoaId}")]
    public async Task<IActionResult> DeleteCrianca(int criancaPessoaId)
    {
        try
        {
            await _service.DeleteCriancaAsync(criancaPessoaId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Vincula um responsável a uma criança
    /// </summary>
    [HttpPost("criancas/{criancaPessoaId}/responsaveis")]
    public async Task<ActionResult<ResponsavelCriancaDto>> VincularResponsavel(
        int criancaPessoaId,
        [FromBody] CreateResponsavelRequest request)
    {
        try
        {
            var responsavel = await _service.VincularResponsavelAsync(criancaPessoaId, request);
            return CreatedAtAction(
                nameof(GetCriancaById),
                new { criancaPessoaId },
                responsavel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao vincular responsável", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza vínculo de responsável
    /// </summary>
    [HttpPut("responsaveis/{responsavelId}")]
    public async Task<ActionResult<ResponsavelCriancaDto>> UpdateResponsavel(
        int responsavelId,
        [FromBody] UpdateResponsavelRequest request)
    {
        try
        {
            var responsavel = await _service.UpdateResponsavelAsync(responsavelId, request);
            return Ok(responsavel);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Desvincula um responsável de uma criança
    /// </summary>
    [HttpDelete("responsaveis/{responsavelId}")]
    public async Task<IActionResult> DesvincularResponsavel(int responsavelId)
    {
        try
        {
            await _service.DesvincularResponsavelAsync(responsavelId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Realiza check-in de uma criança
    /// </summary>
    [HttpPost("checkin")]
    public async Task<ActionResult<CheckinResponse>> Checkin([FromBody] CheckinRequest request)
    {
        try
        {
            var response = await _service.CheckinAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao realizar check-in", error = ex.Message });
        }
    }

    /// <summary>
    /// Realiza check-out de uma criança
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            await _service.CheckoutAsync(request);
            return Ok(new { message = "Check-out realizado com sucesso" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao realizar check-out", error = ex.Message });
        }
    }

    /// <summary>
    /// Lista histórico de check-ins/check-outs
    /// </summary>
    [HttpGet("checkins")]
    public async Task<ActionResult<IEnumerable<KidsCheckinDto>>> GetHistoricoCheckins(
        [FromQuery] int? criancaPessoaId = null)
    {
        try
        {
            var historico = await _service.GetHistoricoCheckinsAsync(criancaPessoaId);
            return Ok(historico);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar histórico", error = ex.Message });
        }
    }
}


