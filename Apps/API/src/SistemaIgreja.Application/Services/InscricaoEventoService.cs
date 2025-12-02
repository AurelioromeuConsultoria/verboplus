using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IInscricaoEventoService
{
    Task<IEnumerable<InscricaoEventoDto>> GetAllAsync();
    Task<InscricaoEventoDto?> GetByIdAsync(int id);
    Task<IEnumerable<InscricaoEventoDto>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<InscricaoEventoDto>> GetByStatusAsync(StatusInscricao status);
    Task<InscricaoEventoDto> CreateAsync(CriarInscricaoEventoDto dto);
    Task<InscricaoEventoDto> UpdateAsync(int id, AtualizarInscricaoEventoDto dto);
    Task<InscricaoEventoDto> ConfirmarInscricaoAsync(int id);
    Task<InscricaoEventoDto> CancelarInscricaoAsync(int id);
    Task<EstatisticasInscricaoDto> ObterEstatisticasAsync(int eventoId);
    Task DeleteAsync(int id);
}

public class InscricaoEventoService : IInscricaoEventoService
{
    private readonly IInscricaoEventoRepository _repository;
    private readonly IEventoRepository _eventoRepository;

    public InscricaoEventoService(IInscricaoEventoRepository repository, IEventoRepository eventoRepository)
    {
        _repository = repository;
        _eventoRepository = eventoRepository;
    }

    public async Task<IEnumerable<InscricaoEventoDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<InscricaoEventoDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<InscricaoEventoDto>> GetByEventoAsync(int eventoId)
    {
        var entities = await _repository.GetByEventoAsync(eventoId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<InscricaoEventoDto>> GetByStatusAsync(StatusInscricao status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<InscricaoEventoDto> CreateAsync(CriarInscricaoEventoDto dto)
    {
        // Verificar se o evento existe
        var evento = await _eventoRepository.GetByIdAsync(dto.EventoId);
        if (evento == null) throw new ArgumentException("Evento não encontrado");

        // Verificar se o evento ainda aceita inscrições
        if (evento.DataInicio < DateTime.Now)
            throw new InvalidOperationException("Não é possível se inscrever em um evento que já iniciou");

        // Verificar se já existe inscrição com o mesmo WhatsApp
        var existeInscricao = await _repository.ExisteInscricaoAsync(dto.EventoId, dto.WhatsApp);
        if (existeInscricao)
            throw new InvalidOperationException("Já existe uma inscrição para este WhatsApp neste evento");

        var entity = new InscricaoEvento
        {
            EventoId = dto.EventoId,
            Nome = dto.Nome,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            QuantidadeAcompanhantes = dto.QuantidadeAcompanhantes,
            Observacoes = dto.Observacoes,
            Status = StatusInscricao.Pendente,
            DataInscricao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<InscricaoEventoDto> UpdateAsync(int id, AtualizarInscricaoEventoDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Inscrição não encontrada");

        entity.Status = dto.Status;
        entity.QuantidadeAcompanhantes = dto.QuantidadeAcompanhantes;
        entity.Observacoes = dto.Observacoes;
        entity.ObservacoesInternas = dto.ObservacoesInternas;

        // Atualizar datas conforme status
        if (dto.Status == StatusInscricao.Confirmada && entity.DataConfirmacao == null)
            entity.DataConfirmacao = DateTime.Now;
        else if (dto.Status == StatusInscricao.Cancelada && entity.DataCancelamento == null)
            entity.DataCancelamento = DateTime.Now;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<InscricaoEventoDto> ConfirmarInscricaoAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Inscrição não encontrada");

        entity.Status = StatusInscricao.Confirmada;
        entity.DataConfirmacao = DateTime.Now;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<InscricaoEventoDto> CancelarInscricaoAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Inscrição não encontrada");

        entity.Status = StatusInscricao.Cancelada;
        entity.DataCancelamento = DateTime.Now;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<EstatisticasInscricaoDto> ObterEstatisticasAsync(int eventoId)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) throw new ArgumentException("Evento não encontrado");

        var inscricoes = await _repository.GetByEventoAsync(eventoId);
        var inscricoesList = inscricoes.ToList();

        return new EstatisticasInscricaoDto
        {
            EventoId = eventoId,
            EventoTitulo = evento.Titulo,
            TotalInscricoes = inscricoesList.Count,
            InscricoesConfirmadas = inscricoesList.Count(i => i.Status == StatusInscricao.Confirmada),
            InscricoesPendentes = inscricoesList.Count(i => i.Status == StatusInscricao.Pendente),
            InscricoesCanceladas = inscricoesList.Count(i => i.Status == StatusInscricao.Cancelada),
            TotalParticipantes = inscricoesList.Sum(i => 1 + i.QuantidadeAcompanhantes)
        };
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static string GetStatusDescricao(StatusInscricao status)
    {
        return status switch
        {
            StatusInscricao.Pendente => "Pendente",
            StatusInscricao.Confirmada => "Confirmada",
            StatusInscricao.Cancelada => "Cancelada",
            StatusInscricao.Presente => "Presente",
            _ => "Desconhecido"
        };
    }

    private static InscricaoEventoDto MapToDto(InscricaoEvento i)
    {
        return new InscricaoEventoDto
        {
            Id = i.Id,
            EventoId = i.EventoId,
            EventoTitulo = i.Evento?.Titulo,
            Nome = i.Nome,
            WhatsApp = i.WhatsApp,
            Email = i.Email,
            Status = i.Status,
            StatusDescricao = GetStatusDescricao(i.Status),
            QuantidadeAcompanhantes = i.QuantidadeAcompanhantes,
            Observacoes = i.Observacoes,
            ObservacoesInternas = i.ObservacoesInternas,
            DataInscricao = i.DataInscricao,
            DataConfirmacao = i.DataConfirmacao,
            DataCancelamento = i.DataCancelamento
        };
    }
}


