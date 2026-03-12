using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IKidsService
{
    Task<IEnumerable<CriancaDto>> GetCriancasAsync();
    Task<CriancaDto?> GetCriancaByIdAsync(int criancaPessoaId);
    Task<CriancaDto> CreateCriancaAsync(CreateCriancaRequest request);
    Task<CriancaDto> UpdateCriancaAsync(int criancaPessoaId, UpdateCriancaRequest request);
    Task DeleteCriancaAsync(int criancaPessoaId);
    
    Task<ResponsavelCriancaDto> VincularResponsavelAsync(int criancaPessoaId, CreateResponsavelRequest request);
    Task<ResponsavelCriancaDto> UpdateResponsavelAsync(int responsavelId, UpdateResponsavelRequest request);
    Task DesvincularResponsavelAsync(int responsavelId);
    
    Task<CheckinResponse> CheckinAsync(CheckinRequest request);
    Task CheckoutAsync(CheckoutRequest request);
    Task<IEnumerable<KidsCheckinDto>> GetHistoricoCheckinsAsync(int? criancaPessoaId = null);
}

public class KidsService : IKidsService
{
    private readonly IPessoaRepository _pessoaRepository;
    private readonly ICriancaDetalheRepository _criancaDetalheRepository;
    private readonly IResponsavelCriancaRepository _responsavelRepository;
    private readonly IKidsCheckinRepository _checkinRepository;
    private readonly IKidsNotificacaoRepository _notificacaoRepository;
    private readonly IPessoaPerfilRepository _perfilRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKidsPushNotificationService? _pushService;

    public KidsService(
        IPessoaRepository pessoaRepository,
        ICriancaDetalheRepository criancaDetalheRepository,
        IResponsavelCriancaRepository responsavelRepository,
        IKidsCheckinRepository checkinRepository,
        IKidsNotificacaoRepository notificacaoRepository,
        IPessoaPerfilRepository perfilRepository,
        IUnitOfWork unitOfWork,
        IKidsPushNotificationService? pushService = null)
    {
        _pessoaRepository = pessoaRepository;
        _criancaDetalheRepository = criancaDetalheRepository;
        _responsavelRepository = responsavelRepository;
        _checkinRepository = checkinRepository;
        _notificacaoRepository = notificacaoRepository;
        _perfilRepository = perfilRepository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
    }

    public async Task<IEnumerable<CriancaDto>> GetCriancasAsync()
    {
        var pessoas = await _pessoaRepository.GetAllAsync();
        var criancas = pessoas.Where(p => p.TipoPessoa == TipoPessoa.Crianca && p.Ativo).ToList();
        
        var resultado = new List<CriancaDto>();
        
        foreach (var pessoa in criancas)
        {
            var detalhe = await _criancaDetalheRepository.GetByPessoaIdAsync(pessoa.Id);
            var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(pessoa.Id);
            var checkinAtivo = await _checkinRepository.GetCheckinAtivoPorCriancaAsync(pessoa.Id);
            
            resultado.Add(MapToCriancaDto(pessoa, detalhe, responsaveis, checkinAtivo));
        }
        
        return resultado;
    }

    public async Task<CriancaDto?> GetCriancaByIdAsync(int criancaPessoaId)
    {
        var pessoa = await _pessoaRepository.GetByIdAsync(criancaPessoaId);
        if (pessoa == null || pessoa.TipoPessoa != TipoPessoa.Crianca) return null;
        
        var detalhe = await _criancaDetalheRepository.GetByPessoaIdAsync(criancaPessoaId);
        var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(criancaPessoaId);
        var checkinAtivo = await _checkinRepository.GetCheckinAtivoPorCriancaAsync(criancaPessoaId);
        var historico = await _checkinRepository.GetHistoricoPorCriancaAsync(criancaPessoaId, 10);
        
        var dto = MapToCriancaDto(pessoa, detalhe, responsaveis, checkinAtivo);
        return dto;
    }

