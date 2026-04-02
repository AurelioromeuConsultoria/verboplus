using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Pessoas;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using System.Security.Claims;

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
    [HttpGet("aniversariantes")]
    public async Task<ActionResult<IEnumerable<AniversarianteDto>>> GetAniversariantes(
        [FromQuery] int dias = 30,
        [FromQuery] int limite = 50,
        [FromQuery] int? mes = null)
    {
        var items = mes.HasValue
            ? await _service.GetAniversariantesPorMesAsync(mes.Value, limite)
            : await _service.GetProximosAniversariantesAsync(dias, limite);
        return Ok(items);
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
    /// Lista pessoas com paginação e filtros (server-side).
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<PessoaDto>>> GetPaged([FromQuery] PessoaPagedQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
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
    /// Visão consolidada 360° da pessoa (perfis, visitas, voluntariado, usuário)
    /// </summary>
    [HttpGet("{id}/360")]
    public async Task<ActionResult<Pessoa360Dto>> Get360(int id)
    {
        var result = await _service.Get360Async(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova pessoa
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PessoaDto>> Create(CriarPessoaDto dto)
    {
        if (!IsAdminUser())
        {
            return StatusCode(403, "Apenas administradores podem criar pessoas.");
        }

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
        if (!IsAdminUser())
        {
            return StatusCode(403, "Apenas administradores podem atualizar pessoas.");
        }

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
        if (!IsAdminUser())
        {
            return StatusCode(403, "Apenas administradores podem excluir pessoas.");
        }

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

    private bool IsAdminUser()
    {
        var tipoUsuarioId = User.FindFirstValue("TipoUsuarioId");
        return tipoUsuarioId == ((int)TipoUsuario.Admin).ToString() ||
               tipoUsuarioId == ((int)TipoUsuario.Ambos).ToString();
    }
}

