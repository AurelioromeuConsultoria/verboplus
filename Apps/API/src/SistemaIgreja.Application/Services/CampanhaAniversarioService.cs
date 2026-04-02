using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface ICampanhaAniversarioService
{
    Task<CampanhaAniversarioConfiguracaoDto> GetAsync(CampanhaAniversarioHistoricoFiltroDto? filtros = null);
    Task<CampanhaAniversarioConfiguracaoDto> UpdateAsync(AtualizarCampanhaAniversarioDto dto);
    Task<CampanhaAniversarioEnvioTesteResultadoDto> EnviarTesteAsync(EnviarTesteCampanhaAniversarioDto dto, CancellationToken cancellationToken = default);
    Task<CampanhaAniversarioReenvioResultadoDto> ReenviarAsync(int envioId, CancellationToken cancellationToken = default);
    Task<CampanhaAniversarioProcessamentoResultadoDto> ProcessarAniversariantesDoDiaAsync(CancellationToken cancellationToken = default);
}

public class CampanhaAniversarioService : ICampanhaAniversarioService
{
    private readonly IConfiguracaoCampanhaAniversarioRepository _configuracaoRepository;
    private readonly IEnvioCampanhaAniversarioRepository _envioRepository;
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IEvolutionApiService _evolutionApiService;
    private readonly BirthdayCampaignSchedulerSettings _schedulerSettings;
    private readonly PublicAppUrlSettings _publicAppUrlSettings;
    private readonly ILogger<CampanhaAniversarioService> _logger;

    public CampanhaAniversarioService(
        IConfiguracaoCampanhaAniversarioRepository configuracaoRepository,
        IEnvioCampanhaAniversarioRepository envioRepository,
        IPessoaRepository pessoaRepository,
        IEvolutionApiService evolutionApiService,
        IOptions<BirthdayCampaignSchedulerSettings> schedulerSettings,
        IOptions<PublicAppUrlSettings> publicAppUrlSettings,
        ILogger<CampanhaAniversarioService> logger)
    {
        _configuracaoRepository = configuracaoRepository;
        _envioRepository = envioRepository;
        _pessoaRepository = pessoaRepository;
        _evolutionApiService = evolutionApiService;
        _schedulerSettings = schedulerSettings.Value;
        _publicAppUrlSettings = publicAppUrlSettings.Value;
        _logger = logger;
    }

    public async Task<CampanhaAniversarioConfiguracaoDto> GetAsync(CampanhaAniversarioHistoricoFiltroDto? filtros = null)
    {
        var configuracao = await _configuracaoRepository.GetAsync();
        var filtrosNormalizados = NormalizarFiltros(filtros);
        var historico = await _envioRepository.GetHistoricoAsync(
            filtrosNormalizados.Busca,
            filtrosNormalizados.Status,
            filtrosNormalizados.Limit);
        var metricas = await ObterMetricasAsync();
        return MapToDto(configuracao, historico, filtrosNormalizados, metricas);
    }