    public async Task<CriancaDto> CreateCriancaAsync(CreateCriancaRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Criar Pessoa
                var pessoa = new Pessoa
                {
                    Nome = request.Nome,
                    DataNascimento = request.DataNascimento,
                    Email = request.Email,
                    Telefone = request.Telefone,
                    WhatsApp = request.WhatsApp,
                    TipoPessoa = TipoPessoa.Crianca,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };

                // Adicionar Pessoa ao contexto sem salvar
                var pessoaCriada = await _pessoaRepository.CreateWithoutSaveAsync(pessoa);

                // Salvar apenas a Pessoa para gerar o ID (dentro da transação)
                await _unitOfWork.SaveChangesAsync();

                // Recarregar a pessoa para garantir que o ID está disponível
                pessoaCriada = await _pessoaRepository.GetByIdAsync(pessoaCriada.Id) ?? pessoaCriada;

                // Agora que o ID foi gerado e confirmado, criar CriancaDetalhe
                var detalhe = new CriancaDetalhe
                {
                    PessoaId = pessoaCriada.Id,
                    Alergias = request.Alergias,
                    RestricoesAlimentares = request.RestricoesAlimentares,
                    Observacoes = request.Observacoes,
                    SalaId = request.SalaId,
                    DataCadastro = DateTime.UtcNow
                };

                await _criancaDetalheRepository.CreateWithoutSaveAsync(detalhe);

                // Criar perfil Kids
                var perfil = new PessoaPerfil
                {
                    PessoaId = pessoaCriada.Id,
                    Perfil = PerfilPessoa.Kids,
                    DataInicio = DateTime.UtcNow,
                    DataFim = null
                };

                await _perfilRepository.CreateWithoutSaveAsync(perfil);

                // Processar responsáveis se fornecidos
                if (request.Responsaveis != null && request.Responsaveis.Any())
                {
                    foreach (var respRequest in request.Responsaveis)
                    {
                        Pessoa? responsavelPessoa;

                        if (respRequest.ResponsavelPessoaId.HasValue)
                        {
                            responsavelPessoa = await _pessoaRepository.GetByIdAsync(respRequest.ResponsavelPessoaId.Value);
                            if (responsavelPessoa == null)
                                throw new ArgumentException($"Responsável com ID {respRequest.ResponsavelPessoaId} não encontrado");
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(respRequest.Nome))
                                throw new ArgumentException("Nome do responsável é obrigatório quando não fornecido ID");

                            var novaPessoa = new Pessoa
                            {
                                Nome = respRequest.Nome,
                                Telefone = respRequest.Telefone,
                                WhatsApp = respRequest.WhatsApp,
                                Email = respRequest.Email,
                                TipoPessoa = TipoPessoa.Adulto,
                                Ativo = true,
                                DataCriacao = DateTime.UtcNow
                            };

                            responsavelPessoa = await _pessoaRepository.CreateWithoutSaveAsync(novaPessoa);
                            await _unitOfWork.SaveChangesAsync();
                        }

                        var responsavelCrianca = new ResponsavelCrianca
                        {
                            CriancaPessoaId = pessoaCriada.Id,
                            ResponsavelPessoaId = responsavelPessoa!.Id,
                            PodeRetirar = respRequest.PodeRetirar,
                            Parentesco = respRequest.Parentesco,
                            Ativo = true,
                            DataCadastro = DateTime.UtcNow
                        };

                        await _responsavelRepository.CreateWithoutSaveAsync(responsavelCrianca);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Retornar crianca criada
                var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(pessoaCriada.Id);
                KidsCheckin? checkinAtivo = null;
                return MapToCriancaDto(pessoaCriada, detalhe, responsaveis, checkinAtivo);
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao criar criança: {ex.Message}", ex);
        }
    }

    public async Task<CriancaDto> UpdateCriancaAsync(int criancaPessoaId, UpdateCriancaRequest request)
    {
        var pessoa = await _pessoaRepository.GetByIdAsync(criancaPessoaId);
        if (pessoa == null || pessoa.TipoPessoa != TipoPessoa.Crianca)
            throw new ArgumentException("Criança não encontrada");
        
        // Atualizar Pessoa
        pessoa.Nome = request.Nome;
        pessoa.DataNascimento = request.DataNascimento;
        pessoa.Email = request.Email;
        pessoa.Telefone = request.Telefone;
        pessoa.WhatsApp = request.WhatsApp;
        await _pessoaRepository.UpdateAsync(pessoa);
        
        // Atualizar ou criar CriancaDetalhe
        var detalhe = await _criancaDetalheRepository.GetByPessoaIdAsync(criancaPessoaId);
        if (detalhe == null)
        {
            detalhe = new CriancaDetalhe
            {
                PessoaId = criancaPessoaId,
                Alergias = request.Alergias,
                RestricoesAlimentares = request.RestricoesAlimentares,
                Observacoes = request.Observacoes,
                SalaId = request.SalaId,
                DataCadastro = DateTime.UtcNow
            };
            await _criancaDetalheRepository.CreateAsync(detalhe);
        }
        else
        {
            detalhe.Alergias = request.Alergias;
            detalhe.RestricoesAlimentares = request.RestricoesAlimentares;
            detalhe.Observacoes = request.Observacoes;
            detalhe.SalaId = request.SalaId;
            await _criancaDetalheRepository.UpdateAsync(detalhe);
        }
        
        var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(criancaPessoaId);
        var checkinAtivo = await _checkinRepository.GetCheckinAtivoPorCriancaAsync(criancaPessoaId);
        
        return MapToCriancaDto(pessoa, detalhe, responsaveis, checkinAtivo);
    }

