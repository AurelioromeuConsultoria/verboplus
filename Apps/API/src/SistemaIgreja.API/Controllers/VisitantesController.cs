using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitantesController : ControllerBase
{
    private readonly IVisitanteService _visitanteService;

    public VisitantesController(IVisitanteService visitanteService)
    {
        _visitanteService = visitanteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitanteDto>>> GetAll()
    {
        var visitantes = await _visitanteService.GetAllAsync();
        return Ok(visitantes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VisitanteDto>> GetById(int id)
    {
        var visitante = await _visitanteService.GetByIdAsync(id);
        if (visitante == null)
            return NotFound();

        return Ok(visitante);
    }

    [HttpPost]
    public async Task<ActionResult<VisitanteDto>> Create(CriarVisitanteDto dto)
    {
        try
        {
            var visitante = await _visitanteService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = visitante.Id }, visitante);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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
            return BadRequest(ex.Message);
        }
    }

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
            return BadRequest(ex.Message);
        }
    }
}

