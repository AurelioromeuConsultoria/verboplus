using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class KidsControllerTests
{
    private readonly Mock<IKidsService> _kidsServiceMock = new();
    private readonly Mock<IKidsPreCheckinService> _preCheckinServiceMock = new();
    private readonly Mock<IKidsConteudoAulaService> _conteudoAulaServiceMock = new();
    private readonly Mock<IKidsNotificacaoService> _notificacaoServiceMock = new();
    private readonly Mock<IKidsRetiradaService> _retiradaServiceMock = new();
    private readonly Mock<IKidsPainelService> _painelServiceMock = new();
    private readonly Mock<IKidsOcorrenciaService> _ocorrenciaServiceMock = new();
    private readonly Mock<IKidsEstruturaService> _estruturaServiceMock = new();
    private readonly Mock<IKidsIndicadoresService> _indicadoresServiceMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IKidsDeviceTokenRepository> _deviceTokenRepositoryMock = new();

    private KidsController CreateController() =>
        new(
            _kidsServiceMock.Object,
            _preCheckinServiceMock.Object,
            _conteudoAulaServiceMock.Object,
            _notificacaoServiceMock.Object,
            _retiradaServiceMock.Object,
            _painelServiceMock.Object,
            _ocorrenciaServiceMock.Object,
            _estruturaServiceMock.Object,
            _indicadoresServiceMock.Object,
            _usuarioRepositoryMock.Object,
            _deviceTokenRepositoryMock.Object);

    [Fact]
    public async Task GetIndicadores_ReturnsOk()
    {
        _indicadoresServiceMock.Setup(s => s.GetIndicadoresAsync(30)).ReturnsAsync(new KidsIndicadoresDto());
        var controller = CreateController();

        var result = await controller.GetIndicadores(30);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetSalas_ReturnsForbidden_WhenUnauthorized()
    {
        _estruturaServiceMock.Setup(s => s.GetSalasAsync(false)).ThrowsAsync(new UnauthorizedAccessException("sem acesso"));
        var controller = CreateController();

        var result = await controller.GetSalas(false);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CreateSala_ReturnsBadRequest_OnArgumentException()
    {
        var request = new CreateKidsSalaRequest { Id = "S1", Nome = "Sala 1" };
        _estruturaServiceMock.Setup(s => s.CreateSalaAsync(request)).ThrowsAsync(new ArgumentException("inválida"));
        var controller = CreateController();

        var result = await controller.CreateSala(request);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateSala_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateKidsSalaRequest { Nome = "Sala Atualizada", Ativo = true };
        _estruturaServiceMock.Setup(s => s.UpdateSalaAsync("S1", request)).ReturnsAsync(new KidsSalaDto { Id = "S1", Nome = "Sala Atualizada" });
        var controller = CreateController();

        var result = await controller.UpdateSala("S1", request);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetTurmas_ReturnsOk()
    {
        _estruturaServiceMock.Setup(s => s.GetTurmasAsync("S1", false)).ReturnsAsync(new List<KidsTurmaDto>());
        var controller = CreateController();

        var result = await controller.GetTurmas("S1", false);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task CreateTurma_ReturnsBadRequest_OnArgumentException()
    {
        var request = new CreateKidsTurmaRequest { Id = "T1", SalaId = "S1", Nome = "Turma 1" };
        _estruturaServiceMock.Setup(s => s.CreateTurmaAsync(request)).ThrowsAsync(new ArgumentException("inválida"));
        var controller = CreateController();

        var result = await controller.CreateTurma(request);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateTurma_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateKidsTurmaRequest { SalaId = "S1", Nome = "Turma Atualizada", Ativo = true };
        _estruturaServiceMock.Setup(s => s.UpdateTurmaAsync("T1", request)).ReturnsAsync(new KidsTurmaDto { Id = "T1", SalaId = "S1", Nome = "Turma Atualizada" });
        var controller = CreateController();

        var result = await controller.UpdateTurma("T1", request);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetCriancas_ReturnsOk()
    {
        _kidsServiceMock.Setup(s => s.GetCriancasAsync()).ReturnsAsync(new List<CriancaDto>());
        var controller = CreateController();

        var result = await controller.GetCriancas();

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetMinhasCriancas_ReturnsForbidden_WhenUnauthorized()
    {
        _kidsServiceMock.Setup(s => s.GetMinhasCriancasAsync()).ThrowsAsync(new UnauthorizedAccessException("sem acesso"));
        var controller = CreateController();

        var result = await controller.GetMinhasCriancas();

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetMinhaCriancaById_ReturnsNotFound_WhenMissing()
    {
        _kidsServiceMock.Setup(s => s.GetMinhaCriancaByIdAsync(10)).ReturnsAsync((MinhaCriancaDetalheDto?)null);
        var controller = CreateController();

        var result = await controller.GetMinhaCriancaById(10);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMeusCheckinsAndAvisos_ReturnOk()
    {
        _kidsServiceMock.Setup(s => s.GetMeusCheckinsAsync()).ReturnsAsync(new List<MeuCheckinResumoDto>());
        _notificacaoServiceMock.Setup(s => s.GetMeusAvisosAsync(true, "ALERTA", 2, 5)).ReturnsAsync(new List<MeuAvisoKidsDto>());
        var controller = CreateController();

        (await controller.GetMeusCheckins()).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetMeusAvisos(true, "ALERTA", 2, 5)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task ConteudoAulaEndpoints_ReturnExpectedResponses()
    {
        _conteudoAulaServiceMock.Setup(s => s.GetAsync("Published", "S1", "T1", null, 10))
            .ReturnsAsync(new List<KidsConteudoAulaAdminDto>());
        _conteudoAulaServiceMock.Setup(s => s.GetByIdAsync(8))
            .ReturnsAsync(new KidsConteudoAulaAdminDto { Id = 8, Titulo = "Aula 8" });
        _conteudoAulaServiceMock.Setup(s => s.GetMeuConteudoPorCriancaAsync(10, 5))
            .ReturnsAsync(new List<MeuConteudoAulaDto>());
        _conteudoAulaServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateKidsConteudoAulaRequest>()))
            .ReturnsAsync(new KidsConteudoAulaAdminDto { Id = 12, Titulo = "Nova aula", Status = "Draft" });
        _conteudoAulaServiceMock.Setup(s => s.UpdateAsync(12, It.IsAny<UpdateKidsConteudoAulaRequest>()))
            .ReturnsAsync(new KidsConteudoAulaAdminDto { Id = 12, Titulo = "Aula atualizada", Status = "Draft" });
        _conteudoAulaServiceMock.Setup(s => s.PublicarAsync(12))
            .ReturnsAsync(new KidsConteudoAulaAdminDto { Id = 12, Titulo = "Aula atualizada", Status = "Published" });
        _conteudoAulaServiceMock.Setup(s => s.ArquivarAsync(12))
            .ReturnsAsync(new KidsConteudoAulaAdminDto { Id = 12, Titulo = "Aula atualizada", Status = "Archived" });

        var controller = CreateController();

        (await controller.GetConteudosAula("Published", "S1", "T1", null, 10)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetConteudoAulaById(8)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetMeuConteudoPorCrianca(10, 5)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.CreateConteudoAula(new CreateKidsConteudoAulaRequest
        {
            Titulo = "Nova aula",
            Resumo = "Resumo",
            DataReferencia = new DateTime(2026, 5, 6)
        })).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.UpdateConteudoAula(12, new UpdateKidsConteudoAulaRequest
        {
            Titulo = "Aula atualizada",
            Resumo = "Resumo",
            DataReferencia = new DateTime(2026, 5, 6)
        })).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.PublicarConteudoAula(12)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.ArquivarConteudoAula(12)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task MeusPreCheckinsEndpoints_ReturnOk()
    {
        _preCheckinServiceMock.Setup(s => s.CriarMeuPreCheckinAsync(It.IsAny<CreateKidsPreCheckinRequest>()))
            .ReturnsAsync(new KidsPreCheckinDto { Id = 1, CriancaPessoaId = 10, ResponsavelPessoaId = 30, Status = "Pending", QrToken = "TOKEN", CodigoCurto = "ABCD1234" });
        _preCheckinServiceMock.Setup(s => s.GetMeusPreCheckinsAsync("Pending", true))
            .ReturnsAsync(new List<KidsPreCheckinDto>());
        _preCheckinServiceMock.Setup(s => s.CancelarMeuPreCheckinAsync(1, It.IsAny<CancelKidsPreCheckinRequest>()))
            .ReturnsAsync(new KidsPreCheckinDto { Id = 1, CriancaPessoaId = 10, ResponsavelPessoaId = 30, Status = "Cancelled", QrToken = "TOKEN", CodigoCurto = "ABCD1234" });
        _preCheckinServiceMock.Setup(s => s.GetPendentesAsync(5, "S1", "T1"))
            .ReturnsAsync(new List<KidsPreCheckinDto>());
        var controller = CreateController();

        (await controller.CreateMeuPreCheckin(new CreateKidsPreCheckinRequest { CriancaPessoaId = 10 })).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetMeusPreCheckins("Pending", true)).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.CancelarMeuPreCheckin(1, new CancelKidsPreCheckinRequest { Motivo = "Mudança de planos" })).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetPreCheckinsPendentes(5, "S1", "T1")).Result
            .Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task MarcarMeuAvisoComoLido_ReturnsOk()
    {
        _notificacaoServiceMock.Setup(s => s.MarcarComoLidoAsync(3)).ReturnsAsync(new MeuAvisoKidsDto { Id = 3 });
        var controller = CreateController();

        var result = await controller.MarcarMeuAvisoComoLido(3);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetCriancaById_ReturnsNotFound_WhenMissing()
    {
        _kidsServiceMock.Setup(s => s.GetCriancaByIdAsync(99)).ReturnsAsync((CriancaDto?)null);
        var controller = CreateController();

        var result = await controller.GetCriancaById(99);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateUpdateDeleteCrianca_ReturnExpectedResponses()
    {
        var createRequest = new CreateCriancaRequest { Nome = "Lia", DataNascimento = new DateTime(2020, 1, 1) };
        _kidsServiceMock.Setup(s => s.CreateCriancaAsync(createRequest)).ReturnsAsync(new CriancaDto { PessoaId = 10, Nome = "Lia" });

        var updateRequest = new UpdateCriancaRequest { Nome = "Lia 2", DataNascimento = new DateTime(2020, 1, 1) };
        _kidsServiceMock.Setup(s => s.UpdateCriancaAsync(10, updateRequest)).ReturnsAsync(new CriancaDto { PessoaId = 10, Nome = "Lia 2" });
        _kidsServiceMock.Setup(s => s.DeleteCriancaAsync(10)).Returns(Task.CompletedTask);

        var controller = CreateController();

        (await controller.CreateCrianca(createRequest)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
        (await controller.UpdateCrianca(10, updateRequest)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.DeleteCrianca(10)).Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
    }

    [Fact]
    public async Task VincularUpdateAndDesvincularResponsavel_ReturnExpectedResponses()
    {
        var createRequest = new CreateResponsavelRequest { ResponsavelPessoaId = 20, PodeRetirar = true };
        _kidsServiceMock.Setup(s => s.VincularResponsavelAsync(10, createRequest))
            .ReturnsAsync(new ResponsavelCriancaDto { Id = 5, CriancaPessoaId = 10, ResponsavelPessoaId = 20 });

        var updateRequest = new UpdateResponsavelRequest { PodeRetirar = false, Ativo = true };
        _kidsServiceMock.Setup(s => s.UpdateResponsavelAsync(5, updateRequest))
            .ReturnsAsync(new ResponsavelCriancaDto { Id = 5, CriancaPessoaId = 10, ResponsavelPessoaId = 20, PodeRetirar = false });
        _kidsServiceMock.Setup(s => s.DesvincularResponsavelAsync(5)).Returns(Task.CompletedTask);

        var controller = CreateController();

        (await controller.VincularResponsavel(10, createRequest)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
        (await controller.UpdateResponsavel(5, updateRequest)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.DesvincularResponsavel(5)).Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
    }

    [Fact]
    public async Task CheckinCheckoutAndHistorico_ReturnExpectedResponses()
    {
        var checkinRequest = new CheckinRequest { CriancaPessoaId = 10, Metodo = "ADMIN" };
        _kidsServiceMock.Setup(s => s.CheckinAsync(checkinRequest)).ReturnsAsync(new CheckinResponse { CheckinId = 1, CodigoSessao = "ABC" });

        var checkoutRequest = new CheckoutRequest { CriancaPessoaId = 10, CodigoSessao = "ABC", CheckoutByPessoaId = 30 };
        _kidsServiceMock.Setup(s => s.CheckoutAsync(checkoutRequest)).Returns(Task.CompletedTask);

        _kidsServiceMock.Setup(s => s.GetHistoricoCheckinsAsync(10)).ReturnsAsync(new List<KidsCheckinDto>());

        var controller = CreateController();

        (await controller.Checkin(checkinRequest)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.Checkout(checkoutRequest)).Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await controller.GetHistoricoCheckins(10)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }
}
