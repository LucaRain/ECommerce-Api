namespace ECommerce.Application.Interfaces;

public interface IFileService
{
    Task<string> SaveImageAsync(Stream fileStream, string fileName);
    void DeleteImage(string imageUrl);
}
