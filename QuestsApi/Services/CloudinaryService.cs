using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace QuestsApi.Services;

public interface IImageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName);
    Task<string> UploadBase64Async(string base64Data, string mimeType);
}

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary CloudName is not configured");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary ApiKey is not configured");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary ApiSecret is not configured");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "quest_platform"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary upload error: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task<string> UploadBase64Async(string base64Data, string mimeType)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription($"upload.{GetExtension(mimeType)}", new MemoryStream(Convert.FromBase64String(base64Data))),
            Folder = "quest_platform"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary upload error: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    private static string GetExtension(string mimeType) => mimeType switch
    {
        var m when m.Contains("jpeg") => "jpg",
        var m when m.Contains("png") => "png",
        var m when m.Contains("gif") => "gif",
        var m when m.Contains("webp") => "webp",
        var m when m.Contains("svg") => "svg",
        _ => "png"
    };
}
