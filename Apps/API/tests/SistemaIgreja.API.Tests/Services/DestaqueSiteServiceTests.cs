using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class DestaqueSiteServiceTests
{
    private readonly Mock<IDestaqueSiteRepository> _repositoryMock = new();
    private readonly DestaqueSiteService _service;

    public DestaqueSiteServiceTests()
    {
        _service = new DestaqueSiteService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_MapsDto()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<DestaqueSite>()))
            .ReturnsAsync((DestaqueSite destaque) =>
            {
                destaque.Id = 4;
                return destaque;
            });

        var result = await _service.CreateAsync(new CriarDestaqueSiteDto
        {
            Texto = "Banner principal",
            Descricao = "Descricao",
            Url = "/portal",
            Imagem = "/banner.png"
        });

        result.Id.Should().Be(4);
        result.Texto.Should().Be("Banner principal");
        result.Url.Should().Be("/portal");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenDestaqueDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((DestaqueSite?)null);

        var act = () => _service.UpdateAsync(2, new AtualizarDestaqueSiteDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Destaque não encontrado");
    }
}
