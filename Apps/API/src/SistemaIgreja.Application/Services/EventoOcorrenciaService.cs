using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IEventoOcorrenciaService
{
    Task<IEnumerable<EventoOcorrenciaDto>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<EventoOcorrenciaDto>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null);
    Task<IEnumerable<CoberturaVoluntariadoOcorrenciaDto>> GetCoberturaVoluntariadoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null, string? nivelRisco = null);
    Task<EventoOcorrenciaDto?> GetByIdAsync(int id);
    Task<EventoOcorrenciaDto> CreateAsync(CriarEventoOcorrenciaDto dto);
    Task<EventoOcorrenciaDto> UpdateAsync(int id, AtualizarEventoOcorrenciaDto dto);
    Task DeleteAsync(int id);
    Task<GerarOcorrenciasResultadoDto> GerarPorRecorrenciaAsync(int eventoId, DateTime dataInicio, DateTime dataFim, bool reconciliarFuturasSemEscala = false);
}

public class EventoOcorrenciaService : IEventoOcorrenciaService
{
    private readonly IEventoOcorrenciaRepository _repository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IEscalaRepository _escalaRepository;
    private readonly ITenantContext _tenantContext;

    public EventoOcorrenciaService(
        IEventoOcorrenciaRepository repository,
        IEventoRepository eventoRepository,
        IEscalaRepository escalaRepository)
        : this(repository, eventoRepository, escalaRepository, new DefaultTenantContext())
    {
    }

