using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class NoticiaServiceTests
{
    private readonly Mock<INoticiaRepository> _repositoryMock = new();
    private readonly Mock<ICategoriaNoticiaRepository> _categoriaRepositoryMock = new();
    private readonly NoticiaService _service;

    public NoticiaServiceTests()
    {
        _service = new NoticiaService(_repositoryMock.Object, _categoriaRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoriaDoesNotExist()
    {
        _categoriaRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((CategoriaNoticia?)null);

        var act = () => _service.CreateAsync(new CriarNoticiaDto
        {
            Titulo = "Nova noticia",
            Data = new DateTime(2026, 4, 6),
            CategoriaNoticiaId = 4
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoria não encontrada");
    }

    [Fact]
    public async Task CreateAsync_MapsDtoAndReturnsCreatedDto()
    {
        _categoriaRepositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new CategoriaNoticia { Id = 2, Nome = "Portal" });
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Noticia>()))
            .ReturnsAsync((Noticia noticia) =>
            {
                noticia.Id = 7;
                noticia.CategoriaNoticia = new CategoriaNoticia { Id = noticia.CategoriaNoticiaId, Nome = "Portal" };
                return noticia;
            });

        var result = await _service.CreateAsync(new CriarNoticiaDto
        {
            Titulo = "Novo destaque",
            Descricao = "Resumo",
            Texto = "Conteudo",
            Data = new DateTime(2026, 4, 6),
            Url = "https://example.com/noticia",
            Imagem = "/img.png",
            CategoriaNoticiaId = 2
        });

        result.Id.Should().Be(7);
        result.Titulo.Should().Be("Novo destaque");
        result.CategoriaNoticiaId.Should().Be(2);
        result.CategoriaNoticiaNome.Should().Be("Portal");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNoticiaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((Noticia?)null);

        var act = () => _service.UpdateAsync(9, new AtualizarNoticiaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Notícia não encontrada");
    }
}
