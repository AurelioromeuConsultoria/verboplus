using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CentroCustoServiceTests
{
    private readonly Mock<ICentroCustoRepository> _repositoryMock = new();
    private readonly CentroCustoService _service;

    public CentroCustoServiceTests()
    {
        _service = new CentroCustoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesActiveCentroCusto()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CentroCusto>()))
            .ReturnsAsync((CentroCusto centro) =>
            {
                centro.Id = 14;
                return centro;
            });

        var result = await _service.CreateAsync(new CriarCentroCustoDto
        {
            Nome = "Administrativo"
        });

        result.Id.Should().Be(14);
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCentroCustoDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(14)).ReturnsAsync((CentroCusto?)null);

        var act = () => _service.UpdateAsync(14, new AtualizarCentroCustoDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Centro de custo não encontrado");
    }
}
