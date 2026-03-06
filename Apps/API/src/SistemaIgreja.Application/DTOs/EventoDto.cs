using SistemaIgreja.Domain.Entities;

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
    public int Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public bool EhRecorrente { get; set; }
    public bool Ativo { get; set; }
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
    public int Tipo { get; set; } = (int)TipoEvento.Evento;
    public bool EhRecorrente { get; set; }
    public bool Ativo { get; set; } = true;
}

public class AtualizarEventoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ImagemDestaque { get; set; }
    public string? Url { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int Tipo { get; set; }
    public bool EhRecorrente { get; set; }
    public bool Ativo { get; set; }
}