    public async Task<CampanhaAniversarioConfiguracaoDto> UpdateAsync(AtualizarCampanhaAniversarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MensagemTemplate))
        {
            throw new InvalidOperationException("A mensagem da campanha é obrigatória.");
        }

        var configuracao = new ConfiguracaoCampanhaAniversario
        {
            Ativo = dto.Ativo,
            ImagemUrl = string.IsNullOrWhiteSpace(dto.ImagemUrl) ? null : dto.ImagemUrl.Trim(),
            MensagemTemplate = dto.MensagemTemplate.Trim(),
            HorarioEnvio = ParseHorario(dto.HorarioEnvio)
        };

        var atualizada = await _configuracaoRepository.UpdateAsync(configuracao);
        var historico = await _envioRepository.GetHistoricoAsync(null, null, 50);
        var metricas = await ObterMetricasAsync();
        return MapToDto(atualizada, historico, new CampanhaAniversarioHistoricoFiltroDto(), metricas);
    }

    public async Task<CampanhaAniversarioEnvioTesteResultadoDto> EnviarTesteAsync(
        EnviarTesteCampanhaAniversarioDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.WhatsApp))
        {
            throw new InvalidOperationException("O WhatsApp para teste é obrigatório.");
        }

        var configuracao = await _configuracaoRepository.GetAsync();
        ValidarConfiguracaoParaEnvio(configuracao);

        var nome = string.IsNullOrWhiteSpace(dto.Nome) ? "Teste" : dto.Nome.Trim();
        var response = await _evolutionApiService.EnviarMensagemImagemAsync(
            dto.WhatsApp,
            ResolverImagemUrl(configuracao.ImagemUrl!),
            RenderizarMensagem(configuracao.MensagemTemplate, nome),
            cancellationToken);

        return new CampanhaAniversarioEnvioTesteResultadoDto
        {
            Sucesso = response.Sucesso,
            Mensagem = response.Sucesso
                ? "Mensagem de teste enviada com sucesso."
                : response.MensagemErro ?? "Falha ao enviar mensagem de teste.",
            MessageId = response.MessageId,
            Detalhes = response.Sucesso ? null : response.RespostaCompleta
        };
    }

    public async Task<CampanhaAniversarioReenvioResultadoDto> ReenviarAsync(
        int envioId,
        CancellationToken cancellationToken = default)
    {
        var configuracao = await _configuracaoRepository.GetAsync();
        ValidarConfiguracaoParaEnvio(configuracao);

        var envio = await _envioRepository.GetByIdAsync(envioId)
                    ?? throw new InvalidOperationException("Registro de envio não encontrado.");

        if (envio.Pessoa is null || !envio.Pessoa.Ativo)
        {
            throw new InvalidOperationException("A pessoa associada ao envio não está disponível para reenvio.");
        }

        if (string.IsNullOrWhiteSpace(envio.Pessoa.WhatsApp))
        {
            throw new InvalidOperationException("A pessoa associada ao envio não possui WhatsApp preenchido.");
        }

        var mensagem = RenderizarMensagem(configuracao.MensagemTemplate, envio.Pessoa.Nome);
        var imagemUrl = ResolverImagemUrl(configuracao.ImagemUrl!);

        envio.Status = StatusEnvioCampanhaAniversario.EmProcessamento;
        envio.Tentativas += 1;
        envio.DataUltimaTentativa = DateTime.Now;
        envio.WhatsAppUtilizado = envio.Pessoa.WhatsApp;
        envio.ImagemUrlUtilizada = configuracao.ImagemUrl;
        envio.MensagemUtilizada = mensagem;
        envio.LogErro = null;
        await _envioRepository.UpdateAsync(envio);

        var response = await _evolutionApiService.EnviarMensagemImagemAsync(
            envio.Pessoa.WhatsApp,
            imagemUrl,
            mensagem,
            cancellationToken);

        if (!response.Sucesso)
        {
            envio.Status = StatusEnvioCampanhaAniversario.Erro;
            envio.LogErro = response.MensagemErro ?? "Falha ao reenviar mensagem de aniversário.";
            await _envioRepository.UpdateAsync(envio);

            return new CampanhaAniversarioReenvioResultadoDto
            {
                Sucesso = false,
                EnvioId = envio.Id,
                Mensagem = envio.LogErro,
                MessageId = response.MessageId,
                Detalhes = response.RespostaCompleta
            };
        }

        envio.Status = StatusEnvioCampanhaAniversario.Enviado;
        envio.DataEnvioSucesso = DateTime.Now;
        envio.LogErro = null;
        await _envioRepository.UpdateAsync(envio);

        return new CampanhaAniversarioReenvioResultadoDto
        {
            Sucesso = true,
            EnvioId = envio.Id,
            Mensagem = "Mensagem reenviada com sucesso.",
            MessageId = response.MessageId
        };
    }

    public async Task<CampanhaAniversarioProcessamentoResultadoDto> ProcessarAniversariantesDoDiaAsync(CancellationToken cancellationToken = default)
    {
        var configuracao = await _configuracaoRepository.GetAsync();
        var resultado = new CampanhaAniversarioProcessamentoResultadoDto();

        if (!configuracao.Ativo)
        {
            return resultado;
        }

        if (string.IsNullOrWhiteSpace(configuracao.ImagemUrl) || string.IsNullOrWhiteSpace(configuracao.MensagemTemplate))
        {
            _logger.LogWarning("Campanha de aniversário ativa, mas sem imagem ou mensagem configurada.");
            return resultado;
        }

        var agoraLocal = GetAgoraLocal();
        if (agoraLocal.TimeOfDay < configuracao.HorarioEnvio)
        {
            return resultado;
        }

        var pessoas = await _pessoaRepository.GetAllAsync();
        var aniversariantes = pessoas
            .Where(p => p.Ativo && p.DataNascimento.HasValue && !string.IsNullOrWhiteSpace(p.WhatsApp))
            .Where(p => EhAniversarioHoje(p.DataNascimento!.Value.Date, agoraLocal.Date))
            .OrderBy(p => p.Nome)
            .Take(_schedulerSettings.MaxPessoasPorExecucao)
            .ToList();

        resultado.TotalElegiveis = aniversariantes.Count;

        foreach (var pessoa in aniversariantes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var status = await ProcessarPessoaAsync(pessoa, configuracao, agoraLocal, cancellationToken);
            if (status == StatusProcessamentoCampanha.Enviado)
            {
                resultado.TotalEnviados++;
            }
            else if (status == StatusProcessamentoCampanha.Falhou)
            {
                resultado.TotalFalhas++;
            }
            else
            {
                resultado.TotalIgnorados++;
            }
        }

        return resultado;
    }

    private async Task<StatusProcessamentoCampanha> ProcessarPessoaAsync(
        Pessoa pessoa,
        ConfiguracaoCampanhaAniversario configuracao,
        DateTime agoraLocal,
        CancellationToken cancellationToken)
    {
        var anoReferencia = agoraLocal.Year;
        var envio = await _envioRepository.GetByPessoaAnoAsync(pessoa.Id, anoReferencia);

        if (envio?.Status == StatusEnvioCampanhaAniversario.Enviado)
        {
            return StatusProcessamentoCampanha.Ignorado;
        }

        if (envio?.Tentativas >= _schedulerSettings.MaxTentativasPorPessoa)
        {
            return StatusProcessamentoCampanha.Ignorado;
        }

        var mensagem = RenderizarMensagem(configuracao.MensagemTemplate, pessoa.Nome);
        var imagemUrl = ResolverImagemUrl(configuracao.ImagemUrl!);

        envio ??= new EnvioCampanhaAniversario
        {
            PessoaId = pessoa.Id,
            AnoReferencia = anoReferencia,
            DataAniversario = AjustarAniversarioParaAno(pessoa.DataNascimento!.Value.Date, anoReferencia),
            DataCriacao = DateTime.Now
        };

        envio.Status = StatusEnvioCampanhaAniversario.EmProcessamento;
        envio.Tentativas += 1;
        envio.DataUltimaTentativa = DateTime.Now;
        envio.WhatsAppUtilizado = pessoa.WhatsApp;
        envio.ImagemUrlUtilizada = configuracao.ImagemUrl;
        envio.MensagemUtilizada = mensagem;
        envio.LogErro = null;

        try
        {
            if (envio.Id == 0)
            {
                await _envioRepository.CreateAsync(envio);
            }
            else
            {
                await _envioRepository.UpdateAsync(envio);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Outra instância já registrou o envio da campanha para a pessoa {PessoaId}.", pessoa.Id);
            return StatusProcessamentoCampanha.Ignorado;
        }

        try
        {
            var response = await _evolutionApiService.EnviarMensagemImagemAsync(
                pessoa.WhatsApp!,
                imagemUrl,
                mensagem,
                cancellationToken);

            if (!response.Sucesso)
            {
                envio.Status = StatusEnvioCampanhaAniversario.Erro;
                envio.LogErro = response.MensagemErro ?? "Falha ao enviar mensagem de aniversário.";
                await _envioRepository.UpdateAsync(envio);
                return StatusProcessamentoCampanha.Falhou;
            }

            envio.Status = StatusEnvioCampanhaAniversario.Enviado;
            envio.DataEnvioSucesso = DateTime.Now;
            envio.LogErro = null;
            await _envioRepository.UpdateAsync(envio);
            return StatusProcessamentoCampanha.Enviado;
        }
        catch (Exception ex)
        {
            envio.Status = StatusEnvioCampanhaAniversario.Erro;
            envio.LogErro = ex.Message;
            await _envioRepository.UpdateAsync(envio);
            _logger.LogError(ex, "Erro ao enviar campanha de aniversário para {PessoaId} - {Nome}.", pessoa.Id, pessoa.Nome);
            return StatusProcessamentoCampanha.Falhou;
        }
    }

    private CampanhaAniversarioConfiguracaoDto MapToDto(
        ConfiguracaoCampanhaAniversario configuracao,
        IReadOnlyList<EnvioCampanhaAniversario> recentes,
        CampanhaAniversarioHistoricoFiltroDto filtros,
        CampanhaAniversarioMetricasDto metricas)
    {
        return new CampanhaAniversarioConfiguracaoDto
        {
            Id = configuracao.Id,
            Ativo = configuracao.Ativo,
            ImagemUrl = configuracao.ImagemUrl,
            MensagemTemplate = configuracao.MensagemTemplate,
            HorarioEnvio = configuracao.HorarioEnvio.ToString(@"hh\:mm"),
            DataAtualizacao = configuracao.DataAtualizacao,
            TotalEnviosRecentes = recentes.Count,
            Filtros = filtros,
            Metricas = metricas,
            EnviosRecentes = recentes.Select(MapEnvioToDto).ToList()
        };
    }

    private async Task<CampanhaAniversarioMetricasDto> ObterMetricasAsync()
    {
        var agoraLocal = GetAgoraLocal();
        var anoAtual = agoraLocal.Year;

        return new CampanhaAniversarioMetricasDto
        {
            TotalHistorico = await _envioRepository.CountAsync(),
            TotalEnviadosAnoAtual = await _envioRepository.CountByStatusAnoAsync(StatusEnvioCampanhaAniversario.Enviado, anoAtual),
            TotalFalhasAnoAtual = await _envioRepository.CountByStatusAnoAsync(StatusEnvioCampanhaAniversario.Erro, anoAtual),
            TotalPendentesAnoAtual = await _envioRepository.CountPendentesAnoAsync(anoAtual),
            TotalEnviadosHoje = await _envioRepository.CountByStatusDataAsync(StatusEnvioCampanhaAniversario.Enviado, agoraLocal.Date),
            TotalFalhasHoje = await _envioRepository.CountByStatusDataAsync(StatusEnvioCampanhaAniversario.Erro, agoraLocal.Date)
        };
    }

    private static CampanhaAniversarioHistoricoFiltroDto NormalizarFiltros(CampanhaAniversarioHistoricoFiltroDto? filtros)
    {
        return new CampanhaAniversarioHistoricoFiltroDto
        {
            Busca = string.IsNullOrWhiteSpace(filtros?.Busca) ? null : filtros.Busca.Trim(),
            Status = string.IsNullOrWhiteSpace(filtros?.Status) ? null : filtros.Status.Trim(),
            Limit = filtros?.Limit is > 0 ? Math.Clamp(filtros.Limit, 1, 200) : 50
        };
    }

    private static CampanhaAniversarioEnvioDto MapEnvioToDto(EnvioCampanhaAniversario envio)
    {
        return new CampanhaAniversarioEnvioDto
        {
            Id = envio.Id,
            PessoaId = envio.PessoaId,
            NomePessoa = envio.Pessoa?.Nome ?? string.Empty,
            WhatsApp = envio.WhatsAppUtilizado,
            AnoReferencia = envio.AnoReferencia,
            DataAniversario = envio.DataAniversario,
            Status = envio.Status switch
            {
                StatusEnvioCampanhaAniversario.Pendente => "Pendente",
                StatusEnvioCampanhaAniversario.EmProcessamento => "Em processamento",
                StatusEnvioCampanhaAniversario.Enviado => "Enviado",
                StatusEnvioCampanhaAniversario.Erro => "Erro",
                _ => "Desconhecido"
            },
            Tentativas = envio.Tentativas,
            DataUltimaTentativa = envio.DataUltimaTentativa,
            DataEnvioSucesso = envio.DataEnvioSucesso,
            LogErro = envio.LogErro
        };
    }

    private void ValidarConfiguracaoParaEnvio(ConfiguracaoCampanhaAniversario configuracao)
    {
        if (string.IsNullOrWhiteSpace(configuracao.ImagemUrl))
        {
            throw new InvalidOperationException("Configure a imagem da campanha antes de enviar.");
        }

        if (string.IsNullOrWhiteSpace(configuracao.MensagemTemplate))
        {
            throw new InvalidOperationException("Configure a mensagem da campanha antes de enviar.");
        }
    }

    private string ResolverImagemUrl(string imagemUrl)
    {
        if (Uri.TryCreate(imagemUrl, UriKind.Absolute, out _))
        {
            return imagemUrl;
        }

        if (string.IsNullOrWhiteSpace(_publicAppUrlSettings.ApiBaseUrl))
        {
            throw new InvalidOperationException("PublicAppUrl:ApiBaseUrl não configurado para expor a imagem da campanha.");
        }

        var baseUrl = _publicAppUrlSettings.ApiBaseUrl.TrimEnd('/');
        var caminho = imagemUrl.StartsWith('/') ? imagemUrl : "/" + imagemUrl;
        return $"{baseUrl}{caminho}";
    }

    private DateTime GetAgoraLocal()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_schedulerSettings.TimeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime;
        }
        catch
        {
            return DateTime.Now;
        }
    }

    private static bool EhAniversarioHoje(DateTime dataNascimento, DateTime hoje)
    {
        return AjustarAniversarioParaAno(dataNascimento, hoje.Year).Date == hoje.Date;
    }

    private static DateTime AjustarAniversarioParaAno(DateTime dataNascimento, int ano)
    {
        var dia = Math.Min(dataNascimento.Day, DateTime.DaysInMonth(ano, dataNascimento.Month));
        return new DateTime(ano, dataNascimento.Month, dia);
    }

    private static TimeSpan ParseHorario(string? horario)
    {
        if (TimeSpan.TryParse(horario, out var valor))
        {
            return valor;
        }

        throw new InvalidOperationException("Horário de envio inválido. Use o formato HH:mm.");
    }

    private static string RenderizarMensagem(string template, string nome)
    {
        return template.Replace("{Nome}", nome.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private enum StatusProcessamentoCampanha
    {
        Enviado,
        Ignorado,
        Falhou
    }
}

public static class CampanhaAniversarioDefaults
{
    public const string MensagemTemplatePadrao =
@"{Nome},

Hoje celebramos a sua vida! 🎂

Que neste novo ciclo, Cristo se manifeste de forma ainda mais clara em cada área da sua vida — nos seus caminhos, decisões e sonhos.

Que você experimente um tempo de crescimento, intimidade com Deus e direção em cada passo. Que a graça te sustente, o amor te envolva e o propósito dEle te conduza todos os dias. 🙏✨

Feliz aniversário! Você é parte importante daquilo que Deus está fazendo! 💛

Equipe Kingdom";
}
