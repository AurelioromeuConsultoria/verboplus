using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class VisitanteServiceTests
{
    private readonly Mock<IVisitanteRepository> _repoMock = new();
    private readonly Mock<IMensagemAgendadaService> _msgServiceMock = new();
    private readonly VisitanteService _service;

    public VisitanteServiceTests()
    {
        _service = new VisitanteService(_repoMock.Object, _msgServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesVisitor_AndSchedulesMessages()
    {
        var dto = new CriarVisitanteDto { Nome = "Joao", Telefone = "123", DataVisita = DateTime.UtcNow };
        var created = new Visitante { Id = 5, Nome = dto.Nome, Telefone = dto.Telefone, DataVisita = dto.DataVisita };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Visitante>())).ReturnsAsync(created);

        var result = await _service.CreateAsync(dto);

        result.Id.Should().Be(5);
        _msgServiceMock.Verify(m => m.AgendarMensagensParaVisitanteAsync(5), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Visitante?)null);
        var dto = new AtualizarVisitanteDto { Nome = "X", Telefone = "1", DataVisita = DateTime.UtcNow };
        await _service.Invoking(s => s.UpdateAsync(1, dto)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        _repoMock.Setup(r => r.DeleteAsync(7)).Returns(Task.CompletedTask);
        await _service.DeleteAsync(7);
        _repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
    }
}
