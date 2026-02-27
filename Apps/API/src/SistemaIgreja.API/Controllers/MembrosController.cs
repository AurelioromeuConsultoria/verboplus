using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

/// <summary>
/// Endpoint público para cadastro de membros (formulário web sem autenticação)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MembrosController : ControllerBase
{
    private readonly IMembroCadastroService _service;

    public MembrosController(IMembroCadastroService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cadastro público de membros - cria Pessoa + perfil Membro
    /// </summary>
    [HttpPost("cadastro")]
    public async Task<ActionResult<CadastroMembroResultadoDto>> Cadastrar([FromBody] CadastroMembroDto dto)
    {
        var resultado = await _service.CadastrarAsync(dto);
        if (!resultado.Sucesso)
            return BadRequest(resultado);
        return Ok(resultado);
    }
}
