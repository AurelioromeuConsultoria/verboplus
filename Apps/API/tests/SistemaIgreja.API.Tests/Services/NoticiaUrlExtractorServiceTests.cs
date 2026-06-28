using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using SistemaIgreja.API.Services;

namespace SistemaIgreja.API.Tests.Services;

public class NoticiaUrlExtractorServiceTests
{
    [Fact]
    public async Task ExtrairAsync_ReturnsNull_WhenUrlIsEmpty()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.ExtrairAsync("");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExtrairAsync_ReturnsNull_WhenHttpRequestFails()
    {
        var service = CreateService(_ => throw new HttpRequestException("offline"));

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExtrairAsync_ExtractsTitleDescriptionTextAndImage()
    {
        const string html = """
            <html>
              <head>
                <title>Titulo longo da noticia</title>
                <meta property="og:title" content="Titulo curto" />
                <meta property="article:published_time" content="2026-04-06T10:30:00Z" />
                <meta property="og:image" content="https://cdn.example.com/img.png" />
              </head>
              <body>
                <article>
                  <div class="sub-title">Subtitulo principal da materia</div>
                  <div class="content-body">
                  <p>Primeiro paragrafo importante da noticia com bastante contexto para passar do minimo.</p>
                  <p>Segundo paragrafo complementar.</p>
                  </div>
                </article>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("example.com/noticia");

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Titulo longo da noticia");
        result.Descricao.Should().Contain("Subtitulo principal");
        result.ImagemUrl.Should().Be("https://cdn.example.com/img.png");
        result.Url.Should().Be("https://example.com/noticia");
        result.Data.Should().Be(new DateTime(2026, 4, 6, 10, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task ExtrairAsync_ReturnsNull_WhenHtmlIsEmpty()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExtrairAsync_PrefersLongerTitleTag_AndNormalizesUrlWithoutProtocol()
    {
        const string html = """
            <html>
              <head>
                <title>Titulo principal mais longo e completo da noticia</title>
                <meta property="og:title" content="Titulo curto" />
              </head>
              <body>
                <article>
                  <p>Primeiro paragrafo longo o bastante para ser tratado como descricao valida e texto de abertura da materia jornalistica.</p>
                </article>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("portal.exemplo.com/noticia");

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Titulo principal mais longo e completo da noticia");
        result.Url.Should().Be("https://portal.exemplo.com/noticia");
    }

    [Fact]
    public async Task ExtrairAsync_UsesBodyParagraph_WhenMetaDescriptionLooksLikeAuthor()
    {
        const string html = """
            <html>
              <head>
                <title>Noticia importante</title>
                <meta name="description" content="Redação CPAD News Website" />
              </head>
              <body>
                <div class="content-body">
                  <p>Este e o primeiro paragrafo util da noticia com contexto suficiente para virar descricao principal da materia sem depender de meta ruim.</p>
                  <p>Outro paragrafo complementar.</p>
                </div>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().NotBeNull();
        result!.Descricao.Should().Contain("primeiro paragrafo util da noticia");
        result.Descricao.Should().NotContain("Redação CPAD News Website");
    }

    [Fact]
    public async Task ExtrairAsync_UsesMetaDescription_WhenItIsLongAndValid()
    {
        const string html = """
            <html>
              <head>
                <title>Noticia sem subtitulo</title>
                <meta property="og:description" content="Descricao longa e valida da materia com informacoes suficientes para ser aproveitada como resumo principal desta noticia." />
              </head>
              <body>
                <div>Bloco curto.</div>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().NotBeNull();
        result!.Descricao.Should().StartWith("Descricao longa e valida da materia");
    }

    [Fact]
    public async Task ExtrairAsync_UsesFirstArticleParagraphAsDescription_WhenSubtitleIsMissing()
    {
        const string html = """
            <html>
              <head>
                <title>Materia sem subtitulo</title>
              </head>
              <body>
                <article>
                  <p>Primeiro bloco descritivo com tamanho suficiente para ser aproveitado como resumo principal da noticia.</p>
                  <p>Segundo bloco complementar.</p>
                </article>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().NotBeNull();
        result!.Descricao.Should().Contain("Primeiro bloco descritivo");
    }

    [Fact]
    public async Task ExtrairAsync_UsesLeadStartingWithSegundoA_WhenSpecificSubtitleClassDoesNotExist()
    {
        const string html = """
            <html>
              <head>
                <title>Noticia com lead textual</title>
              </head>
              <body>
                <div>Segundo a Portas Abertas, houve crescimento expressivo da igreja perseguida em regioes sensiveis ao redor do mundo.</div>
                <div class="content-body">
                  <p>Corpo principal da noticia com mais detalhes e contexto adicional para o leitor.</p>
                </div>
              </body>
            </html>
            """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        });

        var result = await service.ExtrairAsync("https://example.com/noticia");

        result.Should().NotBeNull();
        result!.Descricao.Should().StartWith("Segundo a Portas Abertas");
    }

    private static NoticiaUrlExtractorService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new StubHttpMessageHandler(responder)));

        return new NoticiaUrlExtractorService(factory.Object);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}
