using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Extensions;
using Rhinero.Shouter.Shared.IBuses;

namespace Rhinero.Shouter.Client
{
    internal sealed class ShouterService : IShouter
    {
        private readonly IServiceProvider _provider;

        public ShouterService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<Guid> ShoutAsync(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default) =>
            await ShoutMessage(bus, protocol, payload, cancellationToken);

        public Guid Shout(Buses bus, Protocol protocol, object payload) =>
            ShoutMessage(bus, protocol, payload, CancellationToken.None).GetAwaiter().GetResult();

        private async Task<Guid> ShoutMessage(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default)
        {
            ShouterInterfaceChecker.CheckShouterMessageInterface(payload);
            ShouterInterfaceChecker.CheckProtocolAndMessage(protocol, payload);

            if (cancellationToken == default)
                cancellationToken = new CancellationTokenSource(Constants.CancellationTokenTimeSpan).Token;

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

        private async Task PublishToRabbitMQ(ShouterMessage message, CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();

            var endpoint =
                scope.ServiceProvider.GetRequiredService<Bind<IShouterRabbitMQBus, IPublishEndpoint>>();

            await endpoint.Value.Publish(message, cancellationToken);
        }

        private async Task ProduceToKafka(ShouterMessage message, CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();

            var endpoint =
                scope.ServiceProvider.GetRequiredService<Bind<IShouterKafkaBus, ITopicProducer<ShouterMessage>>>();

            await endpoint.Value.Produce(message, cancellationToken);
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