    public async Task DeleteCriancaAsync(int criancaPessoaId)
    {
        var pessoa = await _pessoaRepository.GetByIdAsync(criancaPessoaId);
        if (pessoa == null || pessoa.TipoPessoa != TipoPessoa.Crianca)
            throw new ArgumentException("Criança não encontrada");
        
        // Soft delete: desativar pessoa
        pessoa.Ativo = false;
        await _pessoaRepository.UpdateAsync(pessoa);
    }

    public async Task<ResponsavelCriancaDto> VincularResponsavelAsync(int criancaPessoaId, CreateResponsavelRequest request)
    {
        var crianca = await _pessoaRepository.GetByIdAsync(criancaPessoaId);
        if (crianca == null || crianca.TipoPessoa != TipoPessoa.Crianca)
            throw new ArgumentException("Criança não encontrada");
        
        var responsavel = await _pessoaRepository.GetByIdAsync(request.ResponsavelPessoaId);
        if (responsavel == null)
            throw new ArgumentException("Responsável não encontrado");
        
        // Verificar se já existe vínculo
        var vinculoExistente = await _responsavelRepository.GetByCriancaAndResponsavelAsync(criancaPessoaId, request.ResponsavelPessoaId);
        if (vinculoExistente != null)
            throw new InvalidOperationException("Responsável já está vinculado a esta criança");
        
        var responsavelCrianca = new ResponsavelCrianca
        {
            CriancaPessoaId = criancaPessoaId,
            ResponsavelPessoaId = request.ResponsavelPessoaId,
            PodeRetirar = request.PodeRetirar,
            Parentesco = request.Parentesco,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };
        
        var criado = await _responsavelRepository.CreateAsync(responsavelCrianca);
        return MapToResponsavelDto(criado);
    }

    public async Task<ResponsavelCriancaDto> UpdateResponsavelAsync(int responsavelId, UpdateResponsavelRequest request)
    {
        var responsavel = await _responsavelRepository.GetByIdAsync(responsavelId);
        if (responsavel == null)
            throw new ArgumentException("Vínculo de responsável não encontrado");
        
        if (request.PodeRetirar.HasValue)
            responsavel.PodeRetirar = request.PodeRetirar.Value;
        
        if (request.Parentesco != null)
            responsavel.Parentesco = request.Parentesco;
        
        if (request.Ativo.HasValue)
            responsavel.Ativo = request.Ativo.Value;
        
        var atualizado = await _responsavelRepository.UpdateAsync(responsavel);
        return MapToResponsavelDto(atualizado);
    }

    public async Task DesvincularResponsavelAsync(int responsavelId)
    {
        var responsavel = await _responsavelRepository.GetByIdAsync(responsavelId);
        if (responsavel == null)
            throw new ArgumentException("Vínculo de responsável não encontrado");
        
        await _responsavelRepository.DeleteAsync(responsavelId);
    }

