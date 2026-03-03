using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/admin/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private const long MaxFileSize = 500 * 1024 * 1024; // 500MB
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public UploadController(IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        return await UploadFile(file, "images", new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" });
    }

    [HttpPost("videos")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        return await UploadFile(file, "videos", new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi" });
    }

    [HttpPost("audios")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        return await UploadFile(file, "audios", new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac" });
    }

    [HttpPost("files")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        return await UploadFile(file, "files", null);
    }

    /// <summary>
    /// Recebe imagem enviada por outro backend (sync server-to-server). Exige header X-Sync-Api-Key.
    /// </summary>
    [HttpPost("sync-image")]
    [Consumes("multipart/form-data")]
    [AllowAnonymous]
    public async Task<IActionResult> SyncImage(IFormFile file, [FromForm] string? fileName)
    {
        var key = Request.Headers["X-Sync-Api-Key"].FirstOrDefault();
        var expectedKey = _configuration["ProductionUploadSync:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || key != expectedKey)
            return Unauthorized(new { message = "Chave de sync inválida" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Nenhum arquivo enviado" });

        var name = !string.IsNullOrWhiteSpace(fileName) ? fileName.Trim() : $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        if (name.Contains(Path.DirectorySeparatorChar) || name.Contains('/'))
            name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var folder = "images";
        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsPath);
        var filePath = Path.Combine(uploadsPath, name);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var relativePath = $"/uploads/{folder}/{name}";
        return Ok(new { url = relativePath, path = relativePath, fileName = name });
    }

    /// <summary>
    /// Baixa uma imagem de uma URL externa (ex.: og:image de notícia) e salva nos uploads.
    /// Se ProductionUploadSync estiver configurado, envia também para a API de produção (para o portal exibir).
    /// Retorna o path relativo para usar no campo imagem da notícia.
    /// </summary>
    [HttpPost("image-from-url")]
    [Authorize]
    public async Task<IActionResult> UploadImageFromUrl([FromBody] UploadImageFromUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Url))
            return BadRequest(new { message = "URL da imagem é obrigatória" });

        var url = request.Url.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            var extension = GetExtensionFromContentType(contentType) ?? GetExtensionFromUrl(url) ?? ".jpg";
            if (!AllowedImageExtensions.Contains(extension.ToLowerInvariant()))
                extension = ".jpg";

            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0)
                return BadRequest(new { message = "Imagem vazia" });
            if (bytes.Length > 5 * 1024 * 1024) // 5MB
                return BadRequest(new { message = "Imagem muito grande (máx. 5MB)" });

            var folder = "images";
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            var relativePath = $"/uploads/{folder}/{fileName}";

            // Sync para produção (server-to-server), para a imagem aparecer no portal
            var syncBaseUrl = _configuration["ProductionUploadSync:BaseUrl"]?.Trim();
            var syncApiKey = _configuration["ProductionUploadSync:ApiKey"]?.Trim();
            if (!string.IsNullOrEmpty(syncBaseUrl) && !string.IsNullOrEmpty(syncApiKey))
            {
                try
                {
                    var syncClient = _httpClientFactory.CreateClient();
                    syncClient.Timeout = TimeSpan.FromSeconds(30);
                    using var content = new MultipartFormDataContent();
                    content.Add(new ByteArrayContent(bytes), "file", fileName);
                    content.Add(new StringContent(fileName), "fileName");
                    var syncRequest = new HttpRequestMessage(HttpMethod.Post, $"{syncBaseUrl.TrimEnd('/')}/api/admin/upload/sync-image");
                    syncRequest.Headers.Add("X-Sync-Api-Key", syncApiKey);
                    syncRequest.Content = content;
                    using var syncResponse = await syncClient.SendAsync(syncRequest);
                    if (!syncResponse.IsSuccessStatusCode)
                        throw new InvalidOperationException($"Sync produção retornou {(int)syncResponse.StatusCode}");
                }
                catch (Exception ex)
                {
                    // Log mas não falha a requisição; a imagem ficou salva localmente
                    Logger.LogWarning(ex, "Falha ao enviar imagem para produção (sync). Imagem salva apenas localmente.");
                }
            }

            return Ok(new { url = relativePath, path = relativePath, fileName });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { message = "Não foi possível baixar a imagem. Verifique a URL.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao salvar imagem", error = ex.Message });
        }
    }

    private static string? GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => null
        };
    }

    private static string? GetExtensionFromUrl(string url)
    {
        try
        {
            var path = new Uri(url).AbsolutePath;
            var ext = Path.GetExtension(path);
            return !string.IsNullOrEmpty(ext) && AllowedImageExtensions.Contains(ext.ToLowerInvariant()) ? ext : null;
        }
        catch { return null; }
    }

    private async Task<IActionResult> UploadFile(IFormFile file, string folder, string[]? allowedExtensions)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Nenhum arquivo enviado" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { message = $"Arquivo muito grande. Máximo: {MaxFileSize / (1024 * 1024)}MB" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (allowedExtensions != null && !allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"Tipo de arquivo não permitido. Extensões permitidas: {string.Join(", ", allowedExtensions)}" });
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retornar o caminho relativo que será usado para servir o arquivo
            var relativePath = $"/uploads/{folder}/{fileName}";
            
            return Ok(new { 
                url = relativePath,
                path = relativePath,
                fileName = fileName,
                size = file.Length
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao fazer upload do arquivo", error = ex.Message });
        }
    }
}

public class UploadImageFromUrlRequest
{
    public string Url { get; set; } = "";
}
