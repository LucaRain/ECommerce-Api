namespace ECommerce.Application.Interfaces;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string value, T data, TimeSpan? expiry = null);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task DeleteByPatternAsync(string pattern);
}
