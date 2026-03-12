using StackExchange.Redis;
using System.Text.Json;

namespace Ntigra.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _cache = connectionMultiplexer.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            _logger.LogDebug("Attempting to get from cache: {Key}", key);
            
            var value = await _cache.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache MISS for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache HIT for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting from cache: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            _logger.LogDebug("Setting cache for key: {Key}, expires in {Expiration} seconds", 
                key, expiration.TotalSeconds);
            
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _logger.LogDebug("Removing cache for key: {Key}", key);
            await _cache.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _cache.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache for key: {Key}", key);
            return false;
        }
    }
}
