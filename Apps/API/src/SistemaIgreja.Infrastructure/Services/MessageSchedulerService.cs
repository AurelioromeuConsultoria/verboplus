using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.Infrastructure.Services;

public class MessageSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageSchedulerService> _logger;
    private readonly MessageSchedulerSettings _settings;

    public MessageSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<MessageSchedulerService> logger,
        IOptions<MessageSchedulerSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "MessageSchedulerService iniciado. Intervalo base: {BaseMin} min, jitter: 0–{Jitter}s, batch: {Batch}",
            _settings.BaseIntervalMinutes,
            _settings.JitterSecondsMax,
            _settings.BatchSizeReserva);

        await ValidarEvolutionApiNoBootAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarMensagensAgendadas();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagens agendadas");
            }

            var delay = ObterDelayComJitter();
            _logger.LogDebug("Próxima execução em {Delay}", delay);
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("MessageSchedulerService parado");
    }

    private async Task ValidarEvolutionApiNoBootAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var evolutionService = scope.ServiceProvider.GetRequiredService<IEvolutionApiService>();
            var ok = await evolutionService.ValidarInstanciaAsync(stoppingToken);

            if (ok)
            {
                _logger.LogInformation("Evolution API: validação inicial OK (instância encontrada)");
            }
            else
            {
                _logger.LogWarning("Evolution API: validação inicial falhou (veja logs de RequestUri/404 acima)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Evolution API: falha ao validar no boot");
        }
    }

    /// <summary>
    /// Intervalo base + jitter aleatório (0–JitterSecondsMax) para reduzir sincronismo entre instâncias.
    /// </summary>
    private TimeSpan ObterDelayComJitter()
    {
        var baseInterval = TimeSpan.FromMinutes(_settings.BaseIntervalMinutes);
        var jitterSec = Random.Shared.Next(0, _settings.JitterSecondsMax + 1);
        return baseInterval.Add(TimeSpan.FromSeconds(jitterSec));
    }

    private async Task ProcessarMensagensAgendadas()
    {
        using var scope = _serviceProvider.CreateScope();
        var mensagemService = scope.ServiceProvider.GetRequiredService<IMensagemAgendadaService>();

        var reservadas = await mensagemService.ReservarProntasParaEnvioAsync(_settings.BatchSizeReserva);
        var lista = reservadas.ToList();

        foreach (var mensagem in lista)
        {
            try
            {
                _logger.LogInformation(
                    "Processando mensagem ID {MensagemId} para visitante {VisitanteNome} ({Telefone})",
                    mensagem.Id,
                    mensagem.NomeVisitante,
                    mensagem.TelefoneVisitante);

                await EnviarViaEvolutionApi(mensagem);

                await mensagemService.MarcarComoEnviadaAsync(mensagem.Id);

                _logger.LogInformation(
                    "Mensagem ID {MensagemId} enviada com sucesso para {Telefone}",
                    mensagem.Id,
                    mensagem.TelefoneVisitante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao enviar mensagem ID {MensagemId} para {Telefone}",
                    mensagem.Id,
                    mensagem.TelefoneVisitante);

                await mensagemService.MarcarComoErroAsync(mensagem.Id, ex.Message);
            }
        }

        if (lista.Count > 0)
        {
            _logger.LogInformation("Processadas {Count} mensagens reservadas", lista.Count);
        }
    }

    private async Task EnviarViaEvolutionApi(Application.DTOs.MensagemAgendadaDto mensagem)
    {
        using var scope = _serviceProvider.CreateScope();
        var evolutionService = scope.ServiceProvider.GetRequiredService<IEvolutionApiService>();

        if (string.IsNullOrWhiteSpace(mensagem.TelefoneVisitante))
        {
            throw new InvalidOperationException(
                $"Mensagem ID {mensagem.Id} não possui número de telefone/WhatsApp para envio");
        }

        _logger.LogInformation(
            "Enviando mensagem via Evolution API - ID: {MensagemId}, Número: {Telefone}, Visitante: {Nome}",
            mensagem.Id,
            mensagem.TelefoneVisitante,
            mensagem.NomeVisitante);

        var resultado = await evolutionService.EnviarMensagemTextoAsync(
            mensagem.TelefoneVisitante,
            mensagem.TextoFinal);

        if (!resultado.Sucesso)
        {
            var erro = $"Evolution API: {resultado.MensagemErro} (Status: {resultado.StatusCode})";
            _logger.LogError(
                "Falha ao enviar mensagem ID {MensagemId} - {Erro}",
                mensagem.Id,
                erro);

            throw new Exception(erro);
        }

        _logger.LogInformation(
            "Mensagem ID {MensagemId} enviada via Evolution API - MessageId: {MessageId}",
            mensagem.Id,
            resultado.MessageId);
    }
}
