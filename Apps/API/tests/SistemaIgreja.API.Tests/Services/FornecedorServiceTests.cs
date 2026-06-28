using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class FornecedorServiceTests
{
    private readonly Mock<IFornecedorRepository> _repositoryMock = new();
    private readonly FornecedorService _service;

    public FornecedorServiceTests()
    {
        _service = new FornecedorService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_MapsFornecedorFields()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync((Fornecedor fornecedor) =>
            {
                fornecedor.Id = 4;
                return fornecedor;
            });

        var result = await _service.CreateAsync(new CriarFornecedorDto
        {
            Nome = "Papelaria Central",
            ContatoEmail = "contato@papelaria.com"
        });

        result.Id.Should().Be(4);
        result.Nome.Should().Be("Papelaria Central");
        result.ContatoEmail.Should().Be("contato@papelaria.com");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenFornecedorDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((Fornecedor?)null);

        var act = () => _service.UpdateAsync(4, new AtualizarFornecedorDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Fornecedor não encontrado");
    }
}
