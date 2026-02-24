using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/admin/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 500 * 1024 * 1024; // 500MB

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        return await UploadFile(file, "images", new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" });
    }

    [HttpPost("videos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
        return await UploadFile(file, "videos", new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi" });
    }

    [HttpPost("audios")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAudio([FromForm] IFormFile file)
    {
        return await UploadFile(file, "audios", new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac" });
    }

    [HttpPost("files")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        return await UploadFile(file, "files", null);
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
