using System.Net;
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

        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        if (!string.IsNullOrEmpty(_settings.BaseUrl))
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/'));

        if (!string.IsNullOrEmpty(_settings.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);

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
            mensagem.Length > 50 ? mensagem[..50] + "..." : mensagem);

        for (int tentativa = 1; tentativa <= _settings.MaxRetries; tentativa++)
        {
            HttpResponseMessage? response = null;
            string? responseContent = null;

            try
            {
                response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
                responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug(
                    "Resposta Evolution API - Status: {StatusCode}, Tentativa: {Tentativa}/{MaxRetries}, Resposta: {Resposta}",
                    response.StatusCode,
                    tentativa,
                    _settings.MaxRetries,
                    responseContent);

                if (response.IsSuccessStatusCode)
                {
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
                        _logger.LogWarning(ex, "Não foi possível deserializar resposta, mas status é sucesso");
                        return new EvolutionApiResponse
                        {
                            Sucesso = true,
                            StatusCode = (int)response.StatusCode,
                            RespostaCompleta = responseContent
                        };
                    }
                }

                var errorResponse = TratarErroResponse(responseContent, (int)response.StatusCode);

                if (!IsTransientFailure((HttpStatusCode)response.StatusCode))
                {
                    _logger.LogWarning(
                        "Erro permanente (sem retry) ao enviar mensagem - Status: {StatusCode}, Erro: {Erro}",
                        response.StatusCode,
                        errorResponse.MensagemErro);
                    return errorResponse;
                }

                if (tentativa < _settings.MaxRetries)
                {
                    var delay = ObterBackoffExponencial(tentativa);
                    _logger.LogWarning(
                        "Retry {Tentativa}/{MaxRetries} - Status: {StatusCode}, Erro: {Erro}. Nova tentativa em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        response.StatusCode,
                        errorResponse.MensagemErro,
                        delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(
                    "Falha definitiva ao enviar mensagem após {MaxRetries} tentativas - Status: {StatusCode}, Erro: {Erro}",
                    _settings.MaxRetries,
                    response.StatusCode,
                    errorResponse.MensagemErro);
                return errorResponse;
            }
            catch (TaskCanceledException)
            {
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = ObterBackoffExponencial(tentativa);
                    _logger.LogWarning(
                        "Retry {Tentativa}/{MaxRetries} - Timeout na requisição. Nova tentativa em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(
                    "Falha definitiva: timeout ao enviar mensagem após {MaxRetries} tentativas",
                    _settings.MaxRetries);
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
                    var delay = ObterBackoffExponencial(tentativa);
                    _logger.LogWarning(
                        ex,
                        "Retry {Tentativa}/{MaxRetries} - Erro de conexão. Nova tentativa em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(
                    ex,
                    "Falha definitiva: erro de conexão ao enviar mensagem após {MaxRetries} tentativas",
                    _settings.MaxRetries);
                return new EvolutionApiResponse
                {
                    Sucesso = false,
                    MensagemErro = $"Erro de conexão: {ex.Message}",
                    StatusCode = 0
                };
            }
            catch (Exception ex)
            {
                if (tentativa < _settings.MaxRetries)
                {
                    var delay = ObterBackoffExponencial(tentativa);
                    _logger.LogWarning(
                        ex,
                        "Retry {Tentativa}/{MaxRetries} - Erro inesperado. Nova tentativa em {Delay}s",
                        tentativa,
                        _settings.MaxRetries,
                        delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogError(
                    ex,
                    "Falha definitiva: erro inesperado ao enviar mensagem após {MaxRetries} tentativas",
                    _settings.MaxRetries);
                return new EvolutionApiResponse
                {
                    Sucesso = false,
                    MensagemErro = $"Erro inesperado: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

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

            try
            {
                var jsonDoc = JsonDocument.Parse(content);
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var encontrada = jsonDoc.RootElement.EnumerateArray()
                        .Any(inst => inst.TryGetProperty("instance", out var p) && p.GetString() == _settings.InstanceName);
                    if (encontrada)
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

    /// <summary>
    /// Retry apenas para falhas transitórias: timeout, 429, 5xx. Não retry para 4xx (exceto 429).
    /// </summary>
    private static bool IsTransientFailure(HttpStatusCode status)
    {
        var code = (int)status;
        if (code is >= 500 and < 600) return true;
        if (status == (HttpStatusCode)429) return true; // Too Many Requests
        return false;
    }

    /// <summary>
    /// Backoff exponencial: base * 2^(tentativa-1), limitado ao máximo configurável.
    /// </summary>
    private TimeSpan ObterBackoffExponencial(int tentativa)
    {
        var segundos = _settings.RetryDelaySeconds * Math.Pow(2, tentativa - 1);
        var cap = Math.Min(60, Math.Max(1, _settings.RetryDelaySeconds * 8));
        segundos = Math.Min(segundos, cap);
        return TimeSpan.FromSeconds(segundos);
    }

    private static EvolutionApiResponse TratarErroResponse(string responseContent, int statusCode)
    {
        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<EvolutionApiErrorResponse>(responseContent, opts);
            var msg = parsed?.Message ?? parsed?.Error ?? responseContent;
            return new EvolutionApiResponse
            {
                Sucesso = false,
                StatusCode = statusCode,
                MensagemErro = msg,
                RespostaCompleta = responseContent
            };
        }
        catch
        {
            return new EvolutionApiResponse
            {
                Sucesso = false,
                StatusCode = statusCode,
                MensagemErro = responseContent,
                RespostaCompleta = responseContent
            };
        }
    }
}
