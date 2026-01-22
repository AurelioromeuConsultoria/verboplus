using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Utils;

namespace SistemaIgreja.Application.Services;

public class EvolutionApiService : IEvolutionApiService
{
    private readonly HttpClient _httpClient;
    private readonly EvolutionApiSettings _settings;
    private readonly ILogger<EvolutionApiService> _logger;

    public EvolutionApiService(
        HttpClient httpClient,
        IOptions<EvolutionApiSettings> settings,
        ILogger<EvolutionApiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Configurar base URL
        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/'));
        }

        // Configurar header padrão da API Key
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
        }

        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    }

    public async Task<EvolutionApiResponse> EnviarMensagemTextoAsync(
        string numero, 
        string mensagem, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return new EvolutionApiResponse
            {
                Sucesso = false,
                MensagemErro = "Número de telefone não pode ser vazio",
                StatusCode = 400
            };
        }

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            return new EvolutionApiResponse
            {
                Sucesso = false,
                MensagemErro = "Mensagem não pode ser vazia",
                StatusCode = 400
            };
        }

        // Formatar número para formato internacional
        string numeroFormatado;
        try
        {
            numeroFormatado = TelefoneUtils.FormatarParaEvolutionApi(numero, _settings.CodigoPaisPadrao);
            _logger.LogDebug("Número formatado: {NumeroOriginal} -> {NumeroFormatado}", numero, numeroFormatado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao formatar número {Numero}", numero);
            return new EvolutionApiResponse
            {
                Sucesso = false,
                MensagemErro = $"Erro ao formatar número: {ex.Message}",
                StatusCode = 400
            };
        }

        // Criar request
        var request = new EvolutionApiSendTextRequest
        {
            Number = numeroFormatado,
            Text = mensagem,
            LinkPreview = false
        };

        var endpoint = $"/message/sendText/{_settings.InstanceName}";
        
        _logger.LogInformation(
            "Enviando mensagem via Evolution API - Endpoint: {Endpoint}, Número: {Numero}, Mensagem: {MensagemPreview}",
            endpoint,
            numeroFormatado,
            mensagem.Length > 50 ? mensagem.Substring(0, 50) + "..." : mensagem);

        // Tentar enviar com retry
        for (int tentativa = 1; tentativa <= _settings.MaxRetries; tentativa++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    endpoint,
                    request,
                    cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug(
                    "Resposta Evolution API - Status: {StatusCode}, Tentativa: {Tentativa}/{MaxRetries}, Resposta: {Resposta}",
                    response.StatusCode,
                    tentativa,
                    _settings.MaxRetries,
                    responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Tentar deserializar resposta de sucesso
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        var messageId = jsonDoc.RootElement.TryGetProperty("key", out var keyElement) 
                            && keyElement.TryGetProperty("id", out var idElement)
                            ? idElement.GetString()
                            : null;

                        _logger.LogInformation(
                            "Mensagem enviada com sucesso - Número: {Numero}, MessageId: {MessageId}",
                            numeroFormatado,
                            messageId);

                        return new EvolutionApiResponse
                        {
                            Sucesso = true,
                            StatusCode = (int)response.StatusCode,
                            MessageId = messageId,
                            RespostaCompleta = responseContent
                        };
                    }
                    catch (JsonException ex)
                    {
                        // Mesmo que não consiga deserializar, se o status é sucesso, consideramos OK
                        _logger.LogWarning(ex, "Não foi possível deserializar resposta, mas status é sucesso");
                        return new EvolutionApiResponse
                        {
                            Sucesso = true,
                            StatusCode = (int)response.StatusCode,
                            RespostaCompleta = responseContent
                        };
                    }
                }

                // Tratar erros específicos
                var errorResponse = await TratarErroResponseAsync(response, responseContent, cancellationToken);

                // Erros que não devem ter retry
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning(
                        "Erro permanente ao enviar mensagem - Status: {StatusCode}, Erro: {Erro}",
                        response.StatusCode,
                        errorResponse.MensagemErro);
                    return errorResponse;
                }

                // Erros que podem ter retry (5xx, timeout, etc)
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelaySeconds * tentativa); // Backoff exponencial
                    _logger.LogWarning(
                        "Erro temporário ao enviar mensagem (tentativa {Tentativa}/{MaxRetries}) - Status: {StatusCode}, Erro: {Erro}. Tentando novamente em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        response.StatusCode,
                        errorResponse.MensagemErro,
                        delay.TotalSeconds);
                    
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                // Última tentativa falhou
                _logger.LogError(
                    "Falha ao enviar mensagem após {MaxRetries} tentativas - Status: {StatusCode}, Erro: {Erro}",
                    _settings.MaxRetries,
                    response.StatusCode,
                    errorResponse.MensagemErro);

                return errorResponse;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelaySeconds * tentativa);
                    _logger.LogWarning(
                        "Timeout ao enviar mensagem (tentativa {Tentativa}/{MaxRetries}). Tentando novamente em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        delay.TotalSeconds);
                    
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(ex, "Timeout ao enviar mensagem após {MaxRetries} tentativas", _settings.MaxRetries);
                return new EvolutionApiResponse
                {
                    Sucesso = false,
                    MensagemErro = $"Timeout após {_settings.MaxRetries} tentativas",
                    StatusCode = 408
                };
            }
            catch (HttpRequestException ex)
            {
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelaySeconds * tentativa);
                    _logger.LogWarning(
                        ex,
                        "Erro de conexão ao enviar mensagem (tentativa {Tentativa}/{MaxRetries}). Tentando novamente em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        delay.TotalSeconds);
                    
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(ex, "Erro de conexão ao enviar mensagem após {MaxRetries} tentativas", _settings.MaxRetries);
                return new EvolutionApiResponse
                {
                    Sucesso = false,
                    MensagemErro = $"Erro de conexão: {ex.Message}",
                    StatusCode = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao enviar mensagem (tentativa {Tentativa}/{MaxRetries})", tentativa, _settings.MaxRetries);
                
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelaySeconds * tentativa);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                return new EvolutionApiResponse
                {
                    Sucesso = false,
                    MensagemErro = $"Erro inesperado: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        // Não deveria chegar aqui, mas por segurança
        return new EvolutionApiResponse
        {
            Sucesso = false,
            MensagemErro = "Falha ao enviar mensagem após todas as tentativas",
            StatusCode = 500
        };
    }

    public async Task<bool> ValidarInstanciaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"/instance/fetchInstances";
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao validar instância - Status: {StatusCode}", response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Resposta validação instância: {Resposta}", content);

            // Verificar se a instância está na lista
            try
            {
                var jsonDoc = JsonDocument.Parse(content);
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var instanciaEncontrada = jsonDoc.RootElement.EnumerateArray()
                        .Any(inst => inst.TryGetProperty("instance", out var instanceProp) 
                            && instanceProp.GetString() == _settings.InstanceName);

                    if (instanciaEncontrada)
                    {
                        _logger.LogInformation("Instância {InstanceName} encontrada e válida", _settings.InstanceName);
                        return true;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Não foi possível parsear resposta de validação");
            }

            _logger.LogWarning("Instância {InstanceName} não encontrada na lista", _settings.InstanceName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar instância");
            return false;
        }
    }

    private async Task<EvolutionApiResponse> TratarErroResponseAsync(
        HttpResponseMessage response,
        string responseContent,
        CancellationToken cancellationToken)
    {
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<EvolutionApiErrorResponse>(cancellationToken: cancellationToken);
            
            return new EvolutionApiResponse
            {
                Sucesso = false,
                StatusCode = (int)response.StatusCode,
                MensagemErro = errorResponse?.Message ?? errorResponse?.Error ?? responseContent,
                RespostaCompleta = responseContent
            };
        }
        catch
        {
            return new EvolutionApiResponse
            {
                Sucesso = false,
                StatusCode = (int)response.StatusCode,
                MensagemErro = responseContent,
                RespostaCompleta = responseContent
            };
        }
    }
}
