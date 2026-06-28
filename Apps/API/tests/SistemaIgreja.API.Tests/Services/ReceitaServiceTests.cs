using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ReceitaServiceTests
{
    private readonly Mock<IReceitaRepository> _repositoryMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly ReceitaService _service;

    public ReceitaServiceTests()
    {
        _service = new ReceitaService(_repositoryMock.Object, _pessoaRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReloadsReceita()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Receita>()))
            .ReturnsAsync((Receita receita) =>
            {
                receita.Id = 10;
                return receita;
            });
        _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(CriarReceita(10));

        var result = await _service.CreateAsync(new CriarReceitaDto
        {
            Descricao = "Oferta",
            Valor = 150,
            DataRecebimento = new DateTime(2026, 4, 1),
            Status = StatusReceita.Recebida,
            CategoriaReceitaId = 2,
            UsuarioId = 7
        });

        result.Id.Should().Be(10);
        result.CategoriaReceitaNome.Should().Be("Dizimos");
        result.UsuarioNome.Should().Be("Tesouraria");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenReceitaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Receita?)null);

        var act = () => _service.UpdateAsync(5, new AtualizarReceitaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Receita não encontrada");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReloadsReceita()
    {
        var receita = CriarReceita(5);
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(receita);
        _repositoryMock.Setup(r => r.UpdateAsync(receita)).ReturnsAsync(receita);

        var result = await _service.UpdateAsync(5, new AtualizarReceitaDto
        {
            Descricao = "Oferta especial",
            Valor = 200,
            DataRecebimento = receita.DataRecebimento,
            DataConfirmacao = receita.DataConfirmacao,
            Status = StatusReceita.Cancelada,
            Observacoes = "Cancelada",
            ComprovanteUrl = "/comp.png",
            CategoriaReceitaId = 3,
            ContaBancariaId = 4,
            CentroCustoId = 5,
            ProjetoId = 6,
            UsuarioId = 8
        });

        receita.Descricao.Should().Be("Oferta especial");
        receita.Status.Should().Be(StatusReceita.Cancelada);
        result.StatusDescricao.Should().Be("Cancelada");
    }

    private static Receita CriarReceita(int id)
    {
        return new Receita
        {
            Id = id,
            Descricao = "Oferta",
            Valor = 150,
            DataRecebimento = new DateTime(2026, 4, 1),
            DataConfirmacao = new DateTime(2026, 4, 1),
            Status = StatusReceita.Recebida,
            CategoriaReceitaId = 2,
            CategoriaReceita = new CategoriaReceita { Id = 2, Nome = "Dizimos" },
            UsuarioId = 7,
            Usuario = new Usuario
            {
                Id = 7,
                PessoaId = 70,
                Pessoa = new Pessoa { Id = 70, Nome = "Tesouraria", TipoPessoa = TipoPessoa.Adulto, Ativo = true }
            }
        };
    }
}
