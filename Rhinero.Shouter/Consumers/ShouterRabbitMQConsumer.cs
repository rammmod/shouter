using MassTransit;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterRabbitMQConsumer : IConsumer<ShouterMessage>
    {
        private readonly ILogger<ShouterRabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShouterRabbitMQConsumer(
            ILogger<ShouterRabbitMQConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<ShouterMessage> context)
        {
            _logger.LogInformation("Processing RabbitMQ message: {@message}", context.Message);

            var service = _serviceProvider.GetRequiredKeyedService<ICallbackService>(context.Message.Protocol);

            await service.SendAsync(context.Message, context.CancellationToken);
        }
    }
}
