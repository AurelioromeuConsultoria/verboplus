using System.ComponentModel.DataAnnotations;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class ReceitaDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataRecebimento { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public StatusReceita Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string? ComprovanteUrl { get; set; }
    public int? CategoriaReceitaId { get; set; }
    public string? CategoriaReceitaNome { get; set; }
    public int? ContaBancariaId { get; set; }
    public string? ContaBancariaNome { get; set; }
    public int? CentroCustoId { get; set; }
    public string? CentroCustoNome { get; set; }
    public int? ProjetoId { get; set; }
    public string? ProjetoNome { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CriarReceitaDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Data de recebimento é obrigatória")]
    public DateTime DataRecebimento { get; set; }

    public DateTime? DataConfirmacao { get; set; }

    public StatusReceita Status { get; set; } = StatusReceita.Pendente;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ComprovanteUrl { get; set; }

    public int? CategoriaReceitaId { get; set; }

    public int? ContaBancariaId { get; set; }

    public int? CentroCustoId { get; set; }

    public int? ProjetoId { get; set; }

    public int? UsuarioId { get; set; }
}

public class AtualizarReceitaDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Data de recebimento é obrigatória")]
    public DateTime DataRecebimento { get; set; }

    public DateTime? DataConfirmacao { get; set; }

    public StatusReceita Status { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ComprovanteUrl { get; set; }

    public int? CategoriaReceitaId { get; set; }

    public int? ContaBancariaId { get; set; }

    public int? CentroCustoId { get; set; }

    public int? ProjetoId { get; set; }

    public int? UsuarioId { get; set; }
}
