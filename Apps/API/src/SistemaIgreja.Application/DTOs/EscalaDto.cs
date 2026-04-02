using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class EscalaDto
{
    public int Id { get; set; }
    public int EventoOcorrenciaId { get; set; }
    public int EquipeId { get; set; }
    public string EquipeNome { get; set; } = string.Empty;
    public DateTime EventoDataHoraInicio { get; set; }
    public string EventoTitulo { get; set; } = string.Empty;
    public StatusEscala Status { get; set; }
    public string? Observacoes { get; set; }
    public int? CriadoPorUsuarioId { get; set; }
    public string? CriadoPorUsuarioNome { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataPublicacao { get; set; }
    public List<EscalaItemDto> Itens { get; set; } = new();
}

public class EscalaItemDto
{
    public int Id { get; set; }
    public int EscalaId { get; set; }
    public int EquipeId { get; set; }
    public string EquipeNome { get; set; } = string.Empty;
    public int? CargoId { get; set; }
    public string? CargoNome { get; set; }
    public int VoluntarioId { get; set; }
    public int VoluntarioPessoaId { get; set; }
    public string VoluntarioNome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool ConflitoAprovado { get; set; }
    public string? MotivoExcecao { get; set; }
    public int? AprovadoPorUsuarioId { get; set; }
    public string? AprovadoPorUsuarioNome { get; set; }
    public DateTime? AprovadoEm { get; set; }
    public StatusEscalaItem Status { get; set; }
    public DateTime? DataConvite { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public DateTime? DataRecusa { get; set; }
    public DateTime? DataLembrete7DiasEnviado { get; set; }
    public DateTime? DataLembrete24HorasEnviado { get; set; }
    public string? MotivoRecusa { get; set; }
    public int? RespondidoPorUsuarioId { get; set; }
    public string? RespondidoPorUsuarioNome { get; set; }
    public string? ObservacaoOperacional { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CriarEscalaDto
{
    public int EventoOcorrenciaId { get; set; }
    public int EquipeId { get; set; }
    public string? Observacoes { get; set; }
}

public class AtualizarEscalaDto
{
    public StatusEscala Status { get; set; }
    public string? Observacoes { get; set; }
}

public class CriarEscalaItemDto
{
    public int EquipeId { get; set; }
    public int? CargoId { get; set; }
    public int VoluntarioId { get; set; }
    public int Ordem { get; set; } = 0;
    public bool ForcarConflito { get; set; } = false;
    public string? MotivoExcecao { get; set; }
}

public class AtualizarEscalaItemDto
{
    public int EquipeId { get; set; }
    public int? CargoId { get; set; }
    public int VoluntarioId { get; set; }
    public int Ordem { get; set; } = 0;
    public bool ForcarConflito { get; set; } = false;
    public string? MotivoExcecao { get; set; }
}

public class RecusarEscalaItemDto
{
    public string? MotivoRecusa { get; set; }
}

public class RegistrarPresencaEscalaItemDto
{
    public bool Compareceu { get; set; }
    public string? ObservacaoOperacional { get; set; }
}

public class HistoricoVoluntarioDto
{
    public int PessoaId { get; set; }
    public string VoluntarioNome { get; set; } = string.Empty;
    public List<string> Equipes { get; set; } = new();
    public int TotalEscalas { get; set; }
    public int Confirmados { get; set; }
    public int Recusados { get; set; }
    public int Substituidos { get; set; }
    public int Presencas { get; set; }
    public int Faltas { get; set; }
    public int Pendentes { get; set; }
    public int CargaMesAtual { get; set; }
    public DateTime? UltimaEscalaEm { get; set; }
    public DateTime? ProximaEscalaEm { get; set; }
}

public class SugestaoEscalaVoluntarioDto
{
    public int VoluntarioId { get; set; }
    public int PessoaId { get; set; }
    public string VoluntarioNome { get; set; } = string.Empty;
    public int EquipeId { get; set; }
    public string EquipeNome { get; set; } = string.Empty;
    public int CargoId { get; set; }
    public string CargoNome { get; set; } = string.Empty;
    public bool Disponivel { get; set; }
    public int CargaRecente { get; set; }
    public string? MotivoBloqueio { get; set; }
}
