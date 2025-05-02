using Rhinero.Shouter.Client.Configuration;
using StackExchange.Redis;

namespace Rhinero.Shouter.Client.Redis
{
    internal class RedisStorage : IRedisStorage
    {
        private readonly ShouterConfiguration _configuration;
        private readonly IDatabase _db;

        public RedisStorage(ShouterConfiguration configuration)
        {
            _configuration = configuration;
            var redis = ConnectionMultiplexer.Connect(configuration.Kafka.Redis.ConnectionString);
            _db = redis.GetDatabase(configuration.Kafka.Redis.CacheId);
        }

        public async Task<string> GetAsync(string key) =>
            (await _db.StringGetAsync(key)).ToString();

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null) =>
            await _db.StringSetAsync(key, value, expiry);
    }
}
