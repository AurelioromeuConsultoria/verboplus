using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.MensagensAgendadas;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IMensagemAgendadaService
{
    Task<IEnumerable<MensagemAgendadaDto>> GetAllAsync();
    Task<PagedResultDto<MensagemAgendadaDto>> GetPagedAsync(MensagemAgendadaPagedQueryDto query);
    Task<MensagemAgendadaStatsDto> GetStatsAsync();
    Task<MensagemAgendadaDto?> GetByIdAsync(int id);
    Task<IEnumerable<MensagemAgendadaDto>> GetMensagensProntasParaEnvioAsync();
    /// <summary>Reserva transacionalmente mensagens prontas (status → EmProcessamento). Apenas as reservadas devem ser processadas.</summary>
    Task<IEnumerable<MensagemAgendadaDto>> ReservarProntasParaEnvioAsync(int limit);
    Task<IEnumerable<MensagemAgendadaDto>> GetMensagensPorVisitanteAsync(int visitanteId);
    Task AgendarMensagensParaVisitanteAsync(int visitanteId);
    Task<RegerarMensagensResultDto> RegerarMensagensParaVisitanteAsync(int visitanteId);
    Task MarcarComoProntaParaEnvioAsync(int mensagemId);
    Task MarcarComoEnviadaAsync(int mensagemId);
    Task MarcarComoErroAsync(int mensagemId, string erro);
}

public class MensagemAgendadaService : IMensagemAgendadaService
{
    private readonly IMensagemAgendadaRepository _mensagemRepository;
    private readonly IVisitanteRepository _visitanteRepository;
    private readonly IConfiguracaoMensagemRepository _configuracaoRepository;

    public MensagemAgendadaService(
        IMensagemAgendadaRepository mensagemRepository,
        IVisitanteRepository visitanteRepository,
        IConfiguracaoMensagemRepository configuracaoRepository)
    {
        _mensagemRepository = mensagemRepository;
        _visitanteRepository = visitanteRepository;
        _configuracaoRepository = configuracaoRepository;
    }

    public async Task<IEnumerable<MensagemAgendadaDto>> GetAllAsync()
    {
        var mensagens = await _mensagemRepository.GetAllAsync();
        return mensagens.Select(MapToDto);
    }

