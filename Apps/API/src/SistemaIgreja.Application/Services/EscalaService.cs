using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IEscalaService
{
    Task<EscalaDto?> GetByIdAsync(int id);
    Task<EscalaDto?> GetByEventoOcorrenciaAsync(int eventoOcorrenciaId);
    Task<IEnumerable<SugestaoEscalaVoluntarioDto>> GetSugestoesAsync(int escalaId, int equipeId);
    Task<EscalaDto> CreateAsync(CriarEscalaDto dto, int usuarioId);
    Task<EscalaDto> UpdateAsync(int id, AtualizarEscalaDto dto);
    Task DeleteAsync(int id);

    Task<EscalaItemDto> AddItemAsync(int escalaId, CriarEscalaItemDto dto, int usuarioId, bool isAdmin);
    Task<EscalaItemDto> UpdateItemAsync(int escalaId, int escalaItemId, AtualizarEscalaItemDto dto, int usuarioId, bool isAdmin);
    Task DeleteItemAsync(int escalaId, int escalaItemId);
    Task<EscalaDto> PublicarAsync(int escalaId);
}

public class EscalaService : IEscalaService
{
    private readonly IEscalaRepository _repository;
    private readonly IEventoOcorrenciaRepository _eventoOcorrenciaRepository;
    private readonly IVoluntarioRepository _voluntarioRepository;

    public EscalaService(
        IEscalaRepository repository,
        IEventoOcorrenciaRepository eventoOcorrenciaRepository,
        IVoluntarioRepository voluntarioRepository)
    {
        _repository = repository;
        _eventoOcorrenciaRepository = eventoOcorrenciaRepository;
        _voluntarioRepository = voluntarioRepository;
    }

    public async Task<EscalaDto?> GetByIdAsync(int id)
    {
        var escala = await _repository.GetByIdAsync(id);
        return escala != null ? MapToDto(escala) : null;
    }

    public async Task<EscalaDto?> GetByEventoOcorrenciaAsync(int eventoOcorrenciaId)
    {
        var escala = await _repository.GetByEventoOcorrenciaIdAsync(eventoOcorrenciaId);
        return escala != null ? MapToDto(escala) : null;
    }

    public async Task<IEnumerable<SugestaoEscalaVoluntarioDto>> GetSugestoesAsync(int escalaId, int equipeId)
    {
        var escala = await _repository.GetByIdAsync(escalaId);
        if (escala == null) throw new ArgumentException("Escala não encontrada");

        var voluntarios = (await _voluntarioRepository.GetByEquipeAsync(equipeId)).ToList();
        var pessoaIdsJaEscaladas = await _repository.GetPessoaIdsJaEscaladasAsync(escalaId);
        var cargaRecente = await _repository.GetCargaRecentePorVoluntarioAsync(equipeId, DateTime.Now.AddDays(-60));

        var sugestoes = voluntarios
            .Select(v =>
            {
                var indisponivel = pessoaIdsJaEscaladas.Contains(v.PessoaId);
                var carga = cargaRecente.TryGetValue(v.Id, out var qtd) ? qtd : 0;
                return new SugestaoEscalaVoluntarioDto
                {
                    VoluntarioId = v.Id,
                    PessoaId = v.PessoaId,
                    VoluntarioNome = v.Pessoa?.Nome ?? string.Empty,
                    EquipeId = v.EquipeId,
                    EquipeNome = v.Equipe?.Nome ?? string.Empty,
                    CargoId = v.CargoId,
                    CargoNome = v.Cargo?.Nome ?? string.Empty,
                    Disponivel = !indisponivel,
                    CargaRecente = carga,
                    MotivoBloqueio = indisponivel ? "Já escalado neste evento" : null
                };
            })
            .OrderByDescending(s => s.Disponivel)
            .ThenBy(s => s.CargaRecente)
            .ThenBy(s => s.VoluntarioNome)
            .ToList();

        return sugestoes;
    }

