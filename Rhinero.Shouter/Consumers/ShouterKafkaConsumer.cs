using MassTransit;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterKafkaConsumer : IConsumer<ShouterMessage>
    {
        private readonly ILogger<ShouterKafkaConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShouterKafkaConsumer(
            ILogger<ShouterKafkaConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Consume(ConsumeContext<ShouterMessage> context)
        {
            var service = _serviceProvider.GetRequiredKeyedService<ICallbackService>(context.Message.Protocol);

            await service.SendAsync(context.Message);

            //try
            //{
            //    await _callback.Send(context.Message);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogConsumeError(nameof(ShouterMessage), context.Message.CorrelationId, ex);
            //    throw;
            //} //TODO: change
        }
    }
}
