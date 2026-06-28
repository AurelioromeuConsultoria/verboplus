using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class PessoaPerfilServiceTests
{
    private readonly Mock<IPessoaPerfilRepository> _repositoryMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<ILogger<PessoaPerfilService>> _loggerMock = new();
    private readonly PessoaPerfilService _service;

    public PessoaPerfilServiceTests()
    {
        _service = new PessoaPerfilService(
            _repositoryMock.Object,
            _pessoaRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenPessoaDoesNotExist()
    {
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync((Pessoa?)null);

        var act = () => _service.CreateAsync(new CriarPessoaPerfilDto
        {
            PessoaId = 15,
            Perfil = PerfilPessoa.Membro
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Pessoa não encontrada");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenActivePerfilAlreadyExists()
    {
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Pessoa { Id = 15, Nome = "Pessoa", TipoPessoa = TipoPessoa.Adulto, Ativo = true });
        _repositoryMock.Setup(r => r.GetPerfilAtivoAsync(15, PerfilPessoa.Membro))
            .ReturnsAsync(new PessoaPerfil { Id = 3, PessoaId = 15, Perfil = PerfilPessoa.Membro });

        var act = () => _service.CreateAsync(new CriarPessoaPerfilDto
        {
            PessoaId = 15,
            Perfil = PerfilPessoa.Membro
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um perfil Membro ativo para esta pessoa");
    }

    [Fact]
    public async Task CreateAsync_CreatesPerfil_WhenDataIsValid()
    {
        var pessoa = new Pessoa { Id = 15, Nome = "Pessoa", TipoPessoa = TipoPessoa.Adulto, Ativo = true };
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pessoa);
        _repositoryMock.Setup(r => r.GetPerfilAtivoAsync(15, PerfilPessoa.Lider))
            .ReturnsAsync((PessoaPerfil?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<PessoaPerfil>()))
            .ReturnsAsync((PessoaPerfil perfil) =>
            {
                perfil.Id = 10;
                return perfil;
            });

        var result = await _service.CreateAsync(new CriarPessoaPerfilDto
        {
            PessoaId = 15,
            Perfil = PerfilPessoa.Lider
        });

        result.Id.Should().Be(10);
        result.PessoaId.Should().Be(15);
        result.Perfil.Should().Be(PerfilPessoa.Lider);
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenPerfilDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PessoaPerfil?)null);

        var act = () => _service.UpdateAsync(99, new AtualizarPessoaPerfilDto
        {
            Perfil = PerfilPessoa.Admin
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Perfil não encontrado");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPerfil_WhenDataIsValid()
    {
        var pessoa = new Pessoa { Id = 15, Nome = "Pessoa", TipoPessoa = TipoPessoa.Adulto, Ativo = true };
        var perfil = new PessoaPerfil
        {
            Id = 9,
            PessoaId = 15,
            Perfil = PerfilPessoa.Membro,
            DataInicio = DateTime.UtcNow.AddDays(-10)
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(perfil);
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pessoa);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PessoaPerfil>()))
            .ReturnsAsync((PessoaPerfil item) => item);
        _repositoryMock.Setup(r => r.GetByIdAsync(9))
            .ReturnsAsync(new PessoaPerfil
            {
                Id = 9,
                PessoaId = 15,
                Perfil = PerfilPessoa.Admin,
                DataInicio = perfil.DataInicio,
                DataFim = DateTime.UtcNow
            });

        var result = await _service.UpdateAsync(9, new AtualizarPessoaPerfilDto
        {
            Perfil = PerfilPessoa.Admin,
            DataFim = DateTime.UtcNow
        });

        result.Id.Should().Be(9);
        result.Perfil.Should().Be(PerfilPessoa.Admin);
        result.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(9)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(9);

        _repositoryMock.Verify(r => r.DeleteAsync(9), Times.Once);
    }
}
