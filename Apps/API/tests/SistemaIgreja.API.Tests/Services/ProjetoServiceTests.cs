using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ProjetoServiceTests
{
    private readonly Mock<IProjetoRepository> _repositoryMock = new();
    private readonly ProjetoService _service;

    public ProjetoServiceTests()
    {
        _service = new ProjetoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesActiveProjeto()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Projeto>()))
            .ReturnsAsync((Projeto projeto) =>
            {
                projeto.Id = 3;
                return projeto;
            });

        var result = await _service.CreateAsync(new CriarProjetoDto
        {
            Nome = "Reforma",
            Orcamento = 10000
        });

        result.Id.Should().Be(3);
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenProjetoDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Projeto?)null);

        var act = () => _service.UpdateAsync(3, new AtualizarProjetoDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Projeto não encontrado");
    }
}
