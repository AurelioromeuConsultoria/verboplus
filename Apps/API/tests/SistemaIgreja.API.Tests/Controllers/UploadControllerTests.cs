using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using System.Net;
using System.Text.Json;
using System.Text;

namespace SistemaIgreja.API.Tests.Controllers;

public class UploadControllerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<UploadController>> _loggerMock = new();
    private readonly IConfiguration _configuration;
    private readonly ITenantContext _tenantContext = new DefaultTenantContext();
    private readonly UploadController _controller;

    public UploadControllerTests()
    {
        StubSave(_fileStorageMock);
        _configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _controller = new UploadController(
            _fileStorageMock.Object,
            _httpClientFactoryMock.Object,
            _configuration,
            _tenantContext,
            _loggerMock.Object);
    }

    // Faz o storage devolver um caminho previsível (sem tocar disco). IsLocalStorage fica false por padrão,
    // então o fluxo de sincronização dev->prod é pulado nos testes que não o exercitam.
    private static void StubSave(Mock<IFileStorageService> mock)
    {
        mock.Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Stream _, string _, string folder, string fileName, string _) => $"/uploads/{folder}/{fileName}");
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
            _fileStorageMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _tenantContext,
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
            _fileStorageMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _tenantContext,
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
            _fileStorageMock.Object,
            factory.Object,
            _configuration,
            _tenantContext,
            _loggerMock.Object);

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.png"
        });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadVideo_ReturnsBadRequest_WhenExtensionIsNotAllowed()
    {
        var file = CreateFormFile("video.txt", "conteudo");

        var result = await _controller.UploadVideo(file);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAudio_ReturnsBadRequest_WhenExtensionIsNotAllowed()
    {
        var file = CreateFormFile("audio.txt", "conteudo");

        var result = await _controller.UploadAudio(file);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAudio_ReturnsOk_WhenExtensionIsAllowed()
    {
        var file = CreateFormFile("audio.mp3", "conteudo");

        var result = await _controller.UploadAudio(file);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadFile_ReturnsBadRequest_WhenFileIsTooLarge()
    {
        var bytes = new byte[10];
        var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, 500L * 1024 * 1024 + 1, "file", "arquivo.bin");

        var result = await _controller.UploadFile(file);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsBadRequest_WhenDownloadedImageIsEmpty()
    {
        var controller = CreateControllerWithHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Array.Empty<byte>())
            {
                Headers =
                {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png")
                }
            }
        });

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.png"
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        JsonSerializer.Serialize(badRequest.Value).Should().Contain("Imagem vazia");
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsBadRequest_WhenDownloadedImageIsTooLarge()
    {
        var controller = CreateControllerWithHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[5 * 1024 * 1024 + 1])
            {
                Headers =
                {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg")
                }
            }
        });

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.jpg"
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        JsonSerializer.Serialize(badRequest.Value).Should().Contain("Imagem muito grande");
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsBadRequest_WhenDownloadFails()
    {
        var controller = CreateControllerWithHttpClient(_ => throw new HttpRequestException("falha externa"));

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.png"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImageFromUrl_FallsBackToJpg_WhenContentTypeIsUnsupported()
    {
        var controller = CreateControllerWithHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image"))
            {
                Headers =
                {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                }
            }
        });

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem-sem-extensao"
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        JsonSerializer.Serialize(ok.Value).Should().Contain(".jpg");
    }

    [Fact]
    public async Task UploadImageFromUrl_UsesExtensionFromUrl_WhenContentTypeIsMissing()
    {
        var controller = CreateControllerWithHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image"))
        });

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.webp"
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        JsonSerializer.Serialize(ok.Value).Should().Contain(".webp");
    }

    [Fact]
    public async Task SyncImage_ReturnsBadRequest_WhenFileIsMissingEvenWithValidApiKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductionUploadSync:ApiKey"] = "segredo"
            })
            .Build();
        var controller = new UploadController(
            _fileStorageMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _tenantContext,
            _loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["X-Sync-Api-Key"] = "segredo";

        var result = await controller.SyncImage(null!, null);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImageFromUrl_ReturnsOk_WhenProductionSyncFailsAfterLocalSave()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductionUploadSync:BaseUrl"] = "https://api.example.com",
                ["ProductionUploadSync:ApiKey"] = "segredo"
            })
            .Build();
        var httpClients = new Queue<HttpClient>(new[]
        {
            new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image"))
                {
                    Headers =
                    {
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png")
                    }
                }
            })),
            new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)))
        });
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => httpClients.Dequeue());

        // Storage local: o controller tenta sincronizar para produção; o sync falha (500) e ele cai no path local.
        var fileStorageMock = new Mock<IFileStorageService>();
        StubSave(fileStorageMock);
        fileStorageMock.SetupGet(s => s.IsLocalStorage).Returns(true);

        var controller = new UploadController(
            fileStorageMock.Object,
            factory.Object,
            configuration,
            _tenantContext,
            _loggerMock.Object);

        var result = await controller.UploadImageFromUrl(new UploadImageFromUrlRequest
        {
            Url = "https://cdn.example.com/imagem.png"
        });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SyncImage_ReplacesUnsafeFileName_WhenProvidedNameContainsPathTraversal()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductionUploadSync:ApiKey"] = "segredo"
            })
            .Build();
        var controller = new UploadController(
            _fileStorageMock.Object,
            _httpClientFactoryMock.Object,
            configuration,
            _tenantContext,
            _loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["X-Sync-Api-Key"] = "segredo";

        var result = await controller.SyncImage(CreateFormFile("imagem.png", "conteudo"), "../fora.png");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = JsonSerializer.Serialize(ok.Value);
        payload.Should().Contain(".png");
        payload.Should().NotContain("../fora.png");
    }

    private UploadController CreateControllerWithHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new StubHttpMessageHandler(responder)));

        return new UploadController(
            _fileStorageMock.Object,
            factory.Object,
            _configuration,
            _tenantContext,
            _loggerMock.Object);
    }

    private static IFormFile CreateFormFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
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
