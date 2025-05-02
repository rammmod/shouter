using Confluent.Kafka;
using MassTransit;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;
using System.Threading;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterKafkaRequestConsumer : IConsumer<ShouterRequestMessage>
    {
        private readonly ILogger<ShouterKafkaRequestConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShouterKafkaRequestConsumer(
            ILogger<ShouterKafkaRequestConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<ShouterRequestMessage> context)
        {
            _logger.LogInformation("Processing Kafka message: {@message}", context.Message);

            var service = _serviceProvider.GetRequiredKeyedService<ICallbackService>(context.Message.Protocol);

            var topicProducer =
                _serviceProvider.GetRequiredService<ITopicProducer<ShouterReplyMessage>>();

            object payload = null;
            string error = null;

            try
            {
                payload = await service.ReplyAsync(context.Message, context.CancellationToken);
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }

            var replyMessage = new ShouterReplyMessage()
            {
                CorrelationId = context.Message.CorrelationId,
                Payload = payload?.ToJson(),
                Error = error
            };

            await topicProducer.Produce(replyMessage, context.CancellationToken);
        }
    }
}
