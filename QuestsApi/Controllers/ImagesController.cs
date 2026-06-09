using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestsApi.Services;

namespace QuestsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
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

        using var stream = file.OpenReadStream();
        var url = await _imageService.UploadAsync(stream, file.FileName);

        return Ok(new { url, fileName = file.FileName });
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
        var mimeType = request.Data[5..dataIndex];

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Any(t => mimeType.Contains(t)))
            return BadRequest(new { error = "Допустимы только изображения (JPEG, PNG, GIF, WebP)." });

        var url = await _imageService.UploadBase64Async(base64Data, mimeType);

        return Ok(new { url, fileName = "upload" });
    }
}

public record Base64UploadRequest(string Data);