    public EventoOcorrenciaService(
        IEventoOcorrenciaRepository repository,
        IEventoRepository eventoRepository,
        IEscalaRepository escalaRepository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _eventoRepository = eventoRepository;
        _escalaRepository = escalaRepository;
        _tenantContext = tenantContext;
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

    public async Task<IEnumerable<CoberturaVoluntariadoOcorrenciaDto>> GetCoberturaVoluntariadoAsync(DateTime dataInicio, DateTime dataFim, int? eventoId = null, string? nivelRisco = null)
    {
        var items = (await _repository.GetByPeriodoAsync(dataInicio, dataFim, eventoId)).ToList();
        var result = new List<CoberturaVoluntariadoOcorrenciaDto>();

        foreach (var ocorrencia in items)
        {
            var escalas = (await _escalaRepository.GetAllByEventoOcorrenciaAsync(ocorrencia.Id)).ToList();
            var dto = MapToCoberturaDto(ocorrencia, escalas);
            result.Add(dto);
        }

        if (!string.IsNullOrWhiteSpace(nivelRisco) && !string.Equals(nivelRisco, "all", StringComparison.OrdinalIgnoreCase))
        {
            result = result
                .Where(x => string.Equals(x.NivelRisco, nivelRisco, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return result
            .OrderBy(x => x.OrdemRisco)
            .ThenBy(x => x.DataHoraInicio)
            .ToList();
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
            TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId,
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

    public async Task<GerarOcorrenciasResultadoDto> GerarPorRecorrenciaAsync(int eventoId, DateTime dataInicio, DateTime dataFim, bool reconciliarFuturasSemEscala = false)
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

        var horariosEsperados = new HashSet<DateTime>();
        var totalCriadas = 0;

        foreach (var recorrencia in recorrencias)
        {
            var inicioVigencia = recorrencia.DataInicioVigencia.Date;
            var fimVigencia = recorrencia.DataFimVigencia?.Date ?? DateTime.MaxValue.Date;

            var inicioFaixa = dataInicio.Date > inicioVigencia ? dataInicio.Date : inicioVigencia;
            var fimFaixa = dataFim.Date < fimVigencia ? dataFim.Date : fimVigencia;
            if (fimFaixa < inicioFaixa) continue;

            var datasOcorrencia = GerarDatasRecorrencia(
                inicioFaixa,
                fimFaixa,
                recorrencia.DiaSemana,
                recorrencia.Periodicidade,
                inicioVigencia);

            var semanasExcluidas = ParseSemanasExcluidas(recorrencia.SemanasDoMesExcluidas);

            foreach (var dataBase in datasOcorrencia)
            {
                // Ex.: "todo domingo, exceto o 2º domingo do mês" -> pula a semana 2.
                if (semanasExcluidas.Contains(SemanaDoMes(dataBase))) continue;

                var dataHoraInicio = dataBase.Date.Add(recorrencia.HoraInicio);
                horariosEsperados.Add(dataHoraInicio);

                var existe = await _repository.ExistsOcorrenciaNoHorarioAsync(eventoId, dataHoraInicio);
                if (existe) continue;

                var dataHoraFim = recorrencia.HoraFim.HasValue
                    ? (DateTime?)dataBase.Date.Add(recorrencia.HoraFim.Value)
                    : null;

                var entity = new EventoOcorrencia
                {
                    TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId,
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

        var (totalRemovidas, totalNaoReconciliadas) = reconciliarFuturasSemEscala
            ? await ReconciliarFuturasSemEscalaAsync(eventoId, dataInicio, dataFim, horariosEsperados)
            : (0, 0);

        return new GerarOcorrenciasResultadoDto
        {
            TotalCriadas = totalCriadas,
            TotalRemovidas = totalRemovidas,
            TotalNaoReconciliadas = totalNaoReconciliadas
        };
    }

    // Remove ocorrências FUTURAS geradas automaticamente que não correspondem mais a nenhuma
    // recorrência ativa (ex.: recorrência alterada, desativada ou apagada, semana excluída).
    // Preserva o passado e ocorrências criadas manualmente.
    // Escalas vazias (rascunho sem itens) são removidas em cascade junto com a ocorrência.
    // Ocorrências com escala que já tem voluntários alocados são preservadas e contadas em NaoReconciliadas.
    private async Task<(int Removidas, int NaoReconciliadas)> ReconciliarFuturasSemEscalaAsync(
        int eventoId,
        DateTime dataInicio,
        DateTime dataFim,
        HashSet<DateTime> horariosEsperados)
    {
        var agora = DateTime.Now;
        var ocorrencias = await _repository.GetByPeriodoAsync(dataInicio, dataFim, eventoId);

        var removidas = 0;
        var naoReconciliadas = 0;
        foreach (var ocorrencia in ocorrencias)
        {
            if (!ocorrencia.GeradaAutomaticamente) continue;
            if (ocorrencia.DataHoraInicio < agora) continue;
            if (horariosEsperados.Contains(ocorrencia.DataHoraInicio)) continue;

            if (ocorrencia.Escalas?.Any() == true)
            {
                var escalasDetalhadas = await _escalaRepository.GetAllByEventoOcorrenciaAsync(ocorrencia.Id);
                if (escalasDetalhadas.Any(e => e.Itens.Any()))
                {
                    naoReconciliadas++;
                    continue;
                }
                foreach (var escala in escalasDetalhadas)
                    await _escalaRepository.DeleteAsync(escala.Id);
            }

            await _repository.DeleteAsync(ocorrencia.Id);
            removidas++;
        }

        return (removidas, naoReconciliadas);
    }

    private static IEnumerable<DateTime> GerarDatasRecorrencia(
        DateTime dataInicio,
        DateTime dataFim,
        DayOfWeek diaSemana,
        PeriodicidadeRecorrencia periodicidade,
        DateTime dataInicioVigencia)
    {
        if (periodicidade == PeriodicidadeRecorrencia.Mensal)
        {
            foreach (var data in GerarDatasMensais(dataInicio, dataFim, diaSemana, dataInicioVigencia))
            {
                yield return data;
            }

            yield break;
        }

        var saltoDias = periodicidade switch
        {
            PeriodicidadeRecorrencia.Quinzenal => 14,
            _ => 7
        };

        var atual = ProximaDataNoDiaSemana(dataInicioVigencia, diaSemana);
        while (atual < dataInicio)
        {
            atual = atual.AddDays(saltoDias);
        }

        while (atual <= dataFim)
        {
            yield return atual;
            atual = atual.AddDays(saltoDias);
        }
    }

    private static IEnumerable<DateTime> GerarDatasMensais(
        DateTime dataInicio,
        DateTime dataFim,
        DayOfWeek diaSemana,
        DateTime dataInicioVigencia)
    {
        var dataReferencia = ProximaDataNoDiaSemana(dataInicioVigencia, diaSemana);
        var semanaDoMes = ((dataReferencia.Day - 1) / 7) + 1;
        var mesAtual = new DateTime(dataInicio.Year, dataInicio.Month, 1);
        var fimMes = new DateTime(dataFim.Year, dataFim.Month, 1);

        while (mesAtual <= fimMes)
        {
            var data = ObterDataPorSemanaDoMes(mesAtual.Year, mesAtual.Month, diaSemana, semanaDoMes);
            if (data.HasValue && data.Value >= dataInicio && data.Value <= dataFim)
            {
                yield return data.Value;
            }

            mesAtual = mesAtual.AddMonths(1);
        }
    }

    private static DateTime ProximaDataNoDiaSemana(DateTime data, DayOfWeek diaSemana)
    {
        var atual = data.Date;
        while (atual.DayOfWeek != diaSemana)
        {
            atual = atual.AddDays(1);
        }

        return atual;
    }

    private static DateTime? ObterDataPorSemanaDoMes(int ano, int mes, DayOfWeek diaSemana, int semanaDoMes)
    {
        var primeira = ProximaDataNoDiaSemana(new DateTime(ano, mes, 1), diaSemana);
        var data = primeira.AddDays((semanaDoMes - 1) * 7);
        return data.Month == mes ? data : null;
    }

    // Ordinal da semana do mês (1 a 5) para a data, baseado no dia do mês.
    private static int SemanaDoMes(DateTime data) => ((data.Day - 1) / 7) + 1;

    private static HashSet<int> ParseSemanasExcluidas(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new HashSet<int>();

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var v) ? v : 0)
            .Where(v => v >= 1 && v <= 5)
            .ToHashSet();
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

    private static CoberturaVoluntariadoOcorrenciaDto MapToCoberturaDto(EventoOcorrencia ocorrencia, IEnumerable<Escala> escalas)
    {
        var cobertura = new CoberturaVoluntariadoOcorrenciaDto
        {
            OcorrenciaId = ocorrencia.Id,
            EventoId = ocorrencia.EventoId,
            EventoTitulo = ocorrencia.Evento?.Titulo ?? string.Empty,
            DataHoraInicio = ocorrencia.DataHoraInicio,
            StatusOcorrencia = ocorrencia.Status,
        };

        foreach (var escala in escalas)
        {
            var equipe = new CoberturaVoluntariadoEquipeDto
            {
                EquipeId = escala.EquipeId,
                EquipeNome = escala.Equipe?.Nome ?? string.Empty,
                StatusEscala = escala.Status,
                TotalVagas = escala.Itens?.Count ?? 0,
                Confirmados = escala.Itens?.Count(i => i.Status == StatusEscalaItem.Confirmado) ?? 0,
                Pendentes = escala.Itens?.Count(i => i.Status == StatusEscalaItem.Pendente) ?? 0,
                Recusados = escala.Itens?.Count(i => i.Status == StatusEscalaItem.Recusado) ?? 0,
                Substituidos = escala.Itens?.Count(i => i.Status == StatusEscalaItem.Substituido) ?? 0,
                Faltas = escala.Itens?.Count(i => i.Status == StatusEscalaItem.Faltou) ?? 0,
            };

            cobertura.Equipes.Add(equipe);
            cobertura.TotalEscalas++;
            cobertura.TotalVagas += equipe.TotalVagas;
            cobertura.Confirmados += equipe.Confirmados;
            cobertura.Pendentes += equipe.Pendentes;
            cobertura.Recusados += equipe.Recusados;
            cobertura.Substituidos += equipe.Substituidos;
            cobertura.Faltas += equipe.Faltas;
            if (escala.Status == StatusEscala.Rascunho) cobertura.Rascunhos++;
        }

        if (cobertura.TotalEscalas == 0 || cobertura.TotalVagas == 0)
        {
            cobertura.NivelRisco = "none";
            cobertura.RotuloRisco = "Sem escala";
            cobertura.OrdemRisco = 0;
        }
        else if (cobertura.Recusados > 0 || cobertura.Faltas > 0)
        {
            cobertura.NivelRisco = "high";
            cobertura.RotuloRisco = "Risco alto";
            cobertura.OrdemRisco = 1;
        }
        else if (cobertura.Pendentes > 0 || cobertura.Rascunhos > 0)
        {
            cobertura.NivelRisco = "attention";
            cobertura.RotuloRisco = "Atenção";
            cobertura.OrdemRisco = 2;
        }
        else
        {
            cobertura.NivelRisco = "ok";
            cobertura.RotuloRisco = "Coberta";
            cobertura.OrdemRisco = 3;
        }

        return cobertura;
    }
}
