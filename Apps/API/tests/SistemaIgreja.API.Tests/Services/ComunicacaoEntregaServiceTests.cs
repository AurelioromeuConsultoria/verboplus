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
    private readonly Mock<IComunicacaoCampanhaRepository> _campanhaRepositoryMock = new();
    private readonly Mock<ILogger<ComunicacaoEntregaService>> _loggerMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly ComunicacaoEntregaService _service;

    public ComunicacaoEntregaServiceTests()
    {
        _service = new ComunicacaoEntregaService(
            _repositoryMock.Object,
            _campanhaRepositoryMock.Object,
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
            ComunicacaoCampanhaId = 12,
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
        _campanhaRepositoryMock.Verify(r => r.AtualizarStatusPorEntregasAsync(12), Times.Once);
    }

    [Fact]
    public async Task MarcarComoEnviadaAsync_AtualizaStatusDaCampanha()
    {
        var entrega = new ComunicacaoEntrega
        {
            Id = 6,
            ComunicacaoCampanhaId = 13,
            Canal = CanalComunicacao.WhatsApp,
            DestinoResolvido = "5511999999999",
            Status = StatusComunicacaoEntrega.Reservado
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(entrega);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega updated) => updated);

        await _service.MarcarComoEnviadaAsync(6);

        entrega.Status.Should().Be(StatusComunicacaoEntrega.Enviado);
        _campanhaRepositoryMock.Verify(r => r.AtualizarStatusPorEntregasAsync(13), Times.Once);
    }

    [Fact]
    public async Task PrepararReprocessamentoAsync_ReabreFalhaElegivel()
    {
        var entrega = new ComunicacaoEntrega
        {
            Id = 8,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "pessoa@example.com",
            Status = StatusComunicacaoEntrega.Falhou,
            Erro = "SMTP indisponivel",
            Tentativas = 2
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(entrega);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega updated) => updated);

        var result = await _service.PrepararReprocessamentoAsync(8);

        result.Status.Should().Be(StatusComunicacaoEntrega.Pendente);
        result.PodeReprocessar.Should().BeFalse();
        entrega.Status.Should().Be(StatusComunicacaoEntrega.Pendente);
        entrega.Erro.Should().BeNull();
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("ComunicacaoEntrega", "8", "ReprocessarEntrega", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task PrepararReprocessamentoAsync_BloqueiaFalhaNaoElegivel()
    {
        var entrega = new ComunicacaoEntrega
        {
            Id = 9,
            Canal = CanalComunicacao.WhatsApp,
            DestinoResolvido = "5511999999999",
            Status = StatusComunicacaoEntrega.Falhou,
            Erro = "Entrega bloqueada: Maria não possui WhatsApp válido."
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(entrega);

        var act = () => _service.PrepararReprocessamentoAsync(9);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Esta entrega não é elegível para reprocessamento.");
    }
}
