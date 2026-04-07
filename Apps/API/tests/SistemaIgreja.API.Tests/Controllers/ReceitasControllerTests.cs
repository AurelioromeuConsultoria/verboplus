using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class ReceitasControllerTests
{
    private readonly Mock<IReceitaService> _serviceMock = new();
    private readonly ReceitasController _controller;

    public ReceitasControllerTests()
    {
        _controller = new ReceitasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ReceitaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new ReceitaDto { Id = 5, Descricao = "Oferta", Status = StatusReceita.Recebida };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarReceitaDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarReceitaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarReceitaDto>()))
            .ThrowsAsync(new ArgumentException("Receita não encontrada"));

        var result = await _controller.Update(5, new AtualizarReceitaDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
