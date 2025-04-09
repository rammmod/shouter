using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Rhinero.Shouter.Client.Configuration;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions;
using Rhinero.Shouter.Shared.IBuses;

namespace Rhinero.Shouter.Client
{
    public static class ShouterInjection
    {
        public static IServiceCollection AddShouter(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IShouter, ShouterService>();

            var shouterConfiguration =
                configuration.GetSection(nameof(ShouterConfiguration)).Get<ShouterConfiguration>();

            var shouterRabbitMQConfiguration = shouterConfiguration?.RabbitMQ;

            var shouterKafkaConfiguration = shouterConfiguration?.Kafka;

            if (shouterRabbitMQConfiguration is null && shouterKafkaConfiguration is null)
                throw new ShouterBusConfigurationException();

            if (shouterRabbitMQConfiguration is not null)
            {
                services.AddMassTransit<IShouterRabbitMQBus>(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(shouterRabbitMQConfiguration.Hostname, shouterRabbitMQConfiguration.Port, shouterRabbitMQConfiguration.VirtualHost, h =>
                        {
                            if (!string.IsNullOrWhiteSpace(shouterRabbitMQConfiguration.UserName))
                                h.Username(shouterRabbitMQConfiguration.UserName);

                            if (!string.IsNullOrWhiteSpace(shouterRabbitMQConfiguration.Password))
                                h.Password(shouterRabbitMQConfiguration.Password);
                        });

                        cfg.Publish(typeof(ShouterMessage), x =>
                        {
                            x.Durable = true;
                            x.ExchangeType = ExchangeType.Fanout;

                            x.BindQueue(
                                BuildExchangeName(typeof(ShouterMessage).Namespace, nameof(ShouterMessage)),
                                shouterRabbitMQConfiguration.Queue,
                                qc =>
                                {
                                    qc.SetQuorumQueue();
                                    qc.Durable = true;
                                    qc.ExchangeType = ExchangeType.Fanout;
                                });
                        });
                    });
                });
            }

            if (shouterKafkaConfiguration is not null)
            {
                services.AddMassTransit<IShouterKafkaBus>(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.AddDelayedMessageScheduler();

                    x.AddRider(r =>
                    {
                        r.AddProducer<ShouterMessage>(BuildExchangeName(typeof(ShouterMessage).Namespace, nameof(ShouterMessage)));

                        r.UsingKafka((context, k) =>
                        {
                            k.Host(shouterKafkaConfiguration.BootstrapServers);
                        });
                    });
                });
            }

            return services;
        }

        private static string BuildExchangeName(string @namespace, string contractName)
        {
            return @namespace + ":" + contractName;
        }
    }
}
