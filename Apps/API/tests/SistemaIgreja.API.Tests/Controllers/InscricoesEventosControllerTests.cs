using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class InscricoesEventosControllerTests
{
    private readonly Mock<IInscricaoEventoService> _serviceMock = new();
    private readonly InscricoesEventosController _controller;

    public InscricoesEventosControllerTests()
    {
        _controller = new InscricoesEventosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetEstatisticas_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.ObterEstatisticasAsync(5)).ThrowsAsync(new ArgumentException("Evento não encontrado"));

        var result = await _controller.GetEstatisticas(5);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarInscricaoEventoDto>()))
            .ThrowsAsync(new InvalidOperationException("Evento lotado"));

        var result = await _controller.Create(new CriarInscricaoEventoDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Confirmar_ReturnsOk_WhenServiceSucceeds()
    {
        var inscricao = new InscricaoEventoDto { Id = 8, EventoId = 2, Status = StatusInscricao.Confirmada };
        _serviceMock.Setup(s => s.ConfirmarInscricaoAsync(8)).ReturnsAsync(inscricao);

        var result = await _controller.Confirmar(8);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(inscricao);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.DeleteAsync(12)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(12);

        result.Should().BeOfType<NoContentResult>();
    }
}
