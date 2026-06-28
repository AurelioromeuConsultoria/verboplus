using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ContaBancariaServiceTests
{
    private readonly Mock<IContaBancariaRepository> _repositoryMock = new();
    private readonly ContaBancariaService _service;

    public ContaBancariaServiceTests()
    {
        _service = new ContaBancariaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesActiveConta()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ContaBancaria>()))
            .ReturnsAsync((ContaBancaria conta) =>
            {
                conta.Id = 12;
                return conta;
            });

        var result = await _service.CreateAsync(new CriarContaBancariaDto
        {
            Nome = "Conta Principal",
            Banco = "Banco A",
            Agencia = "0001",
            Conta = "12345-6",
            TipoConta = "Corrente",
            SaldoInicial = 1000
        });

        result.Id.Should().Be(12);
        result.Ativo.Should().BeTrue();
        result.Banco.Should().Be("Banco A");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenContaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(12)).ReturnsAsync((ContaBancaria?)null);

        var act = () => _service.UpdateAsync(12, new AtualizarContaBancariaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Conta bancária não encontrada");
    }
}
