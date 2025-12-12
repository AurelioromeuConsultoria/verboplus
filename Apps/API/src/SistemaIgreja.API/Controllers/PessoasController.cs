using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação para acessar
public class PessoasController : ControllerBase
{
    private readonly IPessoaService _service;

    public PessoasController(IPessoaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as pessoas com seus perfis
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaDto>>> GetAll()
    {
        var pessoas = await _service.GetAllAsync();
        return Ok(pessoas);
    }

    /// <summary>
    /// Obtém detalhe de uma pessoa específica com seus perfis
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PessoaDto>> GetById(int id)
    {
        var pessoa = await _service.GetByIdAsync(id);
        if (pessoa == null)
            return NotFound();

        return Ok(pessoa);
    }

    /// <summary>
    /// Cria uma nova pessoa
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PessoaDto>> Create(CriarPessoaDto dto)
    {
        try
        {
            var pessoa = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pessoa.Id }, pessoa);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao criar pessoa", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza dados de uma pessoa
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PessoaDto>> Update(int id, AtualizarPessoaDto dto)
    {
        try
        {
            var pessoa = await _service.UpdateAsync(id, dto);
            return Ok(pessoa);
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
    /// Remove uma pessoa
    /// </summary>
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
            return BadRequest(new { message = ex.Message });
        }
    }
}



