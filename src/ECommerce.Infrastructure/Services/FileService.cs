using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public FileService(IConfiguration config, IWebHostEnvironment env)
    {
        _uploadsPath = Path.Combine(env.WebRootPath, "images");
        _baseUrl = config["App:BaseUrl"]!;

        // create folder if it doesn't exist
        if (!Directory.Exists(_uploadsPath))
            Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> SaveImageAsync(Stream fileStream, string fileName)
    {
        // generate unique filename to avoid collisions
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadsPath, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream);

        return $"{_baseUrl}/images/{uniqueFileName}";
    }

    public void DeleteImage(string imageUrl)
    {
        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_uploadsPath, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
