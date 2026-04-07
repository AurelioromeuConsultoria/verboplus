using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoTemplateServiceTests
{
    private readonly Mock<IComunicacaoTemplateRepository> _repositoryMock = new();
    private readonly ComunicacaoTemplateService _service;

    public ComunicacaoTemplateServiceTests()
    {
        _service = new ComunicacaoTemplateService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_TrimsFieldsAndStartsAsDraft()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ComunicacaoTemplate>()))
            .ReturnsAsync((ComunicacaoTemplate template) =>
            {
                template.Id = 9;
                return template;
            });

        var result = await _service.CreateAsync(new CriarComunicacaoTemplateDto
        {
            Nome = "  Aviso semanal  ",
            Objetivo = "  Engajar  ",
            Canal = CanalComunicacao.Email,
            Assunto = "  Assunto  ",
            Corpo = "  Corpo  ",
            VariaveisPermitidas = "  nome,email  "
        });

        result.Id.Should().Be(9);
        result.Nome.Should().Be("Aviso semanal");
        result.Objetivo.Should().Be("Engajar");
        result.Assunto.Should().Be("Assunto");
        result.Corpo.Should().Be("Corpo");
        result.Status.Should().Be(StatusComunicacaoTemplate.Rascunho);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenTemplateDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((ComunicacaoTemplate?)null);

        var act = () => _service.UpdateAsync(4, new AtualizarComunicacaoTemplateDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Template não encontrado");
    }

    [Fact]
    public async Task UpdateAsync_IncrementsVersionAndUpdatesStatus()
    {
        var template = new ComunicacaoTemplate
        {
            Id = 7,
            Nome = "Inicial",
            Objetivo = "Objetivo",
            Canal = CanalComunicacao.WhatsApp,
            Corpo = "Corpo",
            VariaveisPermitidas = "nome",
            Status = StatusComunicacaoTemplate.Rascunho,
            Versao = 2
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(template);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ComunicacaoTemplate>()))
            .ReturnsAsync((ComunicacaoTemplate updated) => updated);

        var result = await _service.UpdateAsync(7, new AtualizarComunicacaoTemplateDto
        {
            Nome = " Atualizado ",
            Objetivo = " Conversao ",
            Corpo = " Conteudo ",
            VariaveisPermitidas = " nome,telefone ",
            Status = StatusComunicacaoTemplate.Ativo
        });

        result.Nome.Should().Be("Atualizado");
        result.Status.Should().Be(StatusComunicacaoTemplate.Ativo);
        result.Versao.Should().Be(3);
    }
}
