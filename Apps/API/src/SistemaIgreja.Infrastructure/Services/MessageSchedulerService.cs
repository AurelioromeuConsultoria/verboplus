using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaIgreja.Application.Services;

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

                // Marcar como pronta para envio (simulação do envio)
                await mensagemService.MarcarComoProntaParaEnvioAsync(mensagem.Id);

                // Simular envio da mensagem (aqui seria integrado com API do WhatsApp)
                await SimularEnvioWhatsApp(mensagem);

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

    private async Task SimularEnvioWhatsApp(Application.DTOs.MensagemAgendadaDto mensagem)
    {
        // Simular delay de envio
        await Task.Delay(1000);

        // Log da mensagem que seria enviada
        _logger.LogInformation(
            "SIMULAÇÃO ENVIO WhatsApp - Para: {Telefone}, Mensagem: {Texto}",
            mensagem.TelefoneVisitante,
            mensagem.TextoFinal);

        // Aqui seria implementada a integração real com API do WhatsApp
        // Exemplo com Z-API, Twilio, etc.
        /*
        var client = new HttpClient();
        var payload = new
        {
            phone = mensagem.TelefoneVisitante,
            message = mensagem.TextoFinal
        };
        
        var response = await client.PostAsJsonAsync("https://api.z-api.io/instances/YOUR_INSTANCE/token/YOUR_TOKEN/send-text", payload);
        response.EnsureSuccessStatusCode();
        */
    }
}

