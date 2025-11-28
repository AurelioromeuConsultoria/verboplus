namespace SistemaIgreja.Application.DTOs;

public class EventoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ImagemDestaque { get; set; }
    public string? Url { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CriarEventoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ImagemDestaque { get; set; }
    public string? Url { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}

public class AtualizarEventoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ImagemDestaque { get; set; }
    public string? Url { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}



