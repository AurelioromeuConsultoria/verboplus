using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IMensagemAgendadaService
{
    Task<IEnumerable<MensagemAgendadaDto>> GetAllAsync();
    Task<MensagemAgendadaDto?> GetByIdAsync(int id);
    Task<IEnumerable<MensagemAgendadaDto>> GetMensagensProntasParaEnvioAsync();
    Task<IEnumerable<MensagemAgendadaDto>> GetMensagensPorVisitanteAsync(int visitanteId);
    Task AgendarMensagensParaVisitanteAsync(int visitanteId);
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

