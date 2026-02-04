using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetEstatisticasAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IVisitanteRepository _visitanteRepository;
    private readonly IMensagemAgendadaRepository _mensagemAgendadaRepository;
    private readonly IConfiguracaoMensagemRepository _configuracaoMensagemRepository;
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IInscricaoEventoRepository _inscricaoEventoRepository;
    private readonly IVoluntarioRepository _voluntarioRepository;

    public DashboardService(
        IVisitanteRepository visitanteRepository,
        IMensagemAgendadaRepository mensagemAgendadaRepository,
        IConfiguracaoMensagemRepository configuracaoMensagemRepository,
        IPessoaRepository pessoaRepository,
        IEventoRepository eventoRepository,
        IInscricaoEventoRepository inscricaoEventoRepository,
        IVoluntarioRepository voluntarioRepository)
    {
        _visitanteRepository = visitanteRepository;
        _mensagemAgendadaRepository = mensagemAgendadaRepository;
        _configuracaoMensagemRepository = configuracaoMensagemRepository;
        _pessoaRepository = pessoaRepository;
        _eventoRepository = eventoRepository;
        _inscricaoEventoRepository = inscricaoEventoRepository;
        _voluntarioRepository = voluntarioRepository;
    }

    public async Task<DashboardDto> GetEstatisticasAsync()
    {
        var visitantes = await _visitanteRepository.GetAllAsync();
        var mensagensAgendadas = await _mensagemAgendadaRepository.GetMensagensPorStatusAsync(StatusMensagem.Agendada);
        var mensagensEnviadas = await _mensagemAgendadaRepository.GetMensagensPorStatusAsync(StatusMensagem.Enviada);
        var configuracoesAtivas = await _configuracaoMensagemRepository.GetAtivasAsync();
        var pessoas = await _pessoaRepository.GetAllAsync();
        var eventos = await _eventoRepository.GetAllAsync();
        var inscricoes = await _inscricaoEventoRepository.GetAllAsync();
        var voluntarios = await _voluntarioRepository.GetAllAsync();

        return new DashboardDto
        {
            TotalVisitantes = visitantes.Count(),
            MensagensAgendadas = mensagensAgendadas.Count(),
            MensagensEnviadas = mensagensEnviadas.Count(),
            ConfiguracoesAtivas = configuracoesAtivas.Count(),
            TotalPessoas = pessoas.Count(),
            TotalEventos = eventos.Count(),
            TotalInscricoes = inscricoes.Count(),
            TotalVoluntarios = voluntarios.Count()
        };
    }
}
