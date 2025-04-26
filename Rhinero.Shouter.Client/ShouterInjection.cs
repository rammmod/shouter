using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Rhinero.Shouter.Client.Configuration;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions.Shouter;
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

                    x.AddRequestClient<ShouterRequestMessage>(new Uri(Shared.Constants.MassTransit.Queue + shouterRabbitMQConfiguration.ReplyQueue));

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
                                shouterRabbitMQConfiguration.Exchange,
                                shouterRabbitMQConfiguration.PublishQueue,
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
                    x.UsingInMemory();

                    x.AddDelayedMessageScheduler();

                    x.AddRequestClient<ShouterRequestMessage>(new Uri(Shared.Constants.MassTransit.Topic + shouterRabbitMQConfiguration.ReplyQueue));

                    x.AddRider(r =>
                    {
                        r.AddProducer<ShouterMessage>(shouterKafkaConfiguration.Topic);

                        r.UsingKafka((context, k) =>
                        {
                            k.Host(shouterKafkaConfiguration.BootstrapServers, h =>
                            {
                                h.UseSasl(s =>
                                {
                                    //s.Username = "your-username"; //TODO: add to config
                                    //s.Password = "your-password";
                                    s.Mechanism = SaslMechanism.Plain;
                                });

                                h.UseSsl(s =>
                                {
                                    s.EnableSslCertificateVerification = false; //TODO: add to config
                                });
                            });
                        });
                    });
                });
            }

            return services;
        }
    }
}
