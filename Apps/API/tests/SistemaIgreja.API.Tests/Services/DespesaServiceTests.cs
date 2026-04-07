using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class DespesaServiceTests
{
    private readonly Mock<IDespesaRepository> _repositoryMock = new();
    private readonly DespesaService _service;

    public DespesaServiceTests()
    {
        _service = new DespesaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReloadsDespesa()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Despesa>()))
            .ReturnsAsync((Despesa despesa) =>
            {
                despesa.Id = 11;
                return despesa;
            });
        _repositoryMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(CriarDespesa(11));

        var result = await _service.CreateAsync(new CriarDespesaDto
        {
            Descricao = "Conta de luz",
            Valor = 320,
            DataVencimento = new DateTime(2026, 4, 10),
            Status = StatusDespesa.Paga,
            FornecedorId = 2,
            UsuarioId = 7
        });

        result.Id.Should().Be(11);
        result.FornecedorNome.Should().Be("Energia SA");
        result.StatusDescricao.Should().Be("Paga");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenDespesaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync((Despesa?)null);

        var act = () => _service.UpdateAsync(6, new AtualizarDespesaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Despesa não encontrada");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReloadsDespesa()
    {
        var despesa = CriarDespesa(6);
        _repositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(despesa);
        _repositoryMock.Setup(r => r.UpdateAsync(despesa)).ReturnsAsync(despesa);

        var result = await _service.UpdateAsync(6, new AtualizarDespesaDto
        {
            Descricao = "Conta de agua",
            Valor = 220,
            DataVencimento = despesa.DataVencimento,
            DataPagamento = despesa.DataPagamento,
            Status = StatusDespesa.Cancelada,
            Observacoes = "Nao pagar",
            ComprovanteUrl = "/comp.png",
            FornecedorId = 3,
            CategoriaDespesaId = 4,
            ContaBancariaId = 5,
            CentroCustoId = 6,
            ProjetoId = 7,
            UsuarioId = 8
        });

        despesa.Descricao.Should().Be("Conta de agua");
        despesa.Status.Should().Be(StatusDespesa.Cancelada);
        result.StatusDescricao.Should().Be("Cancelada");
    }

    private static Despesa CriarDespesa(int id)
    {
        return new Despesa
        {
            Id = id,
            Descricao = "Conta de luz",
            Valor = 320,
            DataVencimento = new DateTime(2026, 4, 10),
            DataPagamento = new DateTime(2026, 4, 8),
            Status = StatusDespesa.Paga,
            FornecedorId = 2,
            Fornecedor = new Fornecedor { Id = 2, Nome = "Energia SA" },
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
