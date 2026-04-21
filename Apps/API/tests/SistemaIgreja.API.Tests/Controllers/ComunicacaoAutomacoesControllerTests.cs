using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoAutomacoesControllerTests
{
    [Fact]
    public async Task GetHistorico_ReturnsOk()
    {
        var serviceMock = new Mock<IComunicacaoAutomacaoService>();
        var query = new ComunicacaoAutomacaoHistoricoQueryDto { Page = 1, PageSize = 20 };
        serviceMock.Setup(s => s.GetHistoricoAsync(query))
            .ReturnsAsync(new PagedResultDto<ComunicacaoAutomacaoHistoricoItemDto>
            {
                Items = [],
                Total = 0,
                Page = 1,
                PageSize = 20
            });

        var controller = new ComunicacaoAutomacoesController(serviceMock.Object);
        var result = await controller.GetHistorico(query);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }
}
