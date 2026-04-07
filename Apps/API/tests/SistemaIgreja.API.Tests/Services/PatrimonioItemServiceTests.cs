using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class PatrimonioItemServiceTests
{
    private readonly Mock<IPatrimonioItemRepository> _repositoryMock = new();
    private readonly Mock<ICategoriaPatrimonioRepository> _categoriaRepositoryMock = new();
    private readonly Mock<IPatrimonioMovimentacaoService> _movimentacaoServiceMock = new();
    private readonly PatrimonioItemService _service;

    public PatrimonioItemServiceTests()
    {
        _service = new PatrimonioItemService(
            _repositoryMock.Object,
            _categoriaRepositoryMock.Object,
            _movimentacaoServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCodigoIsMissing()
    {
        var act = () => _service.CreateAsync(new CriarPatrimonioItemDto
        {
            Codigo = " ",
            Nome = "Projetor",
            CategoriaPatrimonioId = 1
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Código é obrigatório");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _categoriaRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((CategoriaPatrimonio?)null);

        var act = () => _service.CreateAsync(new CriarPatrimonioItemDto
        {
            Codigo = "PAT-001",
            Nome = "Projetor",
            CategoriaPatrimonioId = 2
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Categoria de patrimônio não encontrada");
    }

    [Fact]
    public async Task CreateAsync_RegistersInitialMovement()
    {
        _categoriaRepositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new CategoriaPatrimonio { Id = 2, Nome = "Equipamentos" });
        _repositoryMock.Setup(r => r.GetByCodigoAsync("PAT-001"))
            .ReturnsAsync((PatrimonioItem?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<PatrimonioItem>()))
            .ReturnsAsync((PatrimonioItem item) =>
            {
                item.Id = 7;
                return item;
            });
        _repositoryMock.Setup(r => r.GetByIdAsync(7))
            .ReturnsAsync(new PatrimonioItem
            {
                Id = 7,
                Codigo = "PAT-001",
                Nome = "Projetor",
                CategoriaPatrimonioId = 2,
                CategoriaPatrimonio = new CategoriaPatrimonio { Id = 2, Nome = "Equipamentos" },
                DataCriacao = DateTime.Now
            });

        var result = await _service.CreateAsync(new CriarPatrimonioItemDto
        {
            Codigo = " PAT-001 ",
            Nome = " Projetor ",
            CategoriaPatrimonioId = 2
        });

        result.Id.Should().Be(7);
        result.Codigo.Should().Be("PAT-001");
        result.Nome.Should().Be("Projetor");
        _movimentacaoServiceMock.Verify(s => s.RegistrarCadastroInicialAsync(It.Is<PatrimonioItem>(i => i.Id == 7)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenItemDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((PatrimonioItem?)null);

        var act = () => _service.UpdateAsync(9, new AtualizarPatrimonioItemDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Item patrimonial não encontrado");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCodigoAlreadyExistsForAnotherItem()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new PatrimonioItem
        {
            Id = 5,
            Codigo = "PAT-005",
            Nome = "Mixer",
            CategoriaPatrimonioId = 2
        });
        _categoriaRepositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new CategoriaPatrimonio { Id = 2, Nome = "Equipamentos" });
        _repositoryMock.Setup(r => r.GetByCodigoAsync("PAT-001"))
            .ReturnsAsync(new PatrimonioItem { Id = 3, Codigo = "PAT-001" });

        var act = () => _service.UpdateAsync(5, new AtualizarPatrimonioItemDto
        {
            Codigo = "PAT-001",
            Nome = "Mixer atualizado",
            CategoriaPatrimonioId = 2
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um item patrimonial com este código");
    }
}
