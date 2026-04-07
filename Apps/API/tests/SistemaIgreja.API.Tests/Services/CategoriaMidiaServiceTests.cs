using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaMidiaServiceTests
{
    private readonly Mock<ICategoriaMidiaRepository> _repositoryMock = new();
    private readonly CategoriaMidiaService _service;

    public CategoriaMidiaServiceTests()
    {
        _service = new CategoriaMidiaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync((CategoriaMidia?)null);

        var act = () => _service.UpdateAsync(6, new AtualizarCategoriaMidiaDto { Nome = "Midia" });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria não encontrada");
    }
}
