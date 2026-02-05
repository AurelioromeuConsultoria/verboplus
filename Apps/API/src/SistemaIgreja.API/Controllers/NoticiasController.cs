using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoticiasController : ControllerBase
{
    private readonly INoticiaService _service;

    public NoticiasController(INoticiaService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoticiaDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<NoticiaDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [AllowAnonymous]
    [HttpGet("categoria/{categoriaId}")]
    public async Task<ActionResult<IEnumerable<NoticiaDto>>> GetByCategoria(int categoriaId)
    {
        var items = await _service.GetByCategoriaAsync(categoriaId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<NoticiaDto>> Create(CriarNoticiaDto dto)
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
    public async Task<ActionResult<NoticiaDto>> Update(int id, AtualizarNoticiaDto dto)
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