    public async Task<EscalaDto> CreateAsync(CriarEscalaDto dto, int usuarioId)
    {
        var ocorrencia = await _eventoOcorrenciaRepository.GetByIdAsync(dto.EventoOcorrenciaId);
        if (ocorrencia == null) throw new ArgumentException("Ocorrência não encontrada");

        var existente = await _repository.GetByEventoOcorrenciaIdAsync(dto.EventoOcorrenciaId);
        if (existente != null) throw new ArgumentException("Já existe escala para esta ocorrência");

        var escala = new Escala
        {
            EventoOcorrenciaId = dto.EventoOcorrenciaId,
            Status = StatusEscala.Rascunho,
            Observacoes = dto.Observacoes,
            CriadoPorUsuarioId = usuarioId > 0 ? usuarioId : null,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(escala);
        var createdFull = await _repository.GetByIdAsync(created.Id);
        return MapToDto(createdFull!);
    }

    public async Task<EscalaDto> UpdateAsync(int id, AtualizarEscalaDto dto)
    {
        var escala = await _repository.GetByIdAsync(id);
        if (escala == null) throw new ArgumentException("Escala não encontrada");

        escala.Status = dto.Status;
        escala.Observacoes = dto.Observacoes;

        var updated = await _repository.UpdateAsync(escala);
        var updatedFull = await _repository.GetByIdAsync(updated.Id);
        return MapToDto(updatedFull!);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<EscalaItemDto> AddItemAsync(int escalaId, CriarEscalaItemDto dto, int usuarioId, bool isAdmin)
    {
        var escala = await _repository.GetByIdAsync(escalaId);
        if (escala == null) throw new ArgumentException("Escala não encontrada");
        if (escala.Status == StatusEscala.Fechada) throw new ArgumentException("Escala fechada não pode ser alterada");

        var voluntario = await _voluntarioRepository.GetByIdAsync(dto.VoluntarioId);
        if (voluntario == null) throw new ArgumentException("Voluntário não encontrado");

        await ValidarConflitoPessoaAsync(escalaId, dto.VoluntarioId, dto.ForcarConflito, dto.MotivoExcecao, usuarioId, isAdmin, null);

        var item = new EscalaItem
        {
            EscalaId = escalaId,
            EquipeId = dto.EquipeId,
            CargoId = dto.CargoId,
            VoluntarioId = dto.VoluntarioId,
            Ordem = dto.Ordem,
            ConflitoAprovado = dto.ForcarConflito,
            MotivoExcecao = dto.ForcarConflito ? dto.MotivoExcecao?.Trim() : null,
            AprovadoPorUsuarioId = dto.ForcarConflito ? usuarioId : null,
            AprovadoEm = dto.ForcarConflito ? DateTime.Now : null,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.AddItemAsync(item);
        var createdFull = await _repository.GetItemByIdAsync(created.Id);
        return MapItemToDto(createdFull!);
    }

    public async Task<EscalaItemDto> UpdateItemAsync(int escalaId, int escalaItemId, AtualizarEscalaItemDto dto, int usuarioId, bool isAdmin)
    {
        var escala = await _repository.GetByIdAsync(escalaId);
        if (escala == null) throw new ArgumentException("Escala não encontrada");
        if (escala.Status == StatusEscala.Fechada) throw new ArgumentException("Escala fechada não pode ser alterada");

        var item = await _repository.GetItemByIdAsync(escalaItemId);
        if (item == null || item.EscalaId != escalaId) throw new ArgumentException("Item da escala não encontrado");

        var voluntario = await _voluntarioRepository.GetByIdAsync(dto.VoluntarioId);
        if (voluntario == null) throw new ArgumentException("Voluntário não encontrado");

        await ValidarConflitoPessoaAsync(escalaId, dto.VoluntarioId, dto.ForcarConflito, dto.MotivoExcecao, usuarioId, isAdmin, escalaItemId);

        item.EquipeId = dto.EquipeId;
        item.CargoId = dto.CargoId;
        item.VoluntarioId = dto.VoluntarioId;
        item.Ordem = dto.Ordem;
        item.ConflitoAprovado = dto.ForcarConflito;
        item.MotivoExcecao = dto.ForcarConflito ? dto.MotivoExcecao?.Trim() : null;
        item.AprovadoPorUsuarioId = dto.ForcarConflito ? usuarioId : null;
        item.AprovadoEm = dto.ForcarConflito ? DateTime.Now : null;

        var updated = await _repository.UpdateItemAsync(item);
        var updatedFull = await _repository.GetItemByIdAsync(updated.Id);
        return MapItemToDto(updatedFull!);
    }

    public async Task DeleteItemAsync(int escalaId, int escalaItemId)
    {
        var escala = await _repository.GetByIdAsync(escalaId);
        if (escala == null) throw new ArgumentException("Escala não encontrada");
        if (escala.Status == StatusEscala.Fechada) throw new ArgumentException("Escala fechada não pode ser alterada");

        var item = await _repository.GetItemByIdAsync(escalaItemId);
        if (item == null || item.EscalaId != escalaId) throw new ArgumentException("Item da escala não encontrado");

        await _repository.DeleteItemAsync(escalaItemId);
    }

    public async Task<EscalaDto> PublicarAsync(int escalaId)
    {
        var escala = await _repository.GetByIdAsync(escalaId);
        if (escala == null) throw new ArgumentException("Escala não encontrada");

        if (!escala.Itens.Any())
        {
            throw new ArgumentException("Não é possível publicar escala sem itens");
        }

        escala.Status = StatusEscala.Publicada;
        escala.DataPublicacao = DateTime.Now;

        var updated = await _repository.UpdateAsync(escala);
        var updatedFull = await _repository.GetByIdAsync(updated.Id);
        return MapToDto(updatedFull!);
    }

    private async Task ValidarConflitoPessoaAsync(
        int escalaId,
        int voluntarioId,
        bool forcarConflito,
        string? motivoExcecao,
        int usuarioId,
        bool isAdmin,
        int? ignorarEscalaItemId)
    {
        var conflito = await _repository.GetConflitoPessoaNaEscalaAsync(escalaId, voluntarioId, ignorarEscalaItemId);
        if (conflito == null) return;

        if (!forcarConflito)
        {
            var nome = conflito.Voluntario?.Pessoa?.Nome ?? "Voluntário";
            var equipe = conflito.Equipe?.Nome ?? "Equipe";
            throw new ArgumentException($"{nome} já está escalado neste evento pela equipe '{equipe}'.");
        }

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Apenas administradores podem aprovar exceção de conflito.");
        }

        if (string.IsNullOrWhiteSpace(motivoExcecao))
        {
            throw new ArgumentException("Motivo da exceção é obrigatório ao forçar conflito.");
        }

        if (usuarioId <= 0)
        {
            throw new ArgumentException("Usuário aprovador inválido.");
        }
    }

    private static EscalaDto MapToDto(Escala e)
    {
        return new EscalaDto
        {
            Id = e.Id,
            EventoOcorrenciaId = e.EventoOcorrenciaId,
            EventoDataHoraInicio = e.EventoOcorrencia?.DataHoraInicio ?? DateTime.MinValue,
            EventoTitulo = e.EventoOcorrencia?.Evento?.Titulo ?? string.Empty,
            Status = e.Status,
            Observacoes = e.Observacoes,
            CriadoPorUsuarioId = e.CriadoPorUsuarioId,
            CriadoPorUsuarioNome = e.CriadoPorUsuario?.Pessoa?.Nome,
            DataCriacao = e.DataCriacao,
            DataPublicacao = e.DataPublicacao,
            Itens = e.Itens
                .OrderBy(i => i.Ordem)
                .ThenBy(i => i.Id)
                .Select(MapItemToDto)
                .ToList()
        };
    }

    private static EscalaItemDto MapItemToDto(EscalaItem i)
    {
        return new EscalaItemDto
        {
            Id = i.Id,
            EscalaId = i.EscalaId,
            EquipeId = i.EquipeId,
            EquipeNome = i.Equipe?.Nome ?? string.Empty,
            CargoId = i.CargoId,
            CargoNome = i.Cargo?.Nome,
            VoluntarioId = i.VoluntarioId,
            VoluntarioPessoaId = i.Voluntario?.PessoaId ?? 0,
            VoluntarioNome = i.Voluntario?.Pessoa?.Nome ?? string.Empty,
            Ordem = i.Ordem,
            ConflitoAprovado = i.ConflitoAprovado,
            MotivoExcecao = i.MotivoExcecao,
            AprovadoPorUsuarioId = i.AprovadoPorUsuarioId,
            AprovadoPorUsuarioNome = i.AprovadoPorUsuario?.Pessoa?.Nome,
            AprovadoEm = i.AprovadoEm,
            DataCriacao = i.DataCriacao
        };
    }
}
