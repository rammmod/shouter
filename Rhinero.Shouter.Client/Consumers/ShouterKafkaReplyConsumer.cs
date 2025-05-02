using MassTransit;
using Rhinero.Shouter.Client.Redis;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Client.Consumers
{
    public class ShouterKafkaReplyConsumer : IConsumer<ShouterReplyMessage>
    {
        private readonly IRedisStorage _redisStorage;

        public ShouterKafkaReplyConsumer(IRedisStorage redisStorage)
        {
            _redisStorage = redisStorage;
        }

        public async Task Consume(ConsumeContext<ShouterReplyMessage> context) =>
            await _redisStorage.SetAsync(
                context.Message.CorrelationId.ToString(),
                context.Message?.ToJson(),
                TimeSpan.FromSeconds(Constants.Redis.Lifetime));
    }
}
