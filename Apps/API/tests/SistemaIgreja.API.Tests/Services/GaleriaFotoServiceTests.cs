using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class GaleriaFotoServiceTests
{
    private readonly Mock<IGaleriaFotoRepository> _repositoryMock = new();
    private readonly Mock<IGaleriaFotoItemRepository> _itemRepositoryMock = new();
    private readonly GaleriaFotoService _service;

    public GaleriaFotoServiceTests()
    {
        _service = new GaleriaFotoService(_repositoryMock.Object, _itemRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_GeneratesUploadsPathAndMapsDto()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<GaleriaFoto>()))
            .ReturnsAsync((GaleriaFoto galeria) =>
            {
                galeria.Id = 12;
                return galeria;
            });

        var result = await _service.CreateAsync(new CriarGaleriaFotoDto
        {
            Nome = "Retiro",
            Data = new DateTime(2026, 4, 6),
            CategoriaMidiaId = 2,
            Ativo = true
        });

        result.Id.Should().Be(12);
        result.CaminhoDiretorio.Should().StartWith("uploads");
        result.Nome.Should().Be("Retiro");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenGaleriaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((GaleriaFoto?)null);

        var act = () => _service.UpdateAsync(8, new AtualizarGaleriaFotoDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Galeria não encontrada");
    }

    [Fact]
    public async Task ListarFotosAsync_PrefersItemsFromDatabase()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new GaleriaFoto
        {
            Id = 3,
            CaminhoDiretorio = "uploads/fotos/galeria"
        });
        _itemRepositoryMock.Setup(r => r.GetByGaleriaIdAsync(3))
            .ReturnsAsync(
            [
                new GaleriaFotoItem { NomeArquivo = "a.jpg", Destaque = true },
                new GaleriaFotoItem { NomeArquivo = "b.jpg", Destaque = false }
            ]);

        var result = await _service.ListarFotosAsync(3, "/tmp");

        result.Should().HaveCount(2);
        result[0].NomeArquivo.Should().Be("a.jpg");
        result[0].Destaque.Should().BeTrue();
    }
}
