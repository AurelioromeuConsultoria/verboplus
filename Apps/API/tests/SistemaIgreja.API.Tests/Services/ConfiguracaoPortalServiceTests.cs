using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ConfiguracaoPortalServiceTests
{
    private readonly Mock<IConfiguracaoPortalRepository> _repositoryMock = new();
    private readonly ConfiguracaoPortalService _service;

    public ConfiguracaoPortalServiceTests()
    {
        _service = new ConfiguracaoPortalService(_repositoryMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_UsesDefaultValue_WhenDtoIsInvalid()
    {
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ConfiguracaoPortal>()))
            .ReturnsAsync((ConfiguracaoPortal config) =>
            {
                config.Id = 1;
                return config;
            });

        var result = await _service.UpdateAsync(new AtualizarConfiguracaoPortalDto
        {
            TempoTransicaoCarrossel = 0
        });

        result.TempoTransicaoCarrossel.Should().Be(5);
    }
}
