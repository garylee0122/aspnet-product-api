using StackExchange.Redis;
using System.Text.Json;

namespace DemoAPI.Infrastructure.Services
{
    public class RedisQueueService
    {
        private readonly IDatabase _db;
        private const string QueueKey = "order_queue";

        public RedisQueueService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // 🔥 推入 Queue
        public async Task EnqueueAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            await _db.ListRightPushAsync(QueueKey, json);
        }

        // 🔥 取出 Queue（blocking）
        public async Task<string?> DequeueAsync()
        {
            var result = await _db.ListLeftPopAsync(QueueKey);
            return result;
        }

        public async Task<long> GetLengthAsync()
        {
            return await _db.ListLengthAsync(QueueKey);
        }
    }
}
