using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Pessoa : ITenantEntity
{
    public int Id { get; set; }

    [Required]
    public int TenantId { get; set; } = Tenant.InitialTenantId;

    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(20)]
    public string? WhatsApp { get; set; }

    public DateTime? DataNascimento { get; set; }

    [Required]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Adulto;

    [Required]
    public bool Ativo { get; set; } = true;

    [Required]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamentos
    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<Visitante> Visitantes { get; set; } = new List<Visitante>();
    public virtual ICollection<Voluntario> Voluntarios { get; set; } = new List<Voluntario>();
    public virtual ICollection<PessoaPerfil> Perfis { get; set; } = new List<PessoaPerfil>();
    
    // Relacionamentos Kids
    public virtual CriancaDetalhe? CriancaDetalhe { get; set; }
    public virtual ICollection<ResponsavelCrianca> ResponsaveisComoCrianca { get; set; } = new List<ResponsavelCrianca>();
    public virtual ICollection<ResponsavelCrianca> ResponsaveisComoResponsavel { get; set; } = new List<ResponsavelCrianca>();
    public virtual ICollection<KidsCheckin> Checkins { get; set; } = new List<KidsCheckin>();
    public virtual ICollection<KidsCheckin> CheckinsRealizadosPor { get; set; } = new List<KidsCheckin>();
    public virtual ICollection<KidsCheckin> CheckoutsRealizadosPor { get; set; } = new List<KidsCheckin>();
    public virtual ICollection<KidsNotificacao> NotificacoesComoCrianca { get; set; } = new List<KidsNotificacao>();
    public virtual ICollection<KidsNotificacao> NotificacoesComoResponsavel { get; set; } = new List<KidsNotificacao>();
    public virtual ICollection<KidsDeviceToken> KidsDeviceTokens { get; set; } = new List<KidsDeviceToken>();
    public virtual ICollection<KidsOcorrencia> KidsOcorrenciasComoCrianca { get; set; } = new List<KidsOcorrencia>();
    public virtual ICollection<KidsOcorrencia> KidsOcorrenciasRegistradas { get; set; } = new List<KidsOcorrencia>();
    public virtual ICollection<KidsOcorrencia> KidsOcorrenciasContatoResponsavel { get; set; } = new List<KidsOcorrencia>();
    public virtual ICollection<KidsOcorrencia> KidsOcorrenciasEncerradas { get; set; } = new List<KidsOcorrencia>();
}
