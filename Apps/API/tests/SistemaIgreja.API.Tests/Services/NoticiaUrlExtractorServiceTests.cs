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
