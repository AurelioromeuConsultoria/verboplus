using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaReceitaServiceTests
{
    private readonly Mock<ICategoriaReceitaRepository> _repositoryMock = new();
    private readonly CategoriaReceitaService _service;

    public CategoriaReceitaServiceTests()
    {
        _service = new CategoriaReceitaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_RespectsAtivoFromDto()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CategoriaReceita>()))
            .ReturnsAsync((CategoriaReceita categoria) =>
            {
                categoria.Id = 9;
                return categoria;
            });

        var result = await _service.CreateAsync(new CriarCategoriaReceitaDto
        {
            Nome = "Oferta",
            Ativo = false
        });

        result.Id.Should().Be(9);
        result.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((CategoriaReceita?)null);

        var act = () => _service.UpdateAsync(9, new AtualizarCategoriaReceitaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria de receita não encontrada");
    }
}
