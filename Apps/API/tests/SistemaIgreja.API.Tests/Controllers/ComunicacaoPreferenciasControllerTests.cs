using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoPreferenciasControllerTests
{
    private readonly Mock<IComunicacaoPreferenciaService> _serviceMock = new();
    private readonly ComunicacaoPreferenciasController _controller;

    public ComunicacaoPreferenciasControllerTests()
    {
        _controller = new ComunicacaoPreferenciasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetByPessoaId_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByPessoaIdAsync(7))
            .ReturnsAsync(new List<ComunicacaoPreferenciaResumoDto>());

        var result = await _controller.GetByPessoaId(7);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task Upsert_ReturnsOk()
    {
        var dto = new AtualizarComunicacaoPreferenciaDto
        {
            Status = StatusPreferenciaCanal.Bloqueado,
            OrigemConsentimento = "Portal"
        };
        _serviceMock.Setup(s => s.UpsertAsync(9, CanalComunicacao.WhatsApp, dto))
            .ReturnsAsync(new ComunicacaoPreferenciaResumoDto { PessoaId = 9, Canal = CanalComunicacao.WhatsApp });

        var result = await _controller.Upsert(9, CanalComunicacao.WhatsApp, dto);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetByPessoaId_ReturnsPreferencePayload()
    {
        var preferencias = new List<ComunicacaoPreferenciaResumoDto>
        {
            new() { PessoaId = 7, Canal = CanalComunicacao.Email }
        };
        _serviceMock.Setup(s => s.GetByPessoaIdAsync(7))
            .ReturnsAsync(preferencias);

        var result = await _controller.GetByPessoaId(7);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(preferencias);
    }

    [Fact]
    public async Task Upsert_ReturnsUpdatedPreferencePayload()
    {
        var dto = new AtualizarComunicacaoPreferenciaDto
        {
            Status = StatusPreferenciaCanal.Permitido,
            OrigemConsentimento = "Admin"
        };
        var updated = new ComunicacaoPreferenciaResumoDto { PessoaId = 12, Canal = CanalComunicacao.Email };
        _serviceMock.Setup(s => s.UpsertAsync(12, CanalComunicacao.Email, dto))
            .ReturnsAsync(updated);

        var result = await _controller.Upsert(12, CanalComunicacao.Email, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }
}
