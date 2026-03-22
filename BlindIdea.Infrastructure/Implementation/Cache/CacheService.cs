using BlindIdea.Domain.Abstraction.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Implementation.Cache
{
    public class CacheService:ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IMemoryCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> fetchFromDb,
            TimeSpan? duration = null)
        {
            if (_cache.TryGetValue(key, out T? cached))
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
                return cached!;
            }

            _logger.LogDebug("Cache MISS: {Key}", key);

            var result = await fetchFromDb();

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    duration ?? TimeSpan.FromMinutes(5),
                SlidingExpiration = null,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, result, options);

            return result;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache REMOVED: {Key}", key);
        }

        public void RemoveMany(params string[] keys)
        {
            foreach (var key in keys)
                Remove(key);
        }

        public bool Exists(string key)
            => _cache.TryGetValue(key, out _);

        public void Set<T>(string key, T value, TimeSpan? duration = null)
        {
            _cache.Set(
                key,
                value,
                duration ?? TimeSpan.FromMinutes(5)
            );
            _logger.LogDebug("Cache SET: {Key}", key);
        }

    }
}
