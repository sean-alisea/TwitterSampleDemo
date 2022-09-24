using System.Text.Json;
using StackExchange.Redis;
using TwitterSample.Models;

namespace TwitterSample.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly string _connectionString;
        private IDatabase? _redisDB;
        private const string CacheKey = "TwitterStreamStatistics";

        public IDatabase RedisDB { get
            {
                if (this._redisDB == null)
                {
                    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_connectionString);

                    this._redisDB = redis.GetDatabase();
                }

                return this._redisDB;
            }
        }

        public CacheService(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public async Task<TwitterStreamStatistics> ReadStatisticsAsync()
        {
            string? jsonString = await RedisDB.StringGetAsync(CacheKey);

            return JsonSerializer.Deserialize<TwitterStreamStatistics>(jsonString);
        }

        public async Task WriteStatisticsAsync(TwitterStreamStatistics statistics)
        {
            string jsonString = JsonSerializer.Serialize(statistics);

            await RedisDB.StringSetAsync(CacheKey, jsonString);
        }
    }
}
