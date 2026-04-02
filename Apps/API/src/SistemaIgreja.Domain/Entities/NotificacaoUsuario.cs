using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class NotificacaoUsuario
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    [Required]
    public TipoNotificacaoUsuario Tipo { get; set; } = TipoNotificacaoUsuario.Geral;

    [Required]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Mensagem { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Link { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataLeitura { get; set; }
}

public enum TipoNotificacaoUsuario
{
    Geral = 1,
    Escala = 2,
    TrocaEscala = 3
}
