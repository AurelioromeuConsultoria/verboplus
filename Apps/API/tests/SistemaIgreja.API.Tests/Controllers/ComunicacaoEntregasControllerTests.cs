using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoEntregasControllerTests
{
    private readonly Mock<IComunicacaoEntregaService> _serviceMock = new();
    private readonly Mock<IComunicacaoProcessamentoService> _processamentoServiceMock = new();
    private readonly ComunicacaoEntregasController _controller;

    public ComunicacaoEntregasControllerTests()
    {
        _controller = new ComunicacaoEntregasController(_serviceMock.Object, _processamentoServiceMock.Object);
    }

    [Fact]
    public async Task GetPaged_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<ComunicacaoEntregaPagedQueryDto>()))
            .ReturnsAsync(new PagedResultDto<ComunicacaoEntregaResumoDto>
            {
                Items = [],
                Total = 0,
                Page = 1,
                PageSize = 20
            });

        var result = await _controller.GetPaged(new ComunicacaoEntregaPagedQueryDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task ProcessarPendentes_ReturnsOkWithProcessedCount()
    {
        _processamentoServiceMock.Setup(s => s.ProcessarPendentesAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        var result = await _controller.ProcessarPendentes(25);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task Reprocessar_ReturnsConflict_WhenEntregaNaoEhElegivel()
    {
        _serviceMock.Setup(s => s.PrepararReprocessamentoAsync(9))
            .ThrowsAsync(new InvalidOperationException("Esta entrega não é elegível para reprocessamento."));

        var result = await _controller.Reprocessar(9);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>();
    }

    [Fact]
    public async Task Reprocessar_ReturnsOk_WhenEntregaEhReprocessada()
    {
        _serviceMock.Setup(s => s.PrepararReprocessamentoAsync(8))
            .ReturnsAsync(new ComunicacaoEntregaResumoDto
            {
                Id = 8,
                Status = SistemaIgreja.Domain.Entities.StatusComunicacaoEntrega.Pendente
            });
        _processamentoServiceMock.Setup(s => s.ProcessarEntregaAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _serviceMock.Setup(s => s.GetByIdAsync(8))
            .ReturnsAsync(new ComunicacaoEntregaResumoDto
            {
                Id = 8,
                Status = SistemaIgreja.Domain.Entities.StatusComunicacaoEntrega.Enviado
            });

        var result = await _controller.Reprocessar(8);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task ProcessarPendentes_UsesDefaultLimit_WhenNoLimitIsProvided()
    {
        _processamentoServiceMock.Setup(s => s.ProcessarPendentesAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await _controller.ProcessarPendentes();

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        _processamentoServiceMock.Verify(s => s.ProcessarPendentesAsync(50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reprocessar_ReturnsNotFound_WhenEntregaNaoExiste()
    {
        _serviceMock.Setup(s => s.PrepararReprocessamentoAsync(11))
            .ThrowsAsync(new ArgumentException("Entrega não encontrada."));

        var result = await _controller.Reprocessar(11);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Reprocessar_ReturnsNotFound_WhenEntregaDisappearsAfterProcessing()
    {
        _serviceMock.Setup(s => s.PrepararReprocessamentoAsync(12))
            .ReturnsAsync(new ComunicacaoEntregaResumoDto { Id = 12 });
        _processamentoServiceMock.Setup(s => s.ProcessarEntregaAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _serviceMock.Setup(s => s.GetByIdAsync(12))
            .ReturnsAsync((ComunicacaoEntregaResumoDto?)null);

        var result = await _controller.Reprocessar(12);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }
}
