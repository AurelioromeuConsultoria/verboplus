namespace SistemaIgreja.Application.DTOs;

public class VisitanteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataVisita { get; set; }
    public string? Email { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class CriarVisitanteDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataVisita { get; set; }
    public string? Email { get; set; }
    public string? Observacoes { get; set; }
}

public class AtualizarVisitanteDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataVisita { get; set; }
    public string? Email { get; set; }
    public string? Observacoes { get; set; }
}

