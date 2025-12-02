using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    private readonly IUsuarioRepository _repository;

    public UsuariosController(IUsuarioService service, IUsuarioRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    [HttpGet]
    [Authorize] // Requer autenticação
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    [Authorize] // Requer autenticação
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    // Endpoint público apenas se não existir nenhum usuário no banco
    // Caso contrário, requer autenticação
    public async Task<ActionResult<UsuarioDto>> Create(CriarUsuarioDto dto)
    {
        try
        {
            // Verificar se já existe algum usuário
            var existeUsuario = await _repository.ExisteAlgumUsuarioAsync();
            
            // Se já existir usuário, requer autenticação
            if (existeUsuario)
            {
                // Verificar se o usuário está autenticado
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Unauthorized("É necessário estar autenticado para criar novos usuários.");
                }
            }

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize] // Requer autenticação
    public async Task<ActionResult<UsuarioDto>> Update(int id, AtualizarUsuarioDto dto)
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
    [Authorize] // Requer autenticação
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

