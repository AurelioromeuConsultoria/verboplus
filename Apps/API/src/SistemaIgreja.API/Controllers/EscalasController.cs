using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EscalasController : ControllerBase
{
    private readonly IEscalaService _service;

    public EscalasController(IEscalaService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscalaDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}")]
    public async Task<ActionResult<EscalaDto>> GetByEventoOcorrencia(int eventoOcorrenciaId)
    {
        var item = await _service.GetByEventoOcorrenciaAsync(eventoOcorrenciaId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}/escalas")]
    public async Task<ActionResult<IEnumerable<EscalaDto>>> GetAllByEventoOcorrencia(int eventoOcorrenciaId)
    {
        var items = await _service.GetAllByEventoOcorrenciaAsync(eventoOcorrenciaId);
        return Ok(items);
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}/equipe/{equipeId}")]
    public async Task<ActionResult<EscalaDto>> GetByEventoOcorrenciaAndEquipe(int eventoOcorrenciaId, int equipeId)
    {
        var item = await _service.GetByEventoOcorrenciaAndEquipeAsync(eventoOcorrenciaId, equipeId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("{escalaId}/sugestoes")]
    public async Task<ActionResult<IEnumerable<SugestaoEscalaVoluntarioDto>>> GetSugestoes(int escalaId, [FromQuery] int equipeId)
    {
        try
        {
            var itens = await _service.GetSugestoesAsync(escalaId, equipeId);
            return Ok(itens);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<EscalaDto>> Create(CriarEscalaDto dto)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var created = await _service.CreateAsync(dto, usuarioId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EscalaDto>> Update(int id, AtualizarEscalaDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{escalaId}/itens")]
    public async Task<ActionResult<EscalaItemDto>> AddItem(int escalaId, CriarEscalaItemDto dto)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var isAdmin = IsAdminUser();
            var created = await _service.AddItemAsync(escalaId, dto, usuarioId, isAdmin);
            return Ok(created);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{escalaId}/itens/{escalaItemId}")]
    public async Task<ActionResult<EscalaItemDto>> UpdateItem(int escalaId, int escalaItemId, AtualizarEscalaItemDto dto)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var isAdmin = IsAdminUser();
            var updated = await _service.UpdateItemAsync(escalaId, escalaItemId, dto, usuarioId, isAdmin);
            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{escalaId}/itens/{escalaItemId}")]
    public async Task<IActionResult> DeleteItem(int escalaId, int escalaItemId)
    {
        try
        {
            await _service.DeleteItemAsync(escalaId, escalaItemId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{escalaId}/publicar")]
    public async Task<ActionResult<EscalaDto>> Publicar(int escalaId)
    {
        try
        {
            var updated = await _service.PublicarAsync(escalaId);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("ocorrencia/{eventoOcorrenciaId}/equipe/{equipeId}/gerar-automatico")]
    public async Task<ActionResult<EscalaDto>> GerarAutomatico(int eventoOcorrenciaId, int equipeId)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var escala = await _service.GerarAutomaticoAsync(eventoOcorrenciaId, equipeId, usuarioId);
            return Ok(escala);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private int GetUsuarioId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    private bool IsAdminUser()
    {
        var tipoUsuarioId = User.FindFirstValue("TipoUsuarioId");
        return tipoUsuarioId == ((int)TipoUsuario.Admin).ToString() ||
               tipoUsuarioId == ((int)TipoUsuario.Ambos).ToString();
    }
}
