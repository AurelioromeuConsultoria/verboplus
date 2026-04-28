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
        var aniversariantes = CalcularAniversariantes(pessoas, 30, 5).ToList();

        return new DashboardDto
        {
            TotalVisitantes = visitantes.Count(),
            MensagensAgendadas = mensagensAgendadas.Count(),
            MensagensEnviadas = mensagensEnviadas.Count(),
            ConfiguracoesAtivas = configuracoesAtivas.Count(),
            TotalPessoas = pessoas.Count(),
            TotalEventos = eventos.Count(),
            TotalInscricoes = inscricoes.Count(),
            TotalVoluntarios = voluntarios.Select(v => v.PessoaId).Distinct().Count(),
            TotalAniversariantesProximos = aniversariantes.Count,
            ProximosAniversariantes = aniversariantes
        };
    }

    private static IEnumerable<AniversarianteDto> CalcularAniversariantes(IEnumerable<Pessoa> pessoas, int dias, int limite)
    {
        if (dias <= 0) dias = 30;
        if (limite <= 0) limite = 5;

        var hoje = DateTime.Today;

        return pessoas
            .Where(p => p.Ativo && p.DataNascimento.HasValue)
            .Select(p =>
            {
                var nasc = p.DataNascimento!.Value.Date;
                var prox = GetProximoAniversario(nasc, hoje);
                var diasRestantes = (prox - hoje).Days;
                return new AniversarianteDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    DataNascimento = nasc,
                    ProximoAniversario = prox,
                    DiasParaAniversario = diasRestantes
                };
            })
            .Where(a => a.DiasParaAniversario <= dias && a.DiasParaAniversario >= 0)
            .OrderBy(a => a.DiasParaAniversario)
            .ThenBy(a => a.Nome)
            .Take(limite);
    }

    private static DateTime GetProximoAniversario(DateTime dataNascimento, DateTime hoje)
    {
        var ano = hoje.Year;
        var mes = dataNascimento.Month;
        var dia = dataNascimento.Day;

        var diasNoMes = DateTime.DaysInMonth(ano, mes);
        if (dia > diasNoMes) dia = diasNoMes;

        var proximo = new DateTime(ano, mes, dia);
        if (proximo < hoje)
        {
            ano += 1;
            diasNoMes = DateTime.DaysInMonth(ano, mes);
            if (dia > diasNoMes) dia = diasNoMes;
            proximo = new DateTime(ano, mes, dia);
        }

        return proximo;
    }
}
