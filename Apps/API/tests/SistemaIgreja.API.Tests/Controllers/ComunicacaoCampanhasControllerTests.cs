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
}
