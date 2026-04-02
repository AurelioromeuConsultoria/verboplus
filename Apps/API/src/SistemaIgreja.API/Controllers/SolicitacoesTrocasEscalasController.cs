using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacoesTrocasEscalasController : ControllerBase
{
    private readonly ISolicitacaoTrocaEscalaService _service;
    private readonly IUsuarioRepository _usuarioRepository;

    public SolicitacoesTrocasEscalasController(ISolicitacaoTrocaEscalaService service, IUsuarioRepository usuarioRepository)
    {
        _service = service;
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet("escala/{escalaId}")]
    public async Task<ActionResult<IEnumerable<SolicitacaoTrocaEscalaDto>>> GetByEscala(int escalaId)
    {
        try
        {
            var items = await _service.GetByEscalaAsync(escalaId, GetUsuarioId(), IsAdminUser());
            return Ok(items);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitacaoTrocaEscalaDto>>> GetGerenciaveis([FromQuery] int? equipeId, [FromQuery] StatusSolicitacaoTrocaEscala? status)
    {
        var items = await _service.GetGerenciaveisAsync(GetUsuarioId(), IsAdminUser(), equipeId, status);
        return Ok(items);
    }

    [HttpGet("minhas")]
    public async Task<ActionResult<IEnumerable<SolicitacaoTrocaEscalaDto>>> GetMinhas()
    {
        var pessoaId = await GetUsuarioPessoaIdAsync();
        if (!pessoaId.HasValue) return Unauthorized();

        var items = await _service.GetMinhasAsync(pessoaId.Value);
        return Ok(items);
    }

    [HttpPost("escala/{escalaId}/item/{escalaItemId}")]
    public async Task<ActionResult<SolicitacaoTrocaEscalaDto>> Create(int escalaId, int escalaItemId, CriarSolicitacaoTrocaEscalaDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(escalaId, escalaItemId, dto, GetUsuarioId(), IsAdminUser(), await GetUsuarioPessoaIdAsync());
            return Ok(created);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/aprovar")]
    public async Task<ActionResult<SolicitacaoTrocaEscalaDto>> Aprovar(int id, AprovarSolicitacaoTrocaEscalaDto dto)
    {
        try
        {
            var item = await _service.AprovarAsync(id, dto, GetUsuarioId(), IsAdminUser());
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/rejeitar")]
    public async Task<ActionResult<SolicitacaoTrocaEscalaDto>> Rejeitar(int id, RejeitarSolicitacaoTrocaEscalaDto dto)
    {
        try
        {
            var item = await _service.RejeitarAsync(id, dto, GetUsuarioId(), IsAdminUser());
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
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

    private async Task<int?> GetUsuarioPessoaIdAsync()
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId <= 0) return null;
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        return usuario?.PessoaId;
    }
}
