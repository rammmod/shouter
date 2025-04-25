using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Exceptions.Shouter;
using Rhinero.Shouter.Shared.Extensions;
using Rhinero.Shouter.Shared.IBuses;

namespace Rhinero.Shouter.Client
{
    internal sealed class ShouterService : IShouter
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ShouterService> _logger;

        public ShouterService(
            IServiceProvider provider,
            ILogger<ShouterService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task<Guid> ShoutAsync(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default) =>
            await ShoutMessage(bus, protocol, payload, cancellationToken);

        public Guid Shout(Buses bus, Protocol protocol, object payload) =>
            ShoutMessage(bus, protocol, payload, CancellationToken.None).GetAwaiter().GetResult();

        public async Task<string> ReplyAsync(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default) =>
            await ReplyMessage(bus, protocol, payload, cancellationToken);

        public string Reply(Buses bus, Protocol protocol, object payload) =>
            ReplyMessage(bus, protocol, payload, CancellationToken.None).GetAwaiter().GetResult();

        private async Task<Guid> ShoutMessage(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
                cancellationToken = new CancellationTokenSource(Constants.CancellationTokenTimeSpan).Token;

            CheckPayload(payload, protocol);

            var message = new ShouterMessage()
            {
                Protocol = MapProtocol(protocol),
                Payload = payload.ToJson()
            };

            switch (bus)
            {
                case Buses.RabbitMQ:
                    await PublishToRabbitMQ(message, cancellationToken);
                    break;
                case Buses.Kafka:
                    await ProduceToKafka(message, cancellationToken);
                    break;
            };

            return message.CorrelationId;
        }

        private async Task<string> ReplyMessage(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
                cancellationToken = new CancellationTokenSource(Constants.CancellationTokenTimeSpan).Token;

            CheckPayload(payload, protocol);

            var message = new ShouterRequestMessage()
            {
                Protocol = MapProtocol(protocol),
                Payload = payload.ToJson()
            };

            ShouterReplyMessage response = null;

            switch (bus)
            {
                case Buses.RabbitMQ:
                    response = await ReplyUsingRabbitMQ<ShouterReplyMessage>(message, cancellationToken);
                    break;
                //case Buses.Kafka:
                //    response = await ReplyUsingKafka<ShouterReplyMessage>(message, cancellationToken);
                //    break;
            };

            return response.Payload;
        }

        private async Task PublishToRabbitMQ(ShouterMessage message, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _provider.CreateScope();

                var publishEndpoint =
                    scope.ServiceProvider.GetRequiredService<Bind<IShouterRabbitMQBus, IPublishEndpoint>>();

                await publishEndpoint.Value.Publish(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogPublishError(nameof(ShouterMessage), message, ex);
                throw new RabbitMQPublishException();
            }
        }

        private async Task ProduceToKafka(ShouterMessage message, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _provider.CreateScope();

                var topicProducer =
                    scope.ServiceProvider.GetRequiredService<ITopicProducer<ShouterMessage>>();

                await topicProducer.Produce(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogPublishError(nameof(ShouterMessage), message, ex);
                throw new KafkaProduceException();
            }
        }

        private async Task<ShouterReplyMessage> ReplyUsingRabbitMQ<T>(ShouterRequestMessage message, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _provider.CreateScope();

                var bus = scope.ServiceProvider.GetRequiredService<IShouterRabbitMQBus>();
                
                var client = bus.CreateRequestClient<ShouterRequestMessage>();

                var response = await client.GetResponse<ShouterReplyMessage>(message, cancellationToken);

                return response.Message;
            }
            catch (Exception ex)
            {
                _logger.LogReplyError(nameof(ShouterRequestMessage), message, ex);
                throw new RabbitMQReplyException();
            }
        }

        private static void CheckPayload(object payload, Protocol protocol)
        {
            ShouterInterfaceChecker.CheckShouterMessageInterface(payload);
            ShouterInterfaceChecker.CheckProtocolAndMessage(protocol, payload);
            ShouterInterfaceChecker.CheckForHttpPayload(protocol, payload);
        }

        private static ProtocolEnum MapProtocol(Protocol protocol)
        {
            return protocol switch
            {
                Protocol.HTTP => ProtocolEnum.Http,
                Protocol.gRPC => ProtocolEnum.Grpc,
                _ => throw new NotImplementedException()
            };
        }

    }
}