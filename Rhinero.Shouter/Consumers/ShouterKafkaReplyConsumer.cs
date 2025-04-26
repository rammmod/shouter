using MassTransit;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterKafkaReplyConsumer : IConsumer<ShouterRequestMessage>
    {
        private readonly ILogger<ShouterKafkaReplyConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShouterKafkaReplyConsumer(
            ILogger<ShouterKafkaReplyConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<ShouterRequestMessage> context)
        {
            _logger.LogInformation("Processing RabbitMQ message: {@message}", context.Message);

            var service = _serviceProvider.GetRequiredKeyedService<ICallbackService>(context.Message.Protocol);

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

            await context.RespondAsync(replyMessage);
        }
    }
}
