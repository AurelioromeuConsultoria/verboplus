using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsOcorrenciaServiceTests
{
    private readonly Mock<IKidsOcorrenciaRepository> _repository = new();
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ICurrentUserContext> _currentUserContext = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();

    private readonly KidsOcorrenciaService _service;

    public KidsOcorrenciaServiceTests()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 50, Ativo = true, TipoUsuario = TipoUsuario.Admin });
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);

        _service = new KidsOcorrenciaService(
            _repository.Object,
            _pessoaRepository.Object,
            _checkinRepository.Object,
            _criancaDetalheRepository.Object,
            _usuarioRepository.Object,
            _currentUserContext.Object,
            _authorizationService.Object);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarOcorrenciaParaCriancaAtiva()
    {
        _pessoaRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Pessoa
            {
                Id = 10,
                Nome = "Ana",
                TipoPessoa = TipoPessoa.Crianca,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _checkinRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn"
            });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 10, SalaId = "Sala 1", DataCadastro = DateTime.UtcNow });
        _repository.Setup(r => r.CreateAsync(It.IsAny<KidsOcorrencia>()))
            .ReturnsAsync((KidsOcorrencia item) =>
            {
                item.Id = 99;
                return item;
            });
        _repository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync(new KidsOcorrencia
            {
                Id = 99,
                CriancaPessoaId = 10,
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                CheckinId = 1,
                Tipo = "FEBRE",
                Titulo = "Febre leve",
                Descricao = "Temperatura acima do normal.",
                Status = "Aberta",
                RequerContatoResponsavel = true,
                SalaId = "Sala 1",
                RegistradoPorPessoaId = 50,
                RegistradoPor = new Pessoa { Id = 50, Nome = "Operador", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow },
                DataCriacao = DateTime.UtcNow,
                VisivelAoResponsavel = false
            });

        var result = await _service.CriarAsync(new CriarKidsOcorrenciaRequest
        {
            CriancaPessoaId = 10,
            CheckinId = 1,
            Tipo = "FEBRE",
            Titulo = "Febre leve",
            Descricao = "Temperatura acima do normal.",
            RequerContatoResponsavel = true,
            VisivelAoResponsavel = false
        });

        result.Id.Should().Be(99);
        result.CriancaPessoaId.Should().Be(10);
        result.Status.Should().Be("Aberta");
        result.SalaId.Should().Be("Sala 1");
    }

    [Fact]
    public async Task AtualizarAsync_DeveEncerrarOcorrenciaQuandoStatusEncerrada()
    {
        _repository.Setup(r => r.GetByIdAsync(7))
            .ReturnsAsync(new KidsOcorrencia
            {
                Id = 7,
                CriancaPessoaId = 10,
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                Tipo = "QUEDA",
                Titulo = "Queda leve",
                Descricao = "Escorregou no corredor.",
                Status = "Aberta",
                RegistradoPorPessoaId = 50,
                RegistradoPor = new Pessoa { Id = 50, Nome = "Operador", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow },
                DataCriacao = DateTime.UtcNow
            });
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KidsOcorrencia>()))
            .ReturnsAsync((KidsOcorrencia item) => item);

        var result = await _service.AtualizarAsync(7, new AtualizarKidsOcorrenciaRequest
        {
            Status = "Encerrada",
            ContatoResponsavelRealizado = true,
            VisivelAoResponsavel = true
        });

        result.Status.Should().Be("Encerrada");
        result.EncerradoEm.Should().NotBeNull();
        result.VisivelAoResponsavel.Should().BeTrue();
        result.ContatoResponsavelRealizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAbertasAsync_DeveRetornarSomenteOcorrenciasNaoEncerradas()
    {
        _repository.Setup(r => r.GetAbertasAsync())
            .ReturnsAsync(new List<KidsOcorrencia>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                    Tipo = "OBSERVACAO_GERAL",
                    Status = "Aberta",
                    DataCriacao = DateTime.UtcNow
                }
            });

        var result = (await _service.GetAbertasAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].CriancaNome.Should().Be("Ana");
        result[0].Status.Should().Be("Aberta");
    }

    [Fact]
    public async Task GetAbertasAsync_DeveBloquearQuandoUsuarioNaoForAdministrativo()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.GetAbertasAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }
}
