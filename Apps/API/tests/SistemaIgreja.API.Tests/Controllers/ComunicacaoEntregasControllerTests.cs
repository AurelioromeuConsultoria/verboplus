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
}
