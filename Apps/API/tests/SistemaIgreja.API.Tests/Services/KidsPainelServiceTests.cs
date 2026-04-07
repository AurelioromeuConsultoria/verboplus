using FluentAssertions;
using Moq;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsPainelServiceTests
{
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();

    private readonly KidsPainelService _service;

    public KidsPainelServiceTests()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);
        _service = new KidsPainelService(
            _checkinRepository.Object,
            _criancaDetalheRepository.Object,
            _authorizationService.Object);
    }

    [Fact]
    public async Task GetPainelOperacionalAsync_DeveMontarResumoComPresentesRetiradasESalas()
    {
        var agora = new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc);

        _checkinRepository.Setup(r => r.GetCheckinsAtivosAsync())
            .ReturnsAsync(new List<KidsCheckin>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    Status = "CheckedIn",
                    CheckinTime = agora.AddMinutes(-40),
                    TokenRetirada = "TOKEN1",
                    TokenRetiradaExpiraEm = agora.AddHours(2),
                    Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                },
                new()
                {
                    Id = 2,
                    CriancaPessoaId = 11,
                    Status = "CheckedIn",
                    CheckinTime = agora.AddMinutes(-25),
                    TokenRetirada = "TOKEN2",
                    TokenRetiradaExpiraEm = agora.AddHours(2),
                    Crianca = new Pessoa { Id = 11, Nome = "Bia", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                }
            });

        _checkinRepository.Setup(r => r.GetByPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<KidsCheckin>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    Status = "CheckedIn",
                    CheckinTime = agora.AddMinutes(-40),
                    Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                },
                new()
                {
                    Id = 3,
                    CriancaPessoaId = 12,
                    Status = "CheckedOut",
                    CheckinTime = agora.AddHours(-2),
                    CheckoutTime = agora.AddHours(-1),
                    Crianca = new Pessoa { Id = 12, Nome = "Clara", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                }
            });

        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe
            {
                PessoaId = 10,
                SalaId = "Sala A",
                Alergias = "Leite",
                DataCadastro = agora
            });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(11))
            .ReturnsAsync(new CriancaDetalhe
            {
                PessoaId = 11,
                SalaId = "Sala B",
                DataCadastro = agora
            });

        var result = await _service.GetPainelOperacionalAsync(agora);

        result.TotalPresentes.Should().Be(2);
        result.TotalPendentesRetirada.Should().Be(2);
        result.TotalRetiradasHoje.Should().Be(1);
        result.TotalAlertasCriticos.Should().Be(1);
        result.Salas.Should().HaveCount(2);
        result.AlertasCriticos.Should().ContainSingle(c => c.CriancaPessoaId == 10);
    }

    [Fact]
    public async Task GetPainelOperacionalAsync_DeveFiltrarPorSalaQuandoInformada()
    {
        var agora = new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc);

        _checkinRepository.Setup(r => r.GetCheckinsAtivosAsync())
            .ReturnsAsync(new List<KidsCheckin>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    Status = "CheckedIn",
                    CheckinTime = agora.AddMinutes(-20),
                    Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                },
                new()
                {
                    Id = 2,
                    CriancaPessoaId = 11,
                    Status = "CheckedIn",
                    CheckinTime = agora.AddMinutes(-10),
                    Crianca = new Pessoa { Id = 11, Nome = "Bia", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = agora }
                }
            });

        _checkinRepository.Setup(r => r.GetByPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<KidsCheckin>());

        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 10, SalaId = "Sala A", DataCadastro = agora });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(11))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 11, SalaId = "Sala B", DataCadastro = agora });

        var result = await _service.GetPainelOperacionalAsync(agora, "Sala B");

        result.TotalPresentes.Should().Be(1);
        result.CriancasPresentes.Should().ContainSingle(c => c.CriancaPessoaId == 11);
        result.Salas.Should().ContainSingle(s => s.SalaId == "Sala B");
    }

    [Fact]
    public async Task GetPainelOperacionalAsync_DeveBloquearQuandoUsuarioNaoForOperador()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.GetPainelOperacionalAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }
}
