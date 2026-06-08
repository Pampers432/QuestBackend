using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuestsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ImagesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Файл не выбран или пуст." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Допустимы только изображения (JPEG, PNG, GIF, WebP, SVG)." });

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { url = $"/uploads/{fileName}", fileName });
    }

    [HttpPost("upload-base64")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadBase64([FromBody] Base64UploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Data))
            return BadRequest(new { error = "Данные изображения не предоставлены." });

        var dataIndex = request.Data.IndexOf(',');
        if (dataIndex < 0 || dataIndex + 1 >= request.Data.Length)
            return BadRequest(new { error = "Неверный формат Base64." });

        var base64Data = request.Data[(dataIndex + 1)..];
        var mimeType = request.Data[5..dataIndex]; // data:image/png;base64 -> image/png

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Any(t => mimeType.Contains(t)))
            return BadRequest(new { error = "Допустимы только изображения (JPEG, PNG, GIF, WebP)." });

        var ext = mimeType switch
        {
            var m when m.Contains("jpeg") => ".jpg",
            var m when m.Contains("png") => ".png",
            var m when m.Contains("gif") => ".gif",
            var m when m.Contains("webp") => ".webp",
            _ => ".png"
        };

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        var bytes = Convert.FromBase64String(base64Data);
        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

        return Ok(new { url = $"/uploads/{fileName}", fileName });
    }
}

public record Base64UploadRequest(string Data);
