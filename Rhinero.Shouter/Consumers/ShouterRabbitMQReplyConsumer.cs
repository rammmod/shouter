using MassTransit;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterRabbitMQReplyConsumer : IConsumer<ShouterRequestMessage>
    {
        private readonly ILogger<ShouterRabbitMQReplyConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShouterRabbitMQReplyConsumer(
            ILogger<ShouterRabbitMQReplyConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<ShouterRequestMessage> context)
        {
            _logger.LogInformation("Processing RabbitMQ message: {@message}", context.Message);

            var service = _serviceProvider.GetRequiredKeyedService<ICallbackService>(context.Message.Protocol);

            var payload = await service.ReplyAsync(context.Message, context.CancellationToken);

            var replyMessage = new ShouterReplyMessage()
            {
                CorrelationId = context.Message.CorrelationId,
                Payload = payload.ToJson()
            };

            await context.RespondAsync(replyMessage);
        }
    }
}
