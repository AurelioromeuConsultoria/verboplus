using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.API.Controllers;
using System.Net;
using System.Text;

namespace SistemaIgreja.API.Tests.Controllers;

public class UploadControllerTests
{
    private readonly Mock<IWebHostEnvironment> _environmentMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<UploadController>> _loggerMock = new();
    private readonly IConfiguration _configuration;
    private readonly UploadController _controller;

    public UploadControllerTests()
    {
        _environmentMock.SetupGet(x => x.ContentRootPath).Returns(Path.GetTempPath());
        _configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _controller = new UploadController(
            _environmentMock.Object,
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WhenFileIsMissing()
    {
        var result = await _controller.UploadImage(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WhenExtensionIsNotAllowed()
    {
        var file = CreateFormFile("arquivo.txt", "conteudo");

        var result = await _controller.UploadImage(file);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsBadRequest_WhenUrlIsMissing()
    {
        var result = await _controller.UploadImageFromUrl(new UploadImageFromUrlRequest { Url = "" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SyncImage_ReturnsUnauthorized_WhenApiKeyIsInvalid()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductionUploadSync:ApiKey"] = "segredo"
            })
            .Build();
        var controller = new UploadController(
            _environmentMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var file = CreateFormFile("imagem.png", "conteudo");

        var result = await controller.SyncImage(file, null);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task SyncImage_ReturnsOk_WhenApiKeyIsValid()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductionUploadSync:ApiKey"] = "segredo"
            })
            .Build();
        var controller = new UploadController(
            _environmentMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["X-Sync-Api-Key"] = "segredo";

        var file = CreateFormFile("imagem.png", "conteudo");

        var result = await controller.SyncImage(file, "arquivo-final.png");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsOk_WhenDownloadSucceeds()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image"))
                {
                    Headers =
                    {
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png")
                    }
                }
            })));

        var controller = new UploadController(
            _environmentMock.Object,
            factory.Object,
            _configuration,
            _loggerMock.Object);

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.png"
        });

        result.Should().BeOfType<OkObjectResult>();
    }

    private static IFormFile CreateFormFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
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