    public async Task<PagedResultDto<MensagemAgendadaDto>> GetPagedAsync(MensagemAgendadaPagedQueryDto queryDto)
    {
        var page = queryDto.Page <= 0 ? 1 : queryDto.Page;
        var pageSize = queryDto.PageSize <= 0 ? 20 : Math.Min(queryDto.PageSize, 200);

        StatusMensagem? status = null;
        if (queryDto.Status.HasValue && Enum.IsDefined(typeof(StatusMensagem), queryDto.Status.Value))
        {
            status = (StatusMensagem)queryDto.Status.Value;
        }

        var query = new MensagemAgendadaPagedQuery
        {
            Page = page,
            PageSize = pageSize,
            Sort = queryDto.Sort,
            Direction = queryDto.Direction,
            Texto = queryDto.Texto,
            VisitanteId = queryDto.VisitanteId,
            Status = status,
            DataEnvioFrom = queryDto.DataEnvioFrom,
            DataEnvioTo = queryDto.DataEnvioTo
        };

        var (items, total) = await _mensagemRepository.GetPagedAsync(query);
        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<MensagemAgendadaDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<MensagemAgendadaStatsDto> GetStatsAsync()
    {
        return _mensagemRepository.GetStatsAsync();
    }

    public async Task<MensagemAgendadaDto?> GetByIdAsync(int id)
    {
        var mensagem = await _mensagemRepository.GetByIdAsync(id);
        return mensagem != null ? MapToDto(mensagem) : null;
    }

    public async Task<IEnumerable<MensagemAgendadaDto>> GetMensagensProntasParaEnvioAsync()
    {
        var mensagens = await _mensagemRepository.GetMensagensProntasParaEnvioAsync();
        return mensagens.Select(MapToDto);
    }

    public async Task<IEnumerable<MensagemAgendadaDto>> ReservarProntasParaEnvioAsync(int limit)
    {
        var mensagens = await _mensagemRepository.ReservarProntasParaEnvioAsync(limit);
        return mensagens.Select(MapToDto);
    }

    public async Task<IEnumerable<MensagemAgendadaDto>> GetMensagensPorVisitanteAsync(int visitanteId)
    {
        var mensagens = await _mensagemRepository.GetMensagensPorVisitanteAsync(visitanteId);
        return mensagens.Select(MapToDto);
    }

    public async Task AgendarMensagensParaVisitanteAsync(int visitanteId)
    {
        var visitante = await _visitanteRepository.GetByIdAsync(visitanteId);
        if (visitante == null)
            throw new ArgumentException("Visitante não encontrado");

        var configuracoes = await _configuracaoRepository.GetAtivasAsync();

        foreach (var configuracao in configuracoes)
        {
            var dataEnvio = visitante.DataVisita.AddDays(configuracao.DiasAposVisita);
            var dataEnvioCompleta = dataEnvio.Date + configuracao.HorarioEnvio;

            var textoFinal = configuracao.TextoMensagem.Replace("{Nome}", visitante.Pessoa?.Nome ?? "");

            var mensagemAgendada = new MensagemAgendada
            {
                VisitanteId = visitante.Id,
                ConfiguracaoMensagemId = configuracao.Id,
                DataAgendamento = DateTime.Now,
                DataEnvio = dataEnvioCompleta,
                Status = StatusMensagem.Agendada,
                TextoFinal = textoFinal,
                DataCriacao = DateTime.Now
            };

            await _mensagemRepository.CreateAsync(mensagemAgendada);
        }
    }

    public async Task<RegerarMensagensResultDto> RegerarMensagensParaVisitanteAsync(int visitanteId)
    {
        var visitante = await _visitanteRepository.GetByIdAsync(visitanteId);
        if (visitante == null)
            throw new ArgumentException("Visitante não encontrado");

        var canceladas = await _mensagemRepository.CancelarPendentesPorVisitanteAsync(
            visitanteId,
            $"Cancelada por regeneração em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        var configuracoes = await _configuracaoRepository.GetAtivasAsync();
        var criadas = 0;

        foreach (var configuracao in configuracoes)
        {
            var dataEnvio = visitante.DataVisita.AddDays(configuracao.DiasAposVisita);
            var dataEnvioCompleta = dataEnvio.Date + configuracao.HorarioEnvio;

            var textoFinal = configuracao.TextoMensagem.Replace("{Nome}", visitante.Pessoa?.Nome ?? "");

            var mensagemAgendada = new MensagemAgendada
            {
                VisitanteId = visitante.Id,
                ConfiguracaoMensagemId = configuracao.Id,
                DataAgendamento = DateTime.Now,
                DataEnvio = dataEnvioCompleta,
                Status = StatusMensagem.Agendada,
                TextoFinal = textoFinal,
                DataCriacao = DateTime.Now
            };

            await _mensagemRepository.CreateAsync(mensagemAgendada);
            criadas++;
        }

        return new RegerarMensagensResultDto
        {
            MensagensCanceladas = canceladas,
            MensagensCriadas = criadas
        };
    }

    public async Task MarcarComoProntaParaEnvioAsync(int mensagemId)
    {
        var mensagem = await _mensagemRepository.GetByIdAsync(mensagemId);
        if (mensagem == null)
            throw new ArgumentException("Mensagem não encontrada");

        mensagem.Status = StatusMensagem.ProntaParaEnvio;
        mensagem.DataProcessamento = DateTime.Now;

        await _mensagemRepository.UpdateAsync(mensagem);
    }

    public async Task MarcarComoEnviadaAsync(int mensagemId)
    {
        var mensagem = await _mensagemRepository.GetByIdAsync(mensagemId);
        if (mensagem == null)
            throw new ArgumentException("Mensagem não encontrada");

        mensagem.Status = StatusMensagem.Enviada;
        mensagem.DataProcessamento = DateTime.Now;

        await _mensagemRepository.UpdateAsync(mensagem);
    }

    public async Task MarcarComoErroAsync(int mensagemId, string erro)
    {
        var mensagem = await _mensagemRepository.GetByIdAsync(mensagemId);
        if (mensagem == null)
            throw new ArgumentException("Mensagem não encontrada");

        mensagem.Status = StatusMensagem.Erro;
        mensagem.LogErro = erro;
        mensagem.DataProcessamento = DateTime.Now;

        await _mensagemRepository.UpdateAsync(mensagem);
    }

    private static MensagemAgendadaDto MapToDto(MensagemAgendada mensagem)
    {
        // Priorizar WhatsApp, usar Telefone como fallback
        var telefoneOuWhatsApp = mensagem.Visitante?.Pessoa?.WhatsApp 
            ?? mensagem.Visitante?.Pessoa?.Telefone 
            ?? "";

        return new MensagemAgendadaDto
        {
            Id = mensagem.Id,
            VisitanteId = mensagem.VisitanteId,
            NomeVisitante = mensagem.Visitante?.Pessoa?.Nome ?? "",
            TelefoneVisitante = telefoneOuWhatsApp,
            ConfiguracaoMensagemId = mensagem.ConfiguracaoMensagemId,
            NomeConfiguracao = mensagem.ConfiguracaoMensagem?.Nome ?? "",
            DataAgendamento = mensagem.DataAgendamento,
            DataEnvio = mensagem.DataEnvio,
            Status = mensagem.Status,
            TextoFinal = mensagem.TextoFinal,
            DataProcessamento = mensagem.DataProcessamento,
            LogErro = mensagem.LogErro,
            DataCriacao = mensagem.DataCriacao
        };
    }
}

