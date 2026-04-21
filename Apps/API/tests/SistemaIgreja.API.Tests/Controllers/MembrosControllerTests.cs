using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class MembrosControllerTests
{
    [Fact]
    public async Task Cadastrar_ReturnsBadRequest_WhenServiceReportsFailure()
    {
        var serviceMock = new Mock<IMembroCadastroService>();
        var dto = new CadastroMembroDto { Nome = "Ana", Email = "ana@app.com", WhatsApp = "11999999999", DataNascimento = new DateTime(1990, 1, 1) };
        serviceMock.Setup(s => s.CadastrarAsync(dto))
            .ReturnsAsync(new CadastroMembroResultadoDto { Sucesso = false, Mensagem = "Email inválido" });

        var controller = new MembrosController(serviceMock.Object);
        var result = await controller.Cadastrar(dto);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cadastrar_ReturnsOk_WhenSuccessful()
    {
        var serviceMock = new Mock<IMembroCadastroService>();
        var dto = new CadastroMembroDto { Nome = "Ana", Email = "ana@app.com", WhatsApp = "11999999999", DataNascimento = new DateTime(1990, 1, 1) };
        serviceMock.Setup(s => s.CadastrarAsync(dto))
            .ReturnsAsync(new CadastroMembroResultadoDto { Sucesso = true, Mensagem = "Cadastro realizado" });

        var controller = new MembrosController(serviceMock.Object);
        var result = await controller.Cadastrar(dto);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }
}
