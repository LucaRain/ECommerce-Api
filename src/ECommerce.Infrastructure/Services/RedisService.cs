using System.Text.Json;
using ECommerce.Application.Interfaces;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _connection;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        _connection = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(data);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromDays(30));
    }

    public async Task DeleteAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task DeleteByPatternAsync(string pattern)
    {
        var server = _connection.GetServer(_connection.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Any())
            await _db.KeyDeleteAsync(keys);
    }
}
