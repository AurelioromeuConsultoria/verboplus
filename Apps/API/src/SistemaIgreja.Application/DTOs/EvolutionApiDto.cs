namespace SistemaIgreja.Application.DTOs;

/// <summary>
/// Request para enviar mensagem de texto via Evolution API
/// </summary>
public class EvolutionApiSendTextRequest
{
    /// <summary>
    /// Número do destinatário no formato internacional (ex: 5511999999999)
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Texto da mensagem
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Delay em milissegundos antes de enviar (opcional)
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Desabilitar preview de links (opcional)
    /// </summary>
    public bool? LinkPreview { get; set; }
}

/// <summary>
/// Response da Evolution API
/// </summary>
public class EvolutionApiResponse
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? MensagemErro { get; set; }

    /// <summary>
    /// Código de status HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// ID da mensagem enviada (se disponível)
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Resposta completa da API (para debug)
    /// </summary>
    public string? RespostaCompleta { get; set; }
}

/// <summary>
/// Resposta da Evolution API quando há erro
/// </summary>
public class EvolutionApiErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
}
