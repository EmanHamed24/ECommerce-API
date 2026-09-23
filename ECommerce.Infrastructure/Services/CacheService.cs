using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public Task<string?> GetAsync(string key)
    {
        return _cache.GetStringAsync(key);
    }

    public Task SetAsync(
        string key,
        string value,
        TimeSpan expiration)
    {
        return _cache.SetStringAsync(
            key,
            value,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
    }

    public Task RemoveAsync(string key)
    {
        return _cache.RemoveAsync(key);
    }
}
