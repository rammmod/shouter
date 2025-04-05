using Rhinero.Shouter.Extensions;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Contracts;
using MassTransit;

namespace Rhinero.Shouter.Consumers
{
    public class ShouterConsumer : IConsumer<ShouterEvent>
    {
        private readonly ILogger<ShouterConsumer> _logger;
        private readonly ICallbackService _callback;

        public ShouterConsumer(ILogger<ShouterConsumer> logger, ICallbackService callback)
        {
            _logger = logger;
            _callback = callback;
        }

        public async Task Consume(ConsumeContext<ShouterEvent> context)
        {
            try
            {
                await _callback.Send(context.Message);
            }
            catch (Exception ex)
            {
                _logger.LogConsumeError(nameof(ShouterEvent), context.Message.CorrelationId, ex);
                throw;
            }
        }
    }
}
