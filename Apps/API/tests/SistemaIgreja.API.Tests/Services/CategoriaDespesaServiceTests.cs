using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaDespesaServiceTests
{
    private readonly Mock<ICategoriaDespesaRepository> _repositoryMock = new();
    private readonly CategoriaDespesaService _service;

    public CategoriaDespesaServiceTests()
    {
        _service = new CategoriaDespesaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesActiveCategoria()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CategoriaDespesa>()))
            .ReturnsAsync((CategoriaDespesa categoria) =>
            {
                categoria.Id = 2;
                return categoria;
            });

        var result = await _service.CreateAsync(new CriarCategoriaDespesaDto
        {
            Nome = "Infraestrutura"
        });

        result.Id.Should().Be(2);
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((CategoriaDespesa?)null);

        var act = () => _service.UpdateAsync(2, new AtualizarCategoriaDespesaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria de despesa não encontrada");
    }
}
