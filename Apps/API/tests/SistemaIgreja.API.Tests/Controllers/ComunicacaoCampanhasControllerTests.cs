using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoCampanhasControllerTests
{
    private readonly Mock<IComunicacaoCampanhaService> _serviceMock = new();
    private readonly Mock<IComunicacaoEntregaService> _entregaServiceMock = new();
    private readonly ComunicacaoCampanhasController _controller;

    public ComunicacaoCampanhasControllerTests()
    {
        _controller = new ComunicacaoCampanhasController(_serviceMock.Object, _entregaServiceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCampanhaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ComunicacaoCampanhaDetalheDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task GetEntregas_ReturnsOk()
    {
        _entregaServiceMock.Setup(s => s.GetByCampanhaIdAsync(3))
            .ReturnsAsync(new List<ComunicacaoEntregaResumoDto>());

        var result = await _controller.GetEntregas(3);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCampanhaDoesNotExist()
    {
        _serviceMock.Setup(s => s.UpdateAsync(7, It.IsAny<AtualizarComunicacaoCampanhaDto>()))
            .ThrowsAsync(new ArgumentException("Campanha não encontrada"));

        var result = await _controller.Update(7, new AtualizarComunicacaoCampanhaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task GetPaged_ReturnsOkWithPayload()
    {
        var dto = new PagedResultDto<ComunicacaoCampanhaResumoDto>
        {
            Items = [new ComunicacaoCampanhaResumoDto { Id = 2, Nome = "Páscoa" }],
            Total = 1,
            Page = 1,
            PageSize = 20
        };
        _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<ComunicacaoCampanhaPagedQueryDto>())).ReturnsAsync(dto);

        var result = await _controller.GetPaged(new ComunicacaoCampanhaPagedQueryDto());

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetStats_ReturnsOkWithPayload()
    {
        var dto = new ComunicacaoStatsDto();
        _serviceMock.Setup(s => s.GetStatsAsync()).ReturnsAsync(dto);

        var result = await _controller.GetStats();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenCampanhaExists()
    {
        var dto = new ComunicacaoCampanhaDetalheDto { Id = 4, Nome = "Natal" };
        _serviceMock.Setup(s => s.GetByIdAsync(4)).ReturnsAsync(dto);

        var result = await _controller.GetById(4);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new ComunicacaoCampanhaDetalheDto { Id = 5, Nome = "Conferência" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarComunicacaoCampanhaDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarComunicacaoCampanhaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarComunicacaoCampanhaDto { Nome = "Atualizada" };
        var updated = new ComunicacaoCampanhaDetalheDto { Id = 7, Nome = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(7, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(7, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }
}
