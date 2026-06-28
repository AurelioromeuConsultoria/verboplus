using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaNoticiaServiceTests
{
    private readonly Mock<ICategoriaNoticiaRepository> _repositoryMock = new();
    private readonly CategoriaNoticiaService _service;

    public CategoriaNoticiaServiceTests()
    {
        _service = new CategoriaNoticiaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync((CategoriaNoticia?)null);

        var act = () => _service.UpdateAsync(6, new AtualizarCategoriaNoticiaDto { Nome = "Atualizada" });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria não encontrada");
    }
}
