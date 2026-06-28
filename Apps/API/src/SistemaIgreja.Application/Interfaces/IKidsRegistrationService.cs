using SistemaIgreja.Application.DTOs;

namespace SistemaIgreja.Application.Interfaces;

public interface IKidsRegistrationService
{
    /// <summary>
    /// Registra um novo responsável (portal) de forma self-service.
    /// Cria Pessoa + Usuario vinculados ao tenant identificado pelo slug.
    /// </summary>
    Task<LoginResponseDto> RegistrarResponsavelAsync(RegistrarResponsavelDto dto);
}
