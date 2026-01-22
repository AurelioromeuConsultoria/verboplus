using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Application.Interfaces;

namespace SistemaIgreja.Infrastructure.Services;

public class MessageSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Verifica a cada 5 minutos

    public MessageSchedulerService(IServiceProvider serviceProvider, ILogger<MessageSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageSchedulerService iniciado");

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

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("MessageSchedulerService parado");
    }

    private async Task ProcessarMensagensAgendadas()
    {
        using var scope = _serviceProvider.CreateScope();
        var mensagemService = scope.ServiceProvider.GetRequiredService<IMensagemAgendadaService>();

        var mensagensProntas = await mensagemService.GetMensagensProntasParaEnvioAsync();

        foreach (var mensagem in mensagensProntas)
        {
            try
            {
                _logger.LogInformation(
                    "Processando mensagem ID {MensagemId} para visitante {VisitanteNome} ({Telefone})",
                    mensagem.Id,
                    mensagem.NomeVisitante,
                    mensagem.TelefoneVisitante);

                // Marcar como pronta para envio
                await mensagemService.MarcarComoProntaParaEnvioAsync(mensagem.Id);

                // Enviar mensagem via Evolution API
                await EnviarViaEvolutionApi(mensagem);

                // Marcar como enviada
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

        if (mensagensProntas.Any())
        {
            _logger.LogInformation("Processadas {Count} mensagens", mensagensProntas.Count());
        }
    }

    private async Task EnviarViaEvolutionApi(Application.DTOs.MensagemAgendadaDto mensagem)
    {
        using var scope = _serviceProvider.CreateScope();
        var evolutionService = scope.ServiceProvider.GetRequiredService<IEvolutionApiService>();

        // Validar se tem número para enviar
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

        // Enviar mensagem via Evolution API
        var resultado = await evolutionService.EnviarMensagemTextoAsync(
            mensagem.TelefoneVisitante,
            mensagem.TextoFinal);

        if (!resultado.Sucesso)
        {
            var erro = $"Erro ao enviar mensagem via Evolution API: {resultado.MensagemErro} (Status: {resultado.StatusCode})";
            _logger.LogError(
                "Falha ao enviar mensagem ID {MensagemId} - {Erro}",
                mensagem.Id,
                erro);
            
            throw new Exception(erro);
        }

        _logger.LogInformation(
            "Mensagem ID {MensagemId} enviada com sucesso via Evolution API - MessageId: {MessageId}",
            mensagem.Id,
            resultado.MessageId);
    }
}

