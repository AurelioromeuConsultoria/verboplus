using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IVisitanteService
{
    Task<IEnumerable<VisitanteDto>> GetAllAsync();
    Task<VisitanteDto?> GetByIdAsync(int id);
    Task<VisitanteDto> CreateAsync(CriarVisitanteDto dto);
    Task<VisitanteDto> UpdateAsync(int id, AtualizarVisitanteDto dto);
    Task DeleteAsync(int id);
}

public class VisitanteService : IVisitanteService
{
    private readonly IVisitanteRepository _visitanteRepository;
    private readonly IMensagemAgendadaService _mensagemService;

    public VisitanteService(IVisitanteRepository visitanteRepository, IMensagemAgendadaService mensagemService)
    {
        _visitanteRepository = visitanteRepository;
        _mensagemService = mensagemService;
    }

    public async Task<IEnumerable<VisitanteDto>> GetAllAsync()
    {
        var visitantes = await _visitanteRepository.GetAllAsync();
        return visitantes.Select(MapToDto);
    }

    public async Task<VisitanteDto?> GetByIdAsync(int id)
    {
        var visitante = await _visitanteRepository.GetByIdAsync(id);
        return visitante != null ? MapToDto(visitante) : null;
    }

    public async Task<VisitanteDto> CreateAsync(CriarVisitanteDto dto)
    {
        var visitante = new Visitante
        {
            Nome = dto.Nome,
            Telefone = dto.Telefone,
            DataVisita = dto.DataVisita,
            Email = dto.Email,
            Observacoes = dto.Observacoes,
            DataCadastro = DateTime.Now
        };

        var visitanteCriado = await _visitanteRepository.CreateAsync(visitante);
        
        // Agendar mensagens automaticamente
        await _mensagemService.AgendarMensagensParaVisitanteAsync(visitanteCriado.Id);

        return MapToDto(visitanteCriado);
    }

    public async Task<VisitanteDto> UpdateAsync(int id, AtualizarVisitanteDto dto)
    {
        var visitante = await _visitanteRepository.GetByIdAsync(id);
        if (visitante == null)
            throw new ArgumentException("Visitante não encontrado");

        visitante.Nome = dto.Nome;
        visitante.Telefone = dto.Telefone;
        visitante.DataVisita = dto.DataVisita;
        visitante.Email = dto.Email;
        visitante.Observacoes = dto.Observacoes;

        var visitanteAtualizado = await _visitanteRepository.UpdateAsync(visitante);
        return MapToDto(visitanteAtualizado);
    }

    public async Task DeleteAsync(int id)
    {
        await _visitanteRepository.DeleteAsync(id);
    }

    private static VisitanteDto MapToDto(Visitante visitante)
    {
        return new VisitanteDto
        {
            Id = visitante.Id,
            Nome = visitante.Nome,
            Telefone = visitante.Telefone,
            DataVisita = visitante.DataVisita,
            Email = visitante.Email,
            Observacoes = visitante.Observacoes,
            DataCadastro = visitante.DataCadastro
        };
    }
}

