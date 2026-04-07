using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoEntregaServiceTests
{
    private readonly Mock<IComunicacaoEntregaRepository> _repositoryMock = new();
    private readonly Mock<ILogger<ComunicacaoEntregaService>> _loggerMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly ComunicacaoEntregaService _service;

    public ComunicacaoEntregaServiceTests()
    {
        _service = new ComunicacaoEntregaService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _auditLogServiceMock.Object);
    }

    [Fact]
    public async Task GetPagedAsync_NormalizesPaginationValues()
    {
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<ComunicacaoEntregaPagedQueryDto>()))
            .ReturnsAsync((new List<ComunicacaoEntrega>(), 0));

        var result = await _service.GetPagedAsync(new ComunicacaoEntregaPagedQueryDto
        {
            Page = 0,
            PageSize = 999
        });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(200);
    }

    [Fact]
    public async Task MarcarComoEnviadaAsync_Throws_WhenEntregaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((ComunicacaoEntrega?)null);

        var act = () => _service.MarcarComoEnviadaAsync(2);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Entrega não encontrada");
    }

    [Fact]
    public async Task MarcarComoFalhaAsync_UpdatesStatusAndWritesAudit()
    {
        var entrega = new ComunicacaoEntrega
        {
            Id = 5,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "pessoa@example.com",
            Status = StatusComunicacaoEntrega.Pendente
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entrega);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega updated) => updated);

        await _service.MarcarComoFalhaAsync(5, "SMTP indisponivel");

        entrega.Status.Should().Be(StatusComunicacaoEntrega.Falhou);
        entrega.Erro.Should().Be("SMTP indisponivel");
        entrega.Tentativas.Should().Be(1);
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("ComunicacaoEntrega", "5", "Falha", It.IsAny<object>()),
            Times.Once);
    }
}
