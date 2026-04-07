using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoTemplatesControllerTests
{
    private readonly Mock<IComunicacaoTemplateService> _serviceMock = new();
    private readonly ComunicacaoTemplatesController _controller;

    public ComunicacaoTemplatesControllerTests()
    {
        _controller = new ComunicacaoTemplatesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTemplateDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ComunicacaoTemplateDetalheDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarComunicacaoTemplateDto>()))
            .ReturnsAsync(new ComunicacaoTemplateDetalheDto
            {
                Id = 8,
                Nome = "Template",
                Canal = CanalComunicacao.WhatsApp
            });

        var result = await _controller.Create(new CriarComunicacaoTemplateDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTemplateDoesNotExist()
    {
        _serviceMock.Setup(s => s.UpdateAsync(4, It.IsAny<AtualizarComunicacaoTemplateDto>()))
            .ThrowsAsync(new ArgumentException("Template não encontrado"));

        var result = await _controller.Update(4, new AtualizarComunicacaoTemplateDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }
}
