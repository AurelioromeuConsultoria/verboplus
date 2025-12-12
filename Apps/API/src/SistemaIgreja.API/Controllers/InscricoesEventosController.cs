using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InscricoesEventosController : ControllerBase
{
    private readonly IInscricaoEventoService _service;

    public InscricoesEventosController(IInscricaoEventoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InscricaoEventoDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InscricaoEventoDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("evento/{eventoId}")]
    public async Task<ActionResult<IEnumerable<InscricaoEventoDto>>> GetByEvento(int eventoId)
    {
        var items = await _service.GetByEventoAsync(eventoId);
        return Ok(items);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<InscricaoEventoDto>>> GetByStatus(StatusInscricao status)
    {
        var items = await _service.GetByStatusAsync(status);
        return Ok(items);
    }

    [HttpGet("evento/{eventoId}/estatisticas")]
    public async Task<ActionResult<EstatisticasInscricaoDto>> GetEstatisticas(int eventoId)
    {
        try
        {
            var estatisticas = await _service.ObterEstatisticasAsync(eventoId);
            return Ok(estatisticas);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<InscricaoEventoDto>> Create(CriarInscricaoEventoDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InscricaoEventoDto>> Update(int id, AtualizarInscricaoEventoDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/confirmar")]
    public async Task<ActionResult<InscricaoEventoDto>> Confirmar(int id)
    {
        try
        {
            var updated = await _service.ConfirmarInscricaoAsync(id);
            return Ok(updated);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult<InscricaoEventoDto>> Cancelar(int id)
    {
        try
        {
            var updated = await _service.CancelarInscricaoAsync(id);
            return Ok(updated);
        }
        catch (ArgumentException)
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
}



