using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class PatrimonioMovimentacaoServiceTests
{
    private readonly Mock<IPatrimonioMovimentacaoRepository> _repositoryMock = new();
    private readonly Mock<IPatrimonioItemRepository> _patrimonioRepositoryMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserContextMock = new();
    private readonly PatrimonioMovimentacaoService _service;

    public PatrimonioMovimentacaoServiceTests()
    {
        _currentUserContextMock.SetupGet(c => c.UserId).Returns(10);
        _currentUserContextMock.SetupGet(c => c.UserName).Returns("Marco");
        _currentUserContextMock.SetupGet(c => c.UserEmail).Returns("marco@example.com");

        _service = new PatrimonioMovimentacaoService(
            _repositoryMock.Object,
            _patrimonioRepositoryMock.Object,
            _currentUserContextMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenPatrimonioDoesNotExist()
    {
        _patrimonioRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((PatrimonioItem?)null);

        var act = () => _service.CreateAsync(3, new CriarPatrimonioMovimentacaoDto
        {
            TipoMovimentacao = "Emprestimo"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Item patrimonial não encontrado");
    }

    [Fact]
    public async Task CreateAsync_UpdatesPatrimonioForEmprestimo()
    {
        var patrimonio = new PatrimonioItem
        {
            Id = 5,
            Nome = "Projetor",
            Status = "EmUso",
            Localizacao = "Auditorio"
        };

        _patrimonioRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(patrimonio);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<PatrimonioMovimentacao>()))
            .ReturnsAsync((PatrimonioMovimentacao mov) =>
            {
                mov.Id = 12;
                return mov;
            });
        _patrimonioRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PatrimonioItem>()))
            .ReturnsAsync((PatrimonioItem item) => item);

        var result = await _service.CreateAsync(5, new CriarPatrimonioMovimentacaoDto
        {
            TipoMovimentacao = "Emprestimo",
            Destino = "Congregacao Centro",
            Observacoes = "Uso externo"
        });

        result.Id.Should().Be(12);
        patrimonio.Status.Should().Be("Emprestado");
        patrimonio.Localizacao.Should().Be("Congregacao Centro");
        _patrimonioRepositoryMock.Verify(r => r.UpdateAsync(It.Is<PatrimonioItem>(p => p.Id == 5)), Times.Once);
    }

    [Fact]
    public async Task RegistrarCadastroInicialAsync_UsesCurrentUserContext()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<PatrimonioMovimentacao>()))
            .ReturnsAsync((PatrimonioMovimentacao mov) => mov);

        await _service.RegistrarCadastroInicialAsync(new PatrimonioItem
        {
            Id = 7,
            Localizacao = "Sala 2",
            DataCriacao = new DateTime(2026, 4, 6),
            ResponsavelPessoa = new Pessoa { Nome = "Aline" }
        });

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<PatrimonioMovimentacao>(m =>
            m.PatrimonioItemId == 7 &&
            m.TipoMovimentacao == "CadastroInicial" &&
            m.UsuarioId == 10 &&
            m.UsuarioNome == "Marco")), Times.Once);
    }
}
