using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IEventoOcorrenciaService
{
    Task<IEnumerable<EventoOcorrenciaDto>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<EventoOcorrenciaDto>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null);
    Task<EventoOcorrenciaDto?> GetByIdAsync(int id);
    Task<EventoOcorrenciaDto> CreateAsync(CriarEventoOcorrenciaDto dto);
    Task<EventoOcorrenciaDto> UpdateAsync(int id, AtualizarEventoOcorrenciaDto dto);
    Task DeleteAsync(int id);
    Task<int> GerarPorRecorrenciaAsync(int eventoId, DateTime dataInicio, DateTime dataFim);
}

public class EventoOcorrenciaService : IEventoOcorrenciaService
{
    private readonly IEventoOcorrenciaRepository _repository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IEscalaRepository _escalaRepository;

    public EventoOcorrenciaService(
        IEventoOcorrenciaRepository repository,
        IEventoRepository eventoRepository,
        IEscalaRepository escalaRepository)
    {
        _repository = repository;
        _eventoRepository = eventoRepository;
        _escalaRepository = escalaRepository;
    }

    public async Task<IEnumerable<EventoOcorrenciaDto>> GetByEventoAsync(int eventoId)
    {
        var items = await _repository.GetByEventoAsync(eventoId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<EventoOcorrenciaDto>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null)
    {
        var items = await _repository.GetByPeriodoAsync(dataInicio, dataFim, eventoId);
        return items.Select(MapToDto);
    }

    public async Task<EventoOcorrenciaDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<EventoOcorrenciaDto> CreateAsync(CriarEventoOcorrenciaDto dto)
    {
        var evento = await _eventoRepository.GetByIdAsync(dto.EventoId);
        if (evento == null) throw new ArgumentException("Evento não encontrado");

        if (dto.DataHoraFim.HasValue && dto.DataHoraFim.Value < dto.DataHoraInicio)
        {
            throw new ArgumentException("Data/hora fim não pode ser menor que a data/hora início");
        }

        var existeNoHorario = await _repository.ExistsOcorrenciaNoHorarioAsync(dto.EventoId, dto.DataHoraInicio);
        if (existeNoHorario)
        {
            throw new ArgumentException("Já existe ocorrência para este evento neste mesmo horário");
        }

        var entity = new EventoOcorrencia
        {
            EventoId = dto.EventoId,
            EventoRecorrenciaId = dto.EventoRecorrenciaId,
            DataHoraInicio = dto.DataHoraInicio,
            DataHoraFim = dto.DataHoraFim,
            Status = dto.Status,
            GeradaAutomaticamente = dto.GeradaAutomaticamente,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var createdFull = await _repository.GetByIdAsync(created.Id);
        return MapToDto(createdFull!);
    }

    public async Task<EventoOcorrenciaDto> UpdateAsync(int id, AtualizarEventoOcorrenciaDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Ocorrência não encontrada");

        if (dto.DataHoraFim.HasValue && dto.DataHoraFim.Value < dto.DataHoraInicio)
        {
            throw new ArgumentException("Data/hora fim não pode ser menor que a data/hora início");
        }

        if (entity.DataHoraInicio != dto.DataHoraInicio)
        {
            var existeNoHorario = await _repository.ExistsOcorrenciaNoHorarioAsync(entity.EventoId, dto.DataHoraInicio);
            if (existeNoHorario)
            {
                throw new ArgumentException("Já existe ocorrência para este evento neste mesmo horário");
            }
        }

        entity.DataHoraInicio = dto.DataHoraInicio;
        entity.DataHoraFim = dto.DataHoraFim;
        entity.Status = dto.Status;

        var updated = await _repository.UpdateAsync(entity);
        var updatedFull = await _repository.GetByIdAsync(updated.Id);
        return MapToDto(updatedFull!);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return;

        var escalas = await _escalaRepository.GetAllByEventoOcorrenciaAsync(id);
        if (escalas.Any())
        {
            throw new ArgumentException("Não é possível remover ocorrência que já possui escala(s)");
        }

        await _repository.DeleteAsync(id);
    }

    public async Task<int> GerarPorRecorrenciaAsync(int eventoId, DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim < dataInicio)
        {
            throw new ArgumentException("Data fim deve ser maior ou igual à data início");
        }

        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) throw new ArgumentException("Evento não encontrado");

        var recorrencias = (await _repository.GetRecorrenciasAtivasByEventoAsync(eventoId)).ToList();
        if (recorrencias.Count == 0)
        {
            throw new ArgumentException(
                "Este evento não possui recorrências configuradas. Edite o evento em Eventos e, na seção Recorrências, adicione ao menos uma (dia da semana, horário e periodicidade).");
        }

        var totalCriadas = 0;

        foreach (var recorrencia in recorrencias)
        {
            var inicioVigencia = recorrencia.DataInicioVigencia.Date;
            var fimVigencia = recorrencia.DataFimVigencia?.Date ?? DateTime.MaxValue.Date;

            var inicioFaixa = dataInicio.Date > inicioVigencia ? dataInicio.Date : inicioVigencia;
            var fimFaixa = dataFim.Date < fimVigencia ? dataFim.Date : fimVigencia;
            if (fimFaixa < inicioFaixa) continue;

            var datasOcorrencia = GerarDatasRecorrencia(inicioFaixa, fimFaixa, recorrencia.DiaSemana, recorrencia.Periodicidade);

            foreach (var dataBase in datasOcorrencia)
            {
                var dataHoraInicio = dataBase.Date.Add(recorrencia.HoraInicio);
                var dataHoraFim = recorrencia.HoraFim.HasValue
                    ? (DateTime?)dataBase.Date.Add(recorrencia.HoraFim.Value)
                    : null;

                var existe = await _repository.ExistsOcorrenciaNoHorarioAsync(eventoId, dataHoraInicio);
                if (existe) continue;

                var entity = new EventoOcorrencia
                {
                    EventoId = eventoId,
                    EventoRecorrenciaId = recorrencia.Id,
                    DataHoraInicio = dataHoraInicio,
                    DataHoraFim = dataHoraFim,
                    Status = StatusEventoOcorrencia.Confirmado,
                    GeradaAutomaticamente = true,
                    DataCriacao = DateTime.Now
                };

                await _repository.CreateAsync(entity);
                totalCriadas++;
            }
        }

        return totalCriadas;
    }