    public async Task<CheckinResponse> CheckinAsync(CheckinRequest request)
    {
        var (response, responsavelIds, msg, tipo) = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var crianca = await _pessoaRepository.GetByIdAsync(request.CriancaPessoaId);
            if (crianca == null || crianca.TipoPessoa != TipoPessoa.Crianca || !crianca.Ativo)
                throw new ArgumentException("Criança não encontrada ou inativa");

            var checkinAtivo = await _checkinRepository.GetCheckinAtivoPorCriancaAsync(request.CriancaPessoaId);
            if (checkinAtivo != null)
                throw new InvalidOperationException("Criança já possui um check-in ativo");

            var codigoSessao = Guid.NewGuid().ToString("N")[..12].ToUpper();

            var checkin = new KidsCheckin
            {
                CriancaPessoaId = request.CriancaPessoaId,
                CheckinTime = DateTime.UtcNow,
                CheckinByPessoaId = request.CheckinByPessoaId,
                Metodo = request.Metodo,
                CodigoSessao = codigoSessao,
                Status = "CheckedIn",
                Observacoes = request.Observacoes
            };

            await _checkinRepository.CreateWithoutSaveAsync(checkin);

            var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(request.CriancaPessoaId);
            var notificacoes = new List<NotificacaoCriadaDto>();

            foreach (var responsavel in responsaveis)
            {
                var mensagem = $"Check-in realizado para {crianca.Nome} às {DateTime.UtcNow:HH:mm}";

                var notificacao = new KidsNotificacao
                {
                    CriancaPessoaId = request.CriancaPessoaId,
                    ResponsavelPessoaId = responsavel.ResponsavelPessoaId,
                    Tipo = "CHECKIN",
                    Mensagem = mensagem,
                    Status = "Pendente",
                    DataCriacao = DateTime.UtcNow
                };

                await _notificacaoRepository.CreateWithoutSaveAsync(notificacao);

                notificacoes.Add(new NotificacaoCriadaDto
                {
                    ResponsavelPessoaId = responsavel.ResponsavelPessoaId,
                    ResponsavelNome = responsavel.Responsavel?.Nome ?? "N/A",
                    Status = "Pendente"
                });
            }

            await _unitOfWork.SaveChangesAsync();

            var responsavelIds = notificacoes.Select(n => n.ResponsavelPessoaId).Distinct().ToList();
            var msg = $"Check-in realizado para {crianca.Nome} às {DateTime.UtcNow:HH:mm}";
            return (new CheckinResponse
            {
                CheckinId = checkin.Id,
                CodigoSessao = codigoSessao,
                CheckinTime = checkin.CheckinTime,
                Notificacoes = notificacoes
            }, responsavelIds, msg, "CHECKIN");
        });

        if (_pushService != null && responsavelIds.Count > 0)
            await _pushService.SendToPessoasAsync(responsavelIds, "App Kids - Check-in", msg, new Dictionary<string, string> { ["tipo"] = tipo });

        return response;
    }

    public async Task CheckoutAsync(CheckoutRequest request)
    {
        List<int>? responsavelIdsForPush = null;
        string? msgForPush = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var checkin = await _checkinRepository.GetByCodigoSessaoAsync(request.CodigoSessao);
            if (checkin == null)
                throw new ArgumentException("Código de sessão inválido");

            if (checkin.CriancaPessoaId != request.CriancaPessoaId)
                throw new ArgumentException("Código de sessão não corresponde à criança");

            if (checkin.Status != "CheckedIn")
                throw new InvalidOperationException("Check-in já foi finalizado");

            var podeRetirar = await _responsavelRepository.PodeRetirarAsync(request.CriancaPessoaId, request.CheckoutByPessoaId);
            if (!podeRetirar)
            {
                var pessoa = await _pessoaRepository.GetByIdAsync(request.CheckoutByPessoaId);
                if (pessoa == null || !pessoa.Ativo)
                    throw new UnauthorizedAccessException("Você não tem autorização para retirar esta criança");
            }

            checkin.CheckoutTime = DateTime.UtcNow;
            checkin.CheckoutByPessoaId = request.CheckoutByPessoaId;
            checkin.Status = "CheckedOut";
            if (!string.IsNullOrEmpty(request.Metodo))
                checkin.Metodo = request.Metodo;

            await _checkinRepository.UpdateWithoutSaveAsync(checkin);

            var responsaveis = await _responsavelRepository.GetByCriancaIdAsync(request.CriancaPessoaId);
            var crianca = await _pessoaRepository.GetByIdAsync(request.CriancaPessoaId);
            msgForPush = $"Check-out realizado para {crianca?.Nome ?? "criança"} às {DateTime.UtcNow:HH:mm}";
            responsavelIdsForPush = responsaveis.Select(r => r.ResponsavelPessoaId).Distinct().ToList();

            foreach (var responsavel in responsaveis)
            {
                var notificacao = new KidsNotificacao
                {
                    CriancaPessoaId = request.CriancaPessoaId,
                    ResponsavelPessoaId = responsavel.ResponsavelPessoaId,
                    Tipo = "CHECKOUT",
                    Mensagem = msgForPush,
                    Status = "Pendente",
                    DataCriacao = DateTime.UtcNow
                };

                await _notificacaoRepository.CreateWithoutSaveAsync(notificacao);
            }

            await _unitOfWork.SaveChangesAsync();
        });

        if (_pushService != null && responsavelIdsForPush != null && responsavelIdsForPush.Count > 0 && msgForPush != null)
            await _pushService.SendToPessoasAsync(responsavelIdsForPush, "App Kids - Check-out", msgForPush, new Dictionary<string, string> { ["tipo"] = "CHECKOUT" });
    }

    public async Task<IEnumerable<KidsCheckinDto>> GetHistoricoCheckinsAsync(int? criancaPessoaId = null)
    {
        IEnumerable<KidsCheckin> checkins;
        
        if (criancaPessoaId.HasValue)
        {
            checkins = await _checkinRepository.GetHistoricoPorCriancaAsync(criancaPessoaId.Value);
        }
        else
        {
            checkins = await _checkinRepository.GetCheckinsAtivosAsync();
        }
        
        return checkins.Select(MapToCheckinDto);
    }

    private static CriancaDto MapToCriancaDto(
        Pessoa pessoa,
        CriancaDetalhe? detalhe,
        IEnumerable<ResponsavelCrianca> responsaveis,
        KidsCheckin? checkinAtivo)
    {
        return new CriancaDto
        {
            PessoaId = pessoa.Id,
            Nome = pessoa.Nome,
            DataNascimento = pessoa.DataNascimento,
            Email = pessoa.Email,
            Telefone = pessoa.Telefone,
            WhatsApp = pessoa.WhatsApp,
            Ativo = pessoa.Ativo,
            DataCriacao = pessoa.DataCriacao,
            Alergias = detalhe?.Alergias,
            RestricoesAlimentares = detalhe?.RestricoesAlimentares,
            Observacoes = detalhe?.Observacoes,
            SalaId = detalhe?.SalaId,
            DataCadastro = detalhe?.DataCadastro ?? pessoa.DataCriacao,
            Responsaveis = responsaveis.Select(MapToResponsavelDto).ToList(),
            EstaCheckedIn = checkinAtivo != null,
            CheckinAtual = checkinAtivo != null ? MapToCheckinDto(checkinAtivo) : null
        };
    }

    private static ResponsavelCriancaDto MapToResponsavelDto(ResponsavelCrianca responsavel)
    {
        return new ResponsavelCriancaDto
        {
            Id = responsavel.Id,
            CriancaPessoaId = responsavel.CriancaPessoaId,
            CriancaNome = responsavel.Crianca?.Nome ?? string.Empty,
            ResponsavelPessoaId = responsavel.ResponsavelPessoaId,
            ResponsavelNome = responsavel.Responsavel?.Nome ?? string.Empty,
            ResponsavelTelefone = responsavel.Responsavel?.Telefone,
            ResponsavelWhatsApp = responsavel.Responsavel?.WhatsApp,
            ResponsavelEmail = responsavel.Responsavel?.Email,
            PodeRetirar = responsavel.PodeRetirar,
            Parentesco = responsavel.Parentesco,
            Ativo = responsavel.Ativo,
            DataCadastro = responsavel.DataCadastro
        };
    }

    private static KidsCheckinDto MapToCheckinDto(KidsCheckin checkin)
    {
        return new KidsCheckinDto
        {
            Id = checkin.Id,
            CriancaPessoaId = checkin.CriancaPessoaId,
            CriancaNome = checkin.Crianca?.Nome ?? string.Empty,
            CheckinTime = checkin.CheckinTime,
            CheckoutTime = checkin.CheckoutTime,
            CheckinByPessoaId = checkin.CheckinByPessoaId,
            CheckinByNome = checkin.CheckinBy?.Nome,
            CheckoutByPessoaId = checkin.CheckoutByPessoaId,
            CheckoutByNome = checkin.CheckoutBy?.Nome,
            Metodo = checkin.Metodo,
            CodigoSessao = checkin.CodigoSessao,
            Status = checkin.Status,
            Observacoes = checkin.Observacoes
        };
    }
}


