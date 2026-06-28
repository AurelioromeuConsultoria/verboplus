using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaPatrimonioServiceTests
{
    private readonly Mock<ICategoriaPatrimonioRepository> _repositoryMock = new();
    private readonly CategoriaPatrimonioService _service;

    public CategoriaPatrimonioServiceTests()
    {
        _service = new CategoriaPatrimonioService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenNomeAlreadyExists()
    {
        _repositoryMock.Setup(r => r.GetByNomeAsync("Equipamentos"))
            .ReturnsAsync(new CategoriaPatrimonio { Id = 1, Nome = "Equipamentos" });

        var act = () => _service.CreateAsync(new CriarCategoriaPatrimonioDto
        {
            Nome = "Equipamentos"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe uma categoria de patrimônio com este nome");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((CategoriaPatrimonio?)null);

        var act = () => _service.UpdateAsync(4, new AtualizarCategoriaPatrimonioDto
        {
            Nome = "Audio"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria de patrimônio não encontrada");
    }

    [Fact]
    public async Task CreateAsync_MapsAndReturnsCreatedCategoria()
    {
        _repositoryMock.Setup(r => r.GetByNomeAsync("Audio")).ReturnsAsync((CategoriaPatrimonio?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CategoriaPatrimonio>()))
            .ReturnsAsync((CategoriaPatrimonio item) =>
            {
                item.Id = 6;
                return item;
            });

        var result = await _service.CreateAsync(new CriarCategoriaPatrimonioDto
        {
            Nome = " Audio ",
            Descricao = " Equipamentos de som "
        });

        result.Id.Should().Be(6);
        result.Nome.Should().Be("Audio");
        result.Descricao.Should().Be("Equipamentos de som");
        result.Ativo.Should().BeTrue();
    }
}