    private static IEnumerable<DateTime> GerarDatasRecorrencia(
        DateTime dataInicio,
        DateTime dataFim,
        DayOfWeek diaSemana,
        PeriodicidadeRecorrencia periodicidade)
    {
        var primeiraData = dataInicio;
        while (primeiraData.DayOfWeek != diaSemana)
        {
            primeiraData = primeiraData.AddDays(1);
        }

        var saltoDias = periodicidade switch
        {
            PeriodicidadeRecorrencia.Quinzenal => 14,
            PeriodicidadeRecorrencia.Mensal => 28, // aproximação semanal para geração inicial
            _ => 7
        };

        var atual = primeiraData;
        while (atual <= dataFim)
        {
            yield return atual;
            atual = atual.AddDays(saltoDias);
        }
    }

    private static EventoOcorrenciaDto MapToDto(EventoOcorrencia o)
    {
        return new EventoOcorrenciaDto
        {
            Id = o.Id,
            EventoId = o.EventoId,
            EventoTitulo = o.Evento?.Titulo ?? string.Empty,
            EventoRecorrenciaId = o.EventoRecorrenciaId,
            DataHoraInicio = o.DataHoraInicio,
            DataHoraFim = o.DataHoraFim,
            Status = o.Status,
            GeradaAutomaticamente = o.GeradaAutomaticamente,
            DataCriacao = o.DataCriacao,
            PossuiEscala = o.Escalas?.Any() == true,
            EscalaId = o.Escalas?.FirstOrDefault()?.Id
        };
    }
}
