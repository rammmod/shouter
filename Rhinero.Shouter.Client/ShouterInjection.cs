using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions;
using Rhinero.Shouter.Shared.IBuses;
using ShouterConfiguration = Rhinero.Shouter.Client.Configuration.Shouter;

namespace Rhinero.Shouter.Client
{
    public static class ShouterInjection
    {
        public static IServiceCollection AddShouter(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IShouter, ShouterService>();

            var shouterRabbitMQConfiguration =
                configuration.GetSection(nameof(ShouterConfiguration)).Get<ShouterConfiguration>()?.RabbitMQ;

            var shouterKafkaConfiguration =
                configuration.GetSection(nameof(ShouterConfiguration)).Get<ShouterConfiguration>()?.Kafka;

            if (shouterRabbitMQConfiguration is null && shouterKafkaConfiguration is null)
                throw new ShouterBusConfigurationException();

            if (shouterRabbitMQConfiguration is not null)
            {
                services.AddMassTransit<IShouterRabbitMQBus>(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UseMessageRetry(r => r.Interval(10, 1)); //TODO: make configurable - move to consumer api
                        cfg.UseInMemoryOutbox();

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
                                BuildQueueName(typeof(ShouterMessage).Namespace, nameof(ShouterMessage)),
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
                        r.AddProducer<ShouterMessage>(BuildQueueName(typeof(ShouterMessage).Namespace, nameof(ShouterMessage)));

                        r.UsingKafka((context, k) =>
                        {
                            k.Host(shouterKafkaConfiguration.BootstrapServers);
                        });
                    });
                });
            }

            return services;
        }

        private static string BuildQueueName(string @namespace, string contractName)
        {
            return @namespace + ":" + contractName;
        }
    }
}
