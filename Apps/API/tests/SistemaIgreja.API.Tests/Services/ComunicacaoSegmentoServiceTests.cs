using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoSegmentoServiceTests
{
    private readonly Mock<IComunicacaoSegmentoRepository> _repositoryMock = new();
    private readonly Mock<IComunicacaoAudienceResolver> _audienceResolverMock = new();
    private readonly ComunicacaoSegmentoService _service;

    public ComunicacaoSegmentoServiceTests()
    {
        _service = new ComunicacaoSegmentoService(_repositoryMock.Object, _audienceResolverMock.Object);
    }

    [Fact]
    public async Task GetEstimativaAsync_CalculaVolumePorCanal()
    {
        _audienceResolverMock.Setup(x => x.ResolveAsync("visitantes")).ReturnsAsync(
        [
            new ComunicacaoDestinatario { PessoaId = 1, WhatsApp = "5511999999999", Email = "a@a.com" },
            new ComunicacaoDestinatario { PessoaId = 2, Email = "b@b.com" },
            new ComunicacaoDestinatario { VisitanteId = 9, WhatsApp = "5511888888888" }
        ]);

        var result = await _service.GetEstimativaAsync("visitantes");

        result.TotalDestinatarios.Should().Be(3);
        result.ComWhatsApp.Should().Be(2);
        result.ComEmail.Should().Be(2);
        result.ComPush.Should().Be(2);
        result.ComNotificacaoInterna.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_CriaSegmentoBasico()
    {
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoSegmento>()))
            .ReturnsAsync((ComunicacaoSegmento item) =>
            {
                item.Id = 14;
                return item;
            });

        var result = await _service.CreateAsync(new CriarComunicacaoSegmentoDto
        {
            Nome = " Visitantes ativos ",
            Descricao = " Base connect ",
            PublicoAlvo = " visitantes "
        });

        result.Id.Should().Be(14);
        result.Nome.Should().Be("Visitantes ativos");
        result.PublicoAlvo.Should().Be("visitantes");
        result.Ativo.Should().BeTrue();
    }
}
