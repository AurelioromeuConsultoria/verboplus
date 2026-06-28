using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EnqueteServiceTests
{
    private readonly Mock<IEnqueteRepository> _repositoryMock = new();
    private readonly EnqueteService _service;

    public EnqueteServiceTests()
    {
        _service = new EnqueteService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_SortsOptionsByOrder()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Enquete>()))
            .ReturnsAsync((Enquete enquete) =>
            {
                enquete.Id = 5;
                var id = 1;
                foreach (var opcao in enquete.Opcoes)
                {
                    opcao.Id = id++;
                }
                return enquete;
            });

        var result = await _service.CreateAsync(new CriarEnqueteDto
        {
            Titulo = "Pesquisa",
            DataInicio = new DateTime(2026, 4, 1),
            DataFim = new DateTime(2026, 4, 30),
            Opcoes = new List<CriarEnqueteOpcaoDto>
            {
                new() { Texto = "Segunda", Ordem = 2 },
                new() { Texto = "Primeira", Ordem = 1 }
            }
        });

        result.Opcoes.Select(o => o.Texto).Should().ContainInOrder("Primeira", "Segunda");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEnqueteDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Enquete?)null);

        var act = () => _service.UpdateAsync(3, new AtualizarEnqueteDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Enquete não encontrada");
    }

    [Fact]
    public async Task UpdateAsync_RemovesUpdatesAndCreatesOptions()
    {
        var enquete = new Enquete
        {
            Id = 11,
            Titulo = "Original",
            Opcoes =
            [
                new EnqueteOpcao { Id = 10, EnqueteId = 11, Texto = "A", Ordem = 1 },
                new EnqueteOpcao { Id = 20, EnqueteId = 11, Texto = "B", Ordem = 2 }
            ]
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(enquete);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Enquete>()))
            .ReturnsAsync((Enquete e) => e);

        var result = await _service.UpdateAsync(11, new AtualizarEnqueteDto
        {
            Titulo = "Atualizada",
            DataInicio = new DateTime(2026, 4, 1),
            DataFim = new DateTime(2026, 4, 30),
            Opcoes = new List<AtualizarEnqueteOpcaoDto>
            {
                new() { Id = 10, Texto = "A1", Ordem = 1 },
                new() { Texto = "Nova", Ordem = 3 }
            }
        });

        _repositoryMock.Verify(r => r.DeleteOpcaoAsync(20), Times.Once);
        _repositoryMock.Verify(r => r.UpdateOpcaoAsync(It.Is<EnqueteOpcao>(o => o.Id == 10 && o.Texto == "A1")), Times.Once);
        _repositoryMock.Verify(r => r.CreateOpcaoAsync(It.Is<EnqueteOpcao>(o => o.EnqueteId == 11 && o.Texto == "Nova")), Times.Once);
        result.Titulo.Should().Be("Atualizada");
    }
}
