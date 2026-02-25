using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceitasController : ControllerBase
{
    private readonly IReceitaService _service;

    public ReceitasController(IReceitaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReceitaDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReceitaDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ReceitaDto>> Create(CriarReceitaDto dto)
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
    public async Task<ActionResult<ReceitaDto>> Update(int id, AtualizarReceitaDto dto)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
