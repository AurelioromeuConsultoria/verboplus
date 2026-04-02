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
public class EscalasController : ControllerBase
{
    private readonly IEscalaService _service;
    private readonly IUsuarioRepository _usuarioRepository;

    public EscalasController(IEscalaService service, IUsuarioRepository usuarioRepository)
    {
        _service = service;
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscalaDto>> GetById(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id, GetUsuarioId(), IsAdminUser());
            if (item == null) return NotFound();
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}")]
    public async Task<ActionResult<EscalaDto>> GetByEventoOcorrencia(int eventoOcorrenciaId)
    {
        try
        {
            var item = await _service.GetByEventoOcorrenciaAsync(eventoOcorrenciaId, GetUsuarioId(), IsAdminUser());
            if (item == null) return NotFound();
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}/escalas")]
    public async Task<ActionResult<IEnumerable<EscalaDto>>> GetAllByEventoOcorrencia(int eventoOcorrenciaId)
    {
        var items = await _service.GetAllByEventoOcorrenciaAsync(eventoOcorrenciaId, GetUsuarioId(), IsAdminUser());
        return Ok(items);
    }

    [HttpGet("ocorrencia/{eventoOcorrenciaId}/equipe/{equipeId}")]
    public async Task<ActionResult<EscalaDto>> GetByEventoOcorrenciaAndEquipe(int eventoOcorrenciaId, int equipeId)
    {
        try
        {
            var item = await _service.GetByEventoOcorrenciaAndEquipeAsync(eventoOcorrenciaId, equipeId, GetUsuarioId(), IsAdminUser());
            if (item == null) return NotFound();
            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet("minhas")]
    public async Task<ActionResult<IEnumerable<EscalaDto>>> GetMinhas([FromQuery] bool somenteFuturas = true)
    {
        var usuarioId = GetUsuarioId();
        var usuarioPessoaId = await GetUsuarioPessoaIdAsync(usuarioId);
        if (!usuarioPessoaId.HasValue)
        {
            return Unauthorized();
        }

        var items = await _service.GetMinhasEscalasAsync(usuarioPessoaId.Value, somenteFuturas);
        return Ok(items);
    }

    [HttpGet("historico-voluntarios")]
    public async Task<ActionResult<IEnumerable<HistoricoVoluntarioDto>>> GetHistoricoVoluntarios(
        [FromQuery] int? equipeId = null,
        [FromQuery] int? eventoId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        var items = await _service.GetHistoricoVoluntariosAsync(GetUsuarioId(), IsAdminUser(), equipeId, eventoId, dataInicio, dataFim);
        return Ok(items);
    }

    [HttpGet("{escalaId}/sugestoes")]
    public async Task<ActionResult<IEnumerable<SugestaoEscalaVoluntarioDto>>> GetSugestoes(int escalaId, [FromQuery] int equipeId)
    {
        try
        {
            var itens = await _service.GetSugestoesAsync(escalaId, equipeId, GetUsuarioId(), IsAdminUser());
            return Ok(itens);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
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
            var created = await _service.CreateAsync(dto, usuarioId, IsAdminUser());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

    [HttpPut("{id}")]
    public async Task<ActionResult<EscalaDto>> Update(int id, AtualizarEscalaDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto, GetUsuarioId(), IsAdminUser());
            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
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
            await _service.DeleteAsync(id, GetUsuarioId(), IsAdminUser());
            return NoContent();
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
            await _service.DeleteItemAsync(escalaId, escalaItemId, GetUsuarioId(), IsAdminUser());
            return NoContent();
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

    [HttpPost("{escalaId}/publicar")]
    public async Task<ActionResult<EscalaDto>> Publicar(int escalaId)
    {
        try
        {
            var updated = await _service.PublicarAsync(escalaId, GetUsuarioId(), IsAdminUser());
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

    [HttpPost("ocorrencia/{eventoOcorrenciaId}/equipe/{equipeId}/gerar-automatico")]
    public async Task<ActionResult<EscalaDto>> GerarAutomatico(int eventoOcorrenciaId, int equipeId)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var escala = await _service.GerarAutomaticoAsync(eventoOcorrenciaId, equipeId, usuarioId, IsAdminUser());
            return Ok(escala);
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

    [HttpPost("{escalaId}/itens/{escalaItemId}/confirmar")]
    public async Task<ActionResult<EscalaItemDto>> ConfirmarItem(int escalaId, int escalaItemId)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var usuarioPessoaId = await GetUsuarioPessoaIdAsync(usuarioId);
            var item = await _service.ConfirmarItemAsync(escalaId, escalaItemId, usuarioId, IsAdminUser(), usuarioPessoaId);
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

    [HttpPost("{escalaId}/itens/{escalaItemId}/recusar")]
    public async Task<ActionResult<EscalaItemDto>> RecusarItem(int escalaId, int escalaItemId, [FromBody] RecusarEscalaItemDto dto)
    {
        try
        {
            var usuarioId = GetUsuarioId();
            var usuarioPessoaId = await GetUsuarioPessoaIdAsync(usuarioId);
            var item = await _service.RecusarItemAsync(
                escalaId,
                escalaItemId,
                dto?.MotivoRecusa,
                usuarioId,
                IsAdminUser(),
                usuarioPessoaId);
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

    [HttpPost("{escalaId}/itens/{escalaItemId}/presenca")]
    public async Task<ActionResult<EscalaItemDto>> RegistrarPresenca(int escalaId, int escalaItemId, [FromBody] RegistrarPresencaEscalaItemDto dto)
    {
        try
        {
            var item = await _service.RegistrarPresencaAsync(
                escalaId,
                escalaItemId,
                dto.Compareceu,
                dto.ObservacaoOperacional,
                GetUsuarioId(),
                IsAdminUser());
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

    [HttpPost("lembretes/processar")]
    public async Task<ActionResult<object>> ProcessarLembretes()
    {
        if (!IsAdminUser())
        {
            return StatusCode(403, "Apenas administradores podem processar lembretes manualmente.");
        }

        var total = await _service.EnviarLembretesPendentesAsync();
        return Ok(new { totalEnviados = total });
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

    private async Task<int?> GetUsuarioPessoaIdAsync(int usuarioId)
    {
        if (usuarioId <= 0)
        {
            return null;
        }

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        return usuario?.PessoaId;
    }
}
