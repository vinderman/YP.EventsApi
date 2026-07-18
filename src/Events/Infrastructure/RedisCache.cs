using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure;

public class RedisCache: ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCache> _logger;
    public RedisCache(IConnectionMultiplexer connection, ILogger<RedisCache> logger)
    {
        _db = connection.GetDatabase();
        _logger = logger;
    }
    public async Task<T?> GetByKey<T>(string key) where T : class
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return null;
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}", key);
            return null;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis GET timeout for key {Key}", key);
            return null;
        }
    }
    public async Task Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            await _db.StringSetAsync(
                key,
                JsonSerializer.Serialize(value),
                expiration ?? TimeSpan.FromDays(1));
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }
    public async Task Delete(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
        {
            _logger.LogWarning(ex, "Redis DELETE failed for key {Key}", key);
        }
    }
}