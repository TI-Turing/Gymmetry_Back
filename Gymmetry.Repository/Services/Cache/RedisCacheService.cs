using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Repository.Services.Cache
{
    public interface IRedisCacheService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
        
        // Métodos genéricos agregados para PostShare
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<long> IncrementAsync(string key, TimeSpan? expiry = null);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(string connectionString, ILogger<RedisCacheService> logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Redis connection string cannot be null or empty.", nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            try
            {
                var redis = ConnectionMultiplexer.Connect(connectionString);
                _db = redis.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to connect to Redis. Redis will be disabled for this session.");
                _db = null!; // Mark as unavailable
            }
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping SetAsync for key {Key}", key); return; }
            try
            {
                await _db.StringSetAsync(key, value, expiry).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis for key {Key}. Continuing without cache.", key);
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping GetAsync for key {Key}", key); return null; }
            try
            {
                var value = await _db.StringGetAsync(key).ConfigureAwait(false);
                return value.HasValue ? value.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis for key {Key}. Continuing without cache.", key);
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping RemoveAsync for key {Key}", key); return; }
            try
            {
                await _db.KeyDeleteAsync(key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key from Redis: {Key}. Continuing without cache.", key);
            }
        }

        // Métodos genéricos agregados para PostShare
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null) return;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping SetAsync<T> for key {Key}", key); return; }
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, expiry).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting generic value in Redis for key {Key}. Continuing without cache.", key);
            }
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping GetAsync<T> for key {Key}", key); return null; }
            try
            {
                var value = await _db.StringGetAsync(key).ConfigureAwait(false);
                if (!value.HasValue) return null;
                
                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting generic value from Redis for key {Key}. Continuing without cache.", key);
                return null;
            }
        }

        public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return 0;
            if (_db == null) { _logger.LogWarning("Redis unavailable. Skipping IncrementAsync for key {Key}", key); return 0; }
            try
            {
                var value = await _db.StringIncrementAsync(key).ConfigureAwait(false);
                
                // Set expiry if provided and this is the first increment
                if (expiry.HasValue && value == 1)
                {
                    await _db.KeyExpireAsync(key, expiry.Value).ConfigureAwait(false);
                }
                
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing value in Redis for key {Key}. Continuing without cache.", key);
                return 0;
            }
        }
    }
}